﻿using Bhbk.Daemon.Aurora.SFTP.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
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
    internal class DatabaseReadWriteFileSystem : ReadWriteFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private User _user;

        internal DatabaseReadWriteFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, User user)
            : base(settings)
        {
            _factory = factory;
            _user = user;

            using (var scope = _factory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                DatabasePathFactory.CheckFolder(uow, _user);
            }

            var folderKeysNode = new DirectoryNode(".ssh", Root);
            var fileKeysNode = new FileNode("authorized_keys", folderKeysNode);

            if (!Exists(folderKeysNode.Path, NodeType.Directory))
                CreateDirectory(Root, folderKeysNode);

            if (Exists(fileKeysNode.Path, NodeType.File))
                Delete(fileKeysNode);

            var pubKeysContent = KeyHelper.ExportPubKeyBase64(_user, _user.PublicKeys);

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

                    var folderEntity = DatabasePathFactory.PathToFolder(uow, _user, parent.Path.StringPath);

                    uow.UserFolders.Create(
                        new UserFolder
                        {
                            IdentityId = _user.IdentityId,
                            ParentId = folderEntity.Id,
                            VirtualName = child.Name,
                            IsReadOnly = false,
                        });
                    uow.Commit();

                    Log.Information($"'{callPath}' '{_user.IdentityAlias}' folder:'{child.Path}' at:database");

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

                    _user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                        .Where(x => x.IdentityId == _user.IdentityId).ToLambda())
                        .Single();

                    var folderEntity = DatabasePathFactory.PathToFolder(uow, _user, parent.Path.StringPath);
                    var folderHash = Strings.GetDirectoryHash($"{_user.IdentityId}{parent.Path.StringPath}{child.Name}");
                    var fileName = Hashing.MD5.Create(Guid.NewGuid().ToString());

                    folder = new DirectoryInfo(conf["Storage:UnstructuredData"]
                        + Path.DirectorySeparatorChar + folderHash);

                    if (!folder.Exists)
                        folder.Create();

                    file = new FileInfo(conf["Storage:UnstructuredData"]
                        + Path.DirectorySeparatorChar + folderHash
                        + Path.DirectorySeparatorChar + fileName);

                    /*
                     * enforce quota if user is already over. we do not know size of incoming strea until has
                     * all been received. quota enforcement not possible until after exceeded.
                     */

                    if (_user.QuotaUsedInBytes >= _user.QuotaInBytes)
                        throw new FileSystemOperationCanceledException($"'{callPath}' '{_user.IdentityAlias}' file:'{child.Path}' size:'{child.Length / 1048576f}MB' " +
                            $"at:'{file.FullName}' quota-maximum:'{_user.QuotaInBytes / 1048576f}MB' quota-used:'{_user.QuotaUsedInBytes / 1048576f}MB'");

                    var fileEntity = new UserFile
                    {
                        IdentityId = _user.IdentityId,
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

                    Log.Information($"'{callPath}' '{_user.IdentityAlias}' empty-file:'{child.Path}' at:'{file.FullName}'");

                    return child;
                }
            }
            catch (Exception ex)
                when (ex is FileSystemOperationCanceledException)
            {
                Log.Warning(ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.ToString());
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
                                var fileEntity = DatabasePathFactory.PathToFile(uow, _user, node.Path.StringPath);

                                var file = new FileInfo(conf["Storage:UnstructuredData"]
                                    + Path.DirectorySeparatorChar + fileEntity.RealPath
                                    + Path.DirectorySeparatorChar + fileEntity.RealFileName);

                                _user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                                    .Where(x => x.IdentityId == _user.IdentityId).ToLambda())
                                    .Single();

                                _user.QuotaUsedInBytes -= file.Length;

                                File.Delete(file.FullName);

                                uow.UserFiles.Delete(fileEntity);
                                uow.Users.Update(_user);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_user.IdentityAlias}' file:'{node.Path}' at:'{file.FullName}'");
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folderEntity = DatabasePathFactory.PathToFolder(uow, _user, node.Path.StringPath);

                                uow.UserFolders.Delete(folderEntity);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_user.IdentityAlias}' folder:'{node.Path}' at:database");
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
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    switch (nodeType)
                    {
                        case NodeType.File:
                            {
                                var fileEntity = DatabasePathFactory.PathToFile(uow, _user, path.StringPath);

                                if (fileEntity != null)
                                    return true;

                                return false;
                            }

                        case NodeType.Directory:
                            {
                                var folderEntity = DatabasePathFactory.PathToFolder(uow, _user, path.StringPath);

                                if (folderEntity != null)
                                    return true;

                                return false;
                            }

                        default:
                            throw new NotImplementedException();
                    }
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
                                var fileEntity = DatabasePathFactory.PathToFile(uow, _user, node.Path.StringPath);

                                if (fileEntity.IsReadOnly)
                                    node.SetAttributes(new NodeAttributes(FileAttributes.Normal | FileAttributes.ReadOnly));

                                node.SetAttributes(new NodeAttributes(FileAttributes.Normal));
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folderEntity = DatabasePathFactory.PathToFolder(uow, _user, node.Path.StringPath);

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

                    var folderParentEntity = DatabasePathFactory.PathToFolder(uow, _user, parent.Path.StringPath);

                    var folderEntity = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolder>()
                        .Where(x => x.IdentityId == _user.IdentityId && x.ParentId == folderParentEntity.Id && x.VirtualName == name).ToLambda())
                        .SingleOrDefault();

                    if (folderEntity != null)
                        child = new DirectoryNode(folderEntity.VirtualName, parent,
                            new NodeTimeInfo(folderEntity.CreatedUtc.UtcDateTime,
                                folderEntity.LastAccessedUtc.UtcDateTime, folderEntity.LastUpdatedUtc.UtcDateTime));

                    var fileEntity = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<UserFile>()
                        .Where(x => x.IdentityId == _user.IdentityId && x.FolderId == folderParentEntity.Id && x.VirtualName == name).ToLambda())
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

                    var folderParentEntity = DatabasePathFactory.PathToFolder(uow, _user, parent.Path.StringPath);

                    var folders = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolder>()
                        .Where(x => x.IdentityId == _user.IdentityId).ToLambda())
                        .ToList();

                    foreach (var folder in folders.Where(x => x.IdentityId == _user.IdentityId && x.ParentId == folderParentEntity.Id))
                        children.Add(new DirectoryNode(folder.VirtualName, parent,
                            new NodeTimeInfo(folder.CreatedUtc.UtcDateTime,
                                folder.LastAccessedUtc.UtcDateTime, folder.LastUpdatedUtc.UtcDateTime)));

                    var files = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<UserFile>()
                        .Where(x => x.IdentityId == _user.IdentityId).ToLambda())
                        .ToList();

                    foreach (var file in files.Where(x => x.IdentityId == _user.IdentityId && x.FolderId == folderParentEntity.Id))
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

        protected override NodeContent GetContent(NodeBase node, NodeContentParameters parameters)
        {
            if (!node.Exists())
                return NodeContent.CreateDelayedWriteContent(new MemoryStream());

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            {
                                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                                var fileEntity = DatabasePathFactory.PathToFile(uow, _user, node.Path.StringPath);

                                var file = new FileInfo(conf["Storage:UnstructuredData"]
                                    + Path.DirectorySeparatorChar + fileEntity.RealPath
                                    + Path.DirectorySeparatorChar + fileEntity.RealFileName);

                                var stream = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

                                Log.Information($"'{callPath}' '{_user.IdentityAlias}' file:'{node.Path}' size:'{stream.Length / 1048576f}MB' at:'{file.FullName}'");

                                return parameters.AccessType == NodeContentAccess.Read
                                    ? NodeContent.CreateReadOnlyContent(stream)
                                    : NodeContent.CreateDelayedWriteContent(stream);
                            }

                        default:
                            throw new NotImplementedException();
                    }
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
                using (var scope = _factory.CreateScope())
                {
                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            {
                                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                                var fileEntity = DatabasePathFactory.PathToFile(uow, _user, node.Path.StringPath);

                                return fileEntity.RealFileSize;
                            }

                        case NodeType.Directory:
                            {
                                return 0L;
                            }

                        default:
                            throw new NotImplementedException();
                    }
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
                                var fileEntity = DatabasePathFactory.PathToFile(uow, _user, node.Path.StringPath);

                                node.SetTimeInfo(new NodeTimeInfo(fileEntity.CreatedUtc.UtcDateTime,
                                    fileEntity.LastAccessedUtc.UtcDateTime, fileEntity.LastUpdatedUtc.UtcDateTime));
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folderEntity = DatabasePathFactory.PathToFolder(uow, _user, node.Path.StringPath);

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
                                var toBeMovedEntity = DatabasePathFactory.PathToFile(uow, _user, toBeMovedNode.Path.StringPath);
                                var toBeMovedPath = DatabasePathFactory.FileToPath(uow, _user, toBeMovedEntity);

                                var targetEntity = DatabasePathFactory.PathToFile(uow, _user, targetDirectory.Path.StringPath);
                                var targetPath = DatabasePathFactory.FileToPath(uow, _user, targetEntity);

                                toBeMovedEntity.FolderId = targetEntity.Id;

                                uow.UserFiles.Update(toBeMovedEntity);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_user.IdentityAlias}' from-file:'{toBeMovedPath}' to-file:'{targetPath}' at:database");

                                return new FileNode(toBeMovedNode.Name, targetDirectory);
                            }

                        case NodeType.Directory:
                            {
                                var toBeMovedEntity = DatabasePathFactory.PathToFolder(uow, _user, toBeMovedNode.Path.StringPath);
                                var toBeMovedPath = DatabasePathFactory.FolderToPath(uow, _user, toBeMovedEntity);

                                var targetEntity = DatabasePathFactory.PathToFolder(uow, _user, targetDirectory.Path.StringPath);
                                var targetPath = DatabasePathFactory.FolderToPath(uow, _user, targetEntity);

                                toBeMovedEntity.ParentId = targetEntity.Id;

                                uow.UserFolders.Update(toBeMovedEntity);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_user.IdentityAlias}' from-folder:'{toBeMovedPath}' to-folder:'{targetPath}' at:database");

                                return new DirectoryNode(toBeMovedNode.Name, targetDirectory);
                            }

                        default:
                            throw new NotImplementedException();
                    }
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
                                var fileEntity = DatabasePathFactory.PathToFile(uow, _user, node.Path.StringPath);

                                fileEntity.VirtualName = newName;
                                fileEntity.LastUpdatedUtc = DateTime.UtcNow;

                                uow.UserFiles.Update(fileEntity);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_user.IdentityAlias}' from-file:'{node.Path}' to-file:'{node.Parent.Path}/{newName}' at:database");

                                return new FileNode(newName, node.Parent);
                            }

                        case NodeType.Directory:
                            {
                                var folderEntity = DatabasePathFactory.PathToFolder(uow, _user, node.Path.StringPath);

                                folderEntity.VirtualName = newName;
                                folderEntity.LastUpdatedUtc = DateTime.UtcNow;

                                uow.UserFolders.Update(folderEntity);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_user.IdentityAlias}' from-folder:'{node.Path}' to-folder:'{node.Parent.Path}{newName}' at:database");

                                return new DirectoryNode(newName, node.Parent);
                            }

                        default:
                            throw new NotImplementedException();
                    }
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
                                var fileEntity = DatabasePathFactory.PathToFile(uow, _user, node.Path.StringPath);

                                _user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                                    .Where(x => x.IdentityId == _user.IdentityId).ToLambda())
                                    .Single();

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

                                _user.QuotaUsedInBytes += content.Length;

                                uow.UserFiles.Update(fileEntity);
                                uow.Users.Update(_user);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_user.IdentityAlias}' file:'{node.Path}' size:'{content.Length / 1048576f}MB' at:'{file.FullName}'");
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    return node;
                }
            }
            catch (Exception ex)
                when (ex is DbUpdateException || ex is DbUpdateConcurrencyException)
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