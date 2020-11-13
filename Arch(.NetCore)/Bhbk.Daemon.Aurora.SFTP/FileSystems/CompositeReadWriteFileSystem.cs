using Bhbk.Daemon.Aurora.SFTP.Helpers;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Common.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebex.IO.FileSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Hashing = Bhbk.Lib.Cryptography.Hashing;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class CompositeReadWriteFileSystem : ReadWriteFileSystemProvider, IDisposable
    {
        private readonly IServiceScopeFactory _factory;
        private readonly tbl_User _userEntity;
        private bool _disposed = false;

        internal CompositeReadWriteFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, tbl_User userEntity)
            : base(settings)
        {
            _factory = factory;
            _userEntity = userEntity;

            using (var scope = _factory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var pubKeys = uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKey>()
                    .Where(x => x.IdentityId == _userEntity.IdentityId).ToLambda()).ToList();

                var pubKeysContent = KeyHelper.ExportPubKeyBase64(_userEntity, pubKeys);

                CompositeFileSystemHelper.EnsureRootExists(uow, userEntity);
            }
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
                        new tbl_UserFolder
                        {
                            Id = Guid.NewGuid(),
                            IdentityId = _userEntity.IdentityId,
                            ParentId = folderEntity.Id,
                            VirtualName = child.Name,
                            CreatedUtc = DateTime.UtcNow,
                            IsReadOnly = false,
                        });
                    uow.Commit();

                    Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' directory '{child.Path}'");
                }

                return child;
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

            FileInfo file = null;

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                    var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, parent.Path.StringPath);
                    var filePath = Strings.GetDirectoryHash($"{_userEntity.ToString()}{parent.Path.StringPath}{child.Name}");
                    var fileName = Hashing.MD5.Create(Guid.NewGuid().ToString());
                    var now = DateTime.UtcNow;

                    var folder = new DirectoryInfo(conf["Storage:UnstructuredDataPath"]
                        + Path.DirectorySeparatorChar + filePath);

                    if (!folder.Exists)
                        folder.Create();

                    file = new FileInfo(conf["Storage:UnstructuredDataPath"]
                        + Path.DirectorySeparatorChar + filePath
                        + Path.DirectorySeparatorChar + fileName);

                    var fileEntity = new tbl_UserFile
                    {
                        Id = Guid.NewGuid(),
                        IdentityId = _userEntity.IdentityId,
                        FolderId = folderEntity.Id,
                        VirtualName = child.Name,
                        RealPath = filePath,
                        RealFileName = fileName,
                        IsReadOnly = false,
                        CreatedUtc = now,
                        LastVerifiedUtc = now,
                    };

                    using (var sha256 = new SHA256Managed())
                    using (var fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        var hash = sha256.ComputeHash(fs);

                        fileEntity.RealFileSize = fs.Length;
                        fileEntity.HashSHA256 = Strings.GetHexString(hash);
                    }

                    uow.UserFiles.Create(fileEntity);
                    uow.Commit();

                    Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' file '{child.Path}' to '{file.FullName}'");
                }

                return child;
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
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    if (node.NodeType == NodeType.Directory)
                    {
                        var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, node.Path.StringPath);

                        uow.UserFolders.Delete(folderEntity);
                        uow.Commit();

                        Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' folder '{node.Path}'");

                        return node;
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                        var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, node.Path.StringPath);

                        var file = new FileInfo(conf["Storage:UnstructuredDataPath"]
                            + Path.DirectorySeparatorChar + fileEntity.RealPath
                            + Path.DirectorySeparatorChar + fileEntity.RealFileName);

                        File.Delete(file.FullName);

                        uow.UserFiles.Delete(fileEntity);
                        uow.Commit();

                        Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' file '{node.Path}' from '{file.FullName}'");

                        return node;
                    }
                    else
                        throw new NotImplementedException();
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

                    if (nodeType == NodeType.Directory)
                    {
                        var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, path.StringPath);

                        if (folderEntity != null)
                            return true;

                        return false;
                    }
                    else if (nodeType == NodeType.File)
                    {
                        var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, path.StringPath);

                        if (fileEntity != null)
                            return true;

                        return false;
                    }
                    else
                        throw new NotImplementedException();
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

                    if (node.NodeType == NodeType.Directory)
                    {
                        var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, node.Path.StringPath);

                        if (folderEntity.IsReadOnly)
                            return new NodeAttributes(FileAttributes.Directory | FileAttributes.ReadOnly);

                        else
                            return new NodeAttributes(FileAttributes.Directory);
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, node.Path.StringPath);

                        if (fileEntity.IsReadOnly)
                            return new NodeAttributes(FileAttributes.ReadOnly);

                        else
                            return new NodeAttributes(FileAttributes.Normal);
                    }
                    else
                        throw new NotImplementedException();
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
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var parentFolder = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, parent.Path.StringPath);

                    var folderEntities = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserFolder>()
                        .Where(x => x.IdentityId == _userEntity.IdentityId && x.ParentId == parentFolder.Id && x.VirtualName == name).ToLambda())
                        .SingleOrDefault();

                    if (folderEntities != null)
                        return new DirectoryNode(folderEntities.VirtualName, parent,
                            new NodeTimeInfo(folderEntities.CreatedUtc.UtcDateTime,
                                folderEntities.LastAccessedUtc?.UtcDateTime, folderEntities.LastUpdatedUtc?.UtcDateTime));

                    var fileEntities = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserFile>()
                        .Where(x => x.IdentityId == _userEntity.IdentityId && x.FolderId == parentFolder.Id && x.VirtualName == name).ToLambda())
                        .SingleOrDefault();

                    if (fileEntities != null)
                        return new FileNode(fileEntities.VirtualName, parent,
                            new NodeTimeInfo(fileEntities.CreatedUtc.UtcDateTime,
                                fileEntities.LastAccessedUtc?.UtcDateTime, fileEntities.LastUpdatedUtc?.UtcDateTime));

                    return null;
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
                using (var scope = _factory.CreateScope())
                {
                    var children = new List<NodeBase>();

                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var parentFolder = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, parent.Path.StringPath);

                    _userEntity.tbl_UserFolders = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserFolder>()
                        .Where(x => x.IdentityId == _userEntity.IdentityId).ToLambda())
                        .ToList();

                    _userEntity.tbl_UserFiles = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserFile>()
                        .Where(x => x.IdentityId == _userEntity.IdentityId).ToLambda())
                        .ToList();

                    foreach (var folder in _userEntity.tbl_UserFolders.Where(x => x.IdentityId == _userEntity.IdentityId && x.ParentId == parentFolder.Id))
                        children.Add(new DirectoryNode(folder.VirtualName, parent,
                            new NodeTimeInfo(folder.CreatedUtc.UtcDateTime,
                                folder.LastAccessedUtc?.UtcDateTime, folder.LastUpdatedUtc?.UtcDateTime)));

                    foreach (var file in _userEntity.tbl_UserFiles.Where(x => x.IdentityId == _userEntity.IdentityId && x.FolderId == parentFolder.Id))
                        children.Add(new FileNode(file.VirtualName, parent,
                            new NodeTimeInfo(file.CreatedUtc.UtcDateTime,
                                file.LastAccessedUtc?.UtcDateTime, file.LastUpdatedUtc?.UtcDateTime)));

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
                using (var scope = _factory.CreateScope())
                {
                    if (node.NodeType == NodeType.File)
                    {
                        var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, node.Parent.Path.StringPath);

                        var fileEntity = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserFile>()
                            .Where(x => x.IdentityId == _userEntity.IdentityId && x.FolderId == folderEntity.Id && x.VirtualName == node.Name).ToLambda())
                            .Single();

                        var file = new FileInfo(conf["Storage:UnstructuredDataPath"]
                            + Path.DirectorySeparatorChar + fileEntity.RealPath
                            + Path.DirectorySeparatorChar + fileEntity.RealFileName);

                        fileEntity.LastAccessedUtc = DateTime.UtcNow;

                        uow.UserFiles.Update(fileEntity);
                        uow.Commit();

                        Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' file '{node.Path}' from '{file.FullName}'");

                        return NodeContent.CreateDelayedWriteContent(File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                    }
                    else
                        throw new NotImplementedException();
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
            if (!node.Exists()
                || node.NodeType == NodeType.Directory)
                return 0L;

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    if (node.NodeType == NodeType.File)
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, node.Path.StringPath);

                        return fileEntity.RealFileSize;
                    }
                    else
                        throw new NotImplementedException();
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

                    if (node.NodeType == NodeType.Directory)
                    {
                        var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, node.Path.StringPath);

                        return new NodeTimeInfo(folderEntity.CreatedUtc.UtcDateTime,
                            folderEntity.LastAccessedUtc?.UtcDateTime, folderEntity.LastUpdatedUtc?.UtcDateTime);
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, node.Path.StringPath);

                        return new NodeTimeInfo(fileEntity.CreatedUtc.UtcDateTime,
                            fileEntity.LastAccessedUtc?.UtcDateTime, fileEntity.LastUpdatedUtc?.UtcDateTime);
                    }
                    else
                        throw new NotImplementedException();
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
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    if (toBeMovedNode.NodeType == NodeType.Directory)
                    {
                        var toBeMovedEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, toBeMovedNode.Path.StringPath);
                        var toBeMovedPath = CompositeFileSystemHelper.FolderEntityToPath(uow, _userEntity, toBeMovedEntity);

                        var targetEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, targetDirectory.Path.StringPath);
                        var targetPath = CompositeFileSystemHelper.FolderEntityToPath(uow, _userEntity, targetEntity);

                        toBeMovedEntity.ParentId = targetEntity.Id;

                        uow.UserFolders.Update(toBeMovedEntity);
                        uow.Commit();

                        Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' from '{toBeMovedPath}' to '{targetPath}'");

                        return new DirectoryNode(toBeMovedNode.Name, targetDirectory);
                    }
                    else if (toBeMovedNode.NodeType == NodeType.File)
                    {
                        var toBeMovedEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, toBeMovedNode.Path.StringPath);
                        var toBeMovedPath = CompositeFileSystemHelper.FileEntityToPath(uow, _userEntity, toBeMovedEntity);

                        var targetEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, targetDirectory.Path.StringPath);
                        var targetPath = CompositeFileSystemHelper.FileEntityToPath(uow, _userEntity, targetEntity);

                        toBeMovedEntity.FolderId = targetEntity.Id;

                        uow.UserFiles.Update(toBeMovedEntity);
                        uow.Commit();

                        Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' from '{toBeMovedPath}' to '{targetPath}'");

                        return new FileNode(toBeMovedNode.Name, targetDirectory);
                    }
                    else
                        throw new NotImplementedException();
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
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    if (node.NodeType == NodeType.Directory)
                    {
                        var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, node.Path.StringPath);

                        folderEntity.VirtualName = newName;

                        uow.UserFolders.Update(folderEntity);
                        uow.Commit();

                        var folderPath = CompositeFileSystemHelper.FolderEntityToPath(uow, _userEntity, folderEntity);

                        Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' from '{node.Path}' to '{folderPath}'");

                        return new DirectoryNode(newName, node.Parent);
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, node.Path.StringPath);

                        fileEntity.VirtualName = newName;

                        uow.UserFiles.Update(fileEntity);
                        uow.Commit();

                        var filePath = CompositeFileSystemHelper.FileEntityToPath(uow, _userEntity, fileEntity);

                        Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' from '{node.Path}' to '{filePath}'");

                        return new FileNode(newName, node.Parent);
                    }
                    else
                        throw new NotImplementedException();
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

            FileInfo file = null;

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    if (node.NodeType == NodeType.File)
                    {
                        var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, node.Path.StringPath);

                        file = new FileInfo(conf["Storage:UnstructuredDataPath"]
                            + Path.DirectorySeparatorChar + fileEntity.RealPath
                            + Path.DirectorySeparatorChar + fileEntity.RealFileName);

                        fileEntity = CompositeFileSystemHelper.SaveFileStream(conf, content.GetStream(), fileEntity);

                        uow.UserFiles.Update(fileEntity);
                        uow.Commit();

                        Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' file '{node.Path}' to '{file.FullName}'");

                        return node;
                    }
                    else
                        throw new NotImplementedException();
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
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    if (node.NodeType == NodeType.Directory)
                    {
                        var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, node.Path.StringPath);

                        folderEntity.IsReadOnly = attributes.IsReadOnly;

                        uow.UserFolders.Update(folderEntity);
                        uow.Commit();

                        return node;
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, node.Path.StringPath);

                        fileEntity.IsReadOnly = attributes.IsReadOnly;

                        uow.UserFiles.Update(fileEntity);
                        uow.Commit();

                        return node;
                    }
                    else
                        throw new NotImplementedException();
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
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    if (node.NodeType == NodeType.Directory)
                    {
                        var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, node.Path.StringPath);

                        folderEntity.LastAccessedUtc = timeInfo.LastAccessTime;
                        folderEntity.LastUpdatedUtc = timeInfo.LastWriteTime;

                        uow.UserFolders.Update(folderEntity);
                        uow.Commit();

                        return node;
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var fileEntity = CompositeFileSystemHelper.FilePathToEntity(uow, _userEntity, node.Path.StringPath);

                        fileEntity.LastAccessedUtc = timeInfo.LastAccessTime;
                        fileEntity.LastUpdatedUtc = timeInfo.LastWriteTime;

                        uow.UserFiles.Update(fileEntity);
                        uow.Commit();

                        return node;
                    }
                    else
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)

                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                _disposed = true;
            }
        }

        public new void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
