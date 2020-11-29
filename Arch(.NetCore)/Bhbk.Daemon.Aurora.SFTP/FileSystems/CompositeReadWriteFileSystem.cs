using Bhbk.Daemon.Aurora.SFTP.Helpers;
using Bhbk.Lib.Aurora.Data_EF6.Infrastructure;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Common.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebex.IO.FileSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Hashing = Bhbk.Lib.Cryptography.Hashing;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class CompositeReadWriteFileSystem : ReadWriteFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private User _userEntity;

        internal CompositeReadWriteFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, User userEntity)
            : base(settings)
        {
            _factory = factory;
            _userEntity = userEntity;

            using (var scope = _factory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                CompositeFileSystemHelper.EnsureRootExists(uow, _userEntity);
            }

            var folderKeysNode = new DirectoryNode(".ssh", Root);
            var fileKeysNode = new FileNode("authorized_users", folderKeysNode);

            if (!Exists(folderKeysNode.Path, NodeType.Directory))
                CreateDirectory(Root, folderKeysNode);

            if (Exists(fileKeysNode.Path, NodeType.File))
                Delete(fileKeysNode);

            var pubKeysContent = KeyHelper.ExportPubKeyBase64(_userEntity, _userEntity.PublicKeys);

            CreateFile(folderKeysNode, fileKeysNode);
            SaveContent(fileKeysNode, NodeContent.CreateDelayedWriteContent(
                new MemoryStream(Encoding.UTF8.GetBytes(pubKeysContent.ToString()))));
        }

        protected override DirectoryNode CreateDirectory(DirectoryNode parent, DirectoryNode child)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, parent.Path.StringPath);

                    uow.UserFolders.Create(
                        new UserFolder
                        {
                            IdentityId = _userEntity.IdentityId,
                            ParentId = folderEntity.Id,
                            VirtualName = child.Name,
                            IsReadOnly = false,
                        });
                    uow.Commit();

                    Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' directory '{child.Path}'");

                    return child;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override FileNode CreateFile(DirectoryNode parent, FileNode child)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            DirectoryInfo folder = null;
            FileInfo file = null;

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    _userEntity = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                        .Where(x => x.IdentityId == _userEntity.IdentityId).ToLambda())
                        .Single();

                    if (_userEntity.QuotaUsedInBytes >= _userEntity.QuotaInBytes)
                    {
                        Log.Warning($"'{callPath}' '{_userEntity.IdentityAlias}' file '{child.Path}' cancelled, " +
                            $"totoal quota '{_userEntity.QuotaInBytes}' used quota '{_userEntity.QuotaUsedInBytes}'");

                        throw new FileSystemOperationCanceledException();
                    }

                    var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, parent.Path.StringPath);
                    var folderHash = Strings.GetDirectoryHash($"{_userEntity.IdentityId}{parent.Path.StringPath}{child.Name}");
                    var fileName = Hashing.MD5.Create(Guid.NewGuid().ToString());

                    folder = new DirectoryInfo(conf["Storage:UnstructuredData"]
                        + Path.DirectorySeparatorChar + folderHash);

                    if (!folder.Exists)
                        folder.Create();

                    file = new FileInfo(conf["Storage:UnstructuredData"]
                        + Path.DirectorySeparatorChar + folderHash
                        + Path.DirectorySeparatorChar + fileName);

                    var fileEntity = new UserFile
                    {
                        IdentityId = _userEntity.IdentityId,
                        FolderId = folderEntity.Id,
                        VirtualName = child.Name,
                        RealPath = folderHash,
                        RealFileName = fileName,
                        IsReadOnly = false,
                    };

                    /*
                     * a zero size file will always be created first regardless of actual size of file. 
                     */

                    using (var sha256 = new SHA256Managed())
                    using (var fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        var hash = sha256.ComputeHash(fs);

                        fileEntity.RealFileSize = fs.Length;
                        fileEntity.HashSHA256 = Strings.GetHexString(hash);
                    }

                    uow.UserFiles.Create(fileEntity);
                    uow.Commit();

                    Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' empty file '{child.Path}' at '{file.FullName}'");

                    return child;
                }
            }
            catch (Exception ex) when (ex is DbUpdateException || ex is DbUpdateConcurrencyException)
            {
                if (file.Exists)
                    file.Delete();

                Log.Error(ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeBase Delete(NodeBase node)
        {
            if (!node.Exists())
                return node;

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                if (node.Attributes.IsReadOnly)
                    throw new FileSystemOperationCanceledException();

                using (var scope = _factory.CreateScope())
                {
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            {
                                var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, node.Path.StringPath);

                                var file = new FileInfo(conf["Storage:UnstructuredData"]
                                    + Path.DirectorySeparatorChar + fileEntity.RealPath
                                    + Path.DirectorySeparatorChar + fileEntity.RealFileName);

                                _userEntity = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                                    .Where(x => x.IdentityId == _userEntity.IdentityId).ToLambda())
                                    .Single();

                                _userEntity.QuotaUsedInBytes -= file.Length;

                                uow.UserFiles.Delete(fileEntity);
                                uow.Users.Update(_userEntity);
                                uow.Commit();

                                File.Delete(file.FullName);

                                Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' file '{node.Path}' at '{file.FullName}'");
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, node.Path.StringPath);

                                uow.UserFolders.Delete(folderEntity);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' folder '{node.Path}'");
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    return node;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override bool Exists(NodePath path, NodeType nodeType)
        {
            try
            {
                bool exists = false;

                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    switch (nodeType)
                    {
                        case NodeType.File:
                            {
                                var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, path.StringPath);

                                if (fileEntity != null)
                                    exists = true;
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, path.StringPath);

                                if (folderEntity != null)
                                    exists = true;
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    return exists;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeAttributes GetAttributes(NodeBase node)
        {
            if (!node.Exists())
                return node.Attributes;

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            {
                                var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, node.Path.StringPath);

                                if (fileEntity.IsReadOnly)
                                    node.SetAttributes(new NodeAttributes(FileAttributes.Normal | FileAttributes.ReadOnly));

                                node.SetAttributes(new NodeAttributes(FileAttributes.Normal));
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, node.Path.StringPath);

                                if (folderEntity.IsReadOnly)
                                    node.SetAttributes(new NodeAttributes(FileAttributes.Directory | FileAttributes.ReadOnly));

                                node.SetAttributes(new NodeAttributes(FileAttributes.Directory));
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    return node.Attributes;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeBase GetChild(string name, DirectoryNode parent)
        {
            try
            {
                NodeBase child = null;

                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var folderParentEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, parent.Path.StringPath);

                    var folderEntity = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolder>()
                        .Where(x => x.IdentityId == _userEntity.IdentityId && x.ParentId == folderParentEntity.Id && x.VirtualName == name).ToLambda())
                        .SingleOrDefault();

                    if (folderEntity != null)
                        child = new DirectoryNode(folderEntity.VirtualName, parent,
                            new NodeTimeInfo(folderEntity.CreatedUtc.UtcDateTime,
                                folderEntity.LastAccessedUtc.UtcDateTime, folderEntity.LastUpdatedUtc.UtcDateTime));

                    var fileEntity = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<UserFile>()
                        .Where(x => x.IdentityId == _userEntity.IdentityId && x.FolderId == folderParentEntity.Id && x.VirtualName == name).ToLambda())
                        .SingleOrDefault();

                    if (fileEntity != null)
                        child = new FileNode(fileEntity.VirtualName, parent,
                            new NodeTimeInfo(fileEntity.CreatedUtc.UtcDateTime,
                                fileEntity.LastAccessedUtc.UtcDateTime, fileEntity.LastUpdatedUtc.UtcDateTime));

                    return child;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override IEnumerable<NodeBase> GetChildren(DirectoryNode parent, NodeType nodeType)
        {
            if (!parent.Exists())
                return Enumerable.Empty<NodeBase>();

            try
            {
                var children = new List<NodeBase>();

                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var folderParentEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, parent.Path.StringPath);

                    _userEntity.Folders = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolder>()
                        .Where(x => x.IdentityId == _userEntity.IdentityId).ToLambda())
                        .ToList();

                    foreach (var folder in _userEntity.Folders.Where(x => x.IdentityId == _userEntity.IdentityId && x.ParentId == folderParentEntity.Id))
                        children.Add(new DirectoryNode(folder.VirtualName, parent,
                            new NodeTimeInfo(folder.CreatedUtc.UtcDateTime,
                                folder.LastAccessedUtc.UtcDateTime, folder.LastUpdatedUtc.UtcDateTime)));

                    _userEntity.Files = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<UserFile>()
                        .Where(x => x.IdentityId == _userEntity.IdentityId).ToLambda())
                        .ToList();

                    foreach (var file in _userEntity.Files.Where(x => x.IdentityId == _userEntity.IdentityId && x.FolderId == folderParentEntity.Id))
                        children.Add(new FileNode(file.VirtualName, parent,
                            new NodeTimeInfo(file.CreatedUtc.UtcDateTime,
                                file.LastAccessedUtc.UtcDateTime, file.LastUpdatedUtc.UtcDateTime)));

                    return children;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeContent GetContent(NodeBase node, NodeContentParameters contentParameters)
        {
            if (!node.Exists())
                return NodeContent.CreateDelayedWriteContent(new MemoryStream());

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                NodeContent content = null;

                using (var scope = _factory.CreateScope())
                {
                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            {
                                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                                var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, node.Path.StringPath);

                                var file = new FileInfo(conf["Storage:UnstructuredData"]
                                    + Path.DirectorySeparatorChar + fileEntity.RealPath
                                    + Path.DirectorySeparatorChar + fileEntity.RealFileName);

                                var stream = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

                                content = contentParameters.AccessType == NodeContentAccess.Read
                                    ? NodeContent.CreateReadOnlyContent(stream)
                                    : NodeContent.CreateDelayedWriteContent(stream);

                                Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' file '{node.Path}' from '{file.FullName}'");
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    return content;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override long GetLength(NodeBase node)
        {
            if (!node.Exists())
                return 0L;

            try
            {
                long length = 0L;

                using (var scope = _factory.CreateScope())
                {
                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            {
                                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                                var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, node.Path.StringPath);

                                length = fileEntity.RealFileSize;
                            }
                            break;

                        case NodeType.Directory:
                            {
                                length = 0L;
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    return length;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeTimeInfo GetTimeInfo(NodeBase node)
        {
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            {
                                var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, node.Path.StringPath);

                                node.SetTimeInfo(new NodeTimeInfo(fileEntity.CreatedUtc.UtcDateTime,
                                    fileEntity.LastAccessedUtc.UtcDateTime, fileEntity.LastUpdatedUtc.UtcDateTime));
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, node.Path.StringPath);

                                node.SetTimeInfo(new NodeTimeInfo(folderEntity.CreatedUtc.UtcDateTime,
                                    folderEntity.LastAccessedUtc.UtcDateTime, folderEntity.LastUpdatedUtc.UtcDateTime));
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    return node.TimeInfo;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeBase Move(NodeBase toBeMovedNode, DirectoryNode targetDirectory)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                if (toBeMovedNode.Attributes.IsReadOnly)
                    throw new FileSystemOperationCanceledException();

                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    switch (toBeMovedNode.NodeType)
                    {
                        case NodeType.File:
                            {
                                var toBeMovedEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, toBeMovedNode.Path.StringPath);
                                var toBeMovedPath = CompositeFileSystemHelper.FileEntityToPath(uow, _userEntity, toBeMovedEntity);

                                var targetEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, targetDirectory.Path.StringPath);
                                var targetPath = CompositeFileSystemHelper.FileEntityToPath(uow, _userEntity, targetEntity);

                                toBeMovedEntity.FolderId = targetEntity.Id;

                                uow.UserFiles.Update(toBeMovedEntity);
                                uow.Commit();

                                toBeMovedNode = new FileNode(toBeMovedNode.Name, targetDirectory);

                                Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' from '{toBeMovedPath}' to '{targetPath}'");
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var toBeMovedEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, toBeMovedNode.Path.StringPath);
                                var toBeMovedPath = CompositeFileSystemHelper.FolderEntityToPath(uow, _userEntity, toBeMovedEntity);

                                var targetEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, targetDirectory.Path.StringPath);
                                var targetPath = CompositeFileSystemHelper.FolderEntityToPath(uow, _userEntity, targetEntity);

                                toBeMovedEntity.ParentId = targetEntity.Id;

                                uow.UserFolders.Update(toBeMovedEntity);
                                uow.Commit();

                                toBeMovedNode = new DirectoryNode(toBeMovedNode.Name, targetDirectory);

                                Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' from '{toBeMovedPath}' to '{targetPath}'");
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    return toBeMovedNode;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeBase Rename(NodeBase node, string newName)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                if (node.Attributes.IsReadOnly)
                    throw new FileSystemOperationCanceledException();

                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            {
                                var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, node.Path.StringPath);

                                fileEntity.VirtualName = newName;
                                fileEntity.LastUpdatedUtc = DateTime.UtcNow;

                                uow.UserFiles.Update(fileEntity);
                                uow.Commit();

                                var filePath = CompositeFileSystemHelper.FileEntityToPath(uow, _userEntity, fileEntity);

                                node = new FileNode(newName, node.Parent);

                                Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' from '{node.Path}' to '{filePath}'");
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, node.Path.StringPath);

                                folderEntity.VirtualName = newName;
                                folderEntity.LastUpdatedUtc = DateTime.UtcNow;

                                uow.UserFolders.Update(folderEntity);
                                uow.Commit();

                                var folderPath = CompositeFileSystemHelper.FolderEntityToPath(uow, _userEntity, folderEntity);

                                node = new DirectoryNode(newName, node.Parent);

                                Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' from '{node.Path}' to '{folderPath}'");
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    return node;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeBase SaveContent(NodeBase node, NodeContent content)
        {
            if (!node.Exists())
                return node;

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            DirectoryInfo folder = null;
            FileInfo file = null;

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            {
                                var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, node.Path.StringPath);

                                folder = new DirectoryInfo(conf["Storage:UnstructuredData"]
                                    + Path.DirectorySeparatorChar + fileEntity.RealPath);

                                if (!folder.Exists)
                                    folder.Create();

                                file = new FileInfo(conf["Storage:UnstructuredData"]
                                    + Path.DirectorySeparatorChar + fileEntity.RealPath
                                    + Path.DirectorySeparatorChar + fileEntity.RealFileName);

                                using (var fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                                    content.GetStream().CopyTo(fs);

                                using (var sha256 = new SHA256Managed())
                                using (var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    var hash = sha256.ComputeHash(fs);

                                    fileEntity.RealFileSize = fs.Length;
                                    fileEntity.HashSHA256 = Strings.GetHexString(hash);
                                }

                                _userEntity = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                                    .Where(x => x.IdentityId == _userEntity.IdentityId).ToLambda())
                                    .Single();

                                _userEntity.QuotaUsedInBytes += content.Length;

                                uow.UserFiles.Update(fileEntity);
                                uow.Users.Update(_userEntity);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' file '{node.Path}' at '{file.FullName}'");
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    return node;
                }
            }
            catch (Exception ex) when (ex is DbUpdateException || ex is DbUpdateConcurrencyException)
            {
                if (file.Exists)
                    file.Delete();

                Log.Error(ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeBase SetAttributes(NodeBase node, NodeAttributes attributes)
        {
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            break;

                        case NodeType.Directory:
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    return node;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeBase SetTimeInfo(NodeBase node, NodeTimeInfo timeInfo)
        {
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            break;

                        case NodeType.Directory:
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    return node;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }
    }
}
