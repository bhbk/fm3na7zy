using Bhbk.Daemon.Aurora.SFTP.Helpers;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebex.IO.FileSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class CompositeReadOnlyFileSystem : ReadOnlyFileSystemProvider, IDisposable
    {
        private readonly IServiceScopeFactory _factory;
        private readonly tbl_User _userEntity;
        private bool _disposed = false;

        internal CompositeReadOnlyFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, tbl_User userEntity)
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
            try
            {
                if (!node.Exists())
                    return node.Attributes;

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
            try
            {
                if (!parent.Exists())
                    return Enumerable.Empty<NodeBase>();

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
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                if (!node.Exists())
                    return NodeContent.CreateDelayedWriteContent(new MemoryStream());

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
            try
            {
                if (!node.Exists()
                    || node.NodeType == NodeType.Directory)
                    return 0L;

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
