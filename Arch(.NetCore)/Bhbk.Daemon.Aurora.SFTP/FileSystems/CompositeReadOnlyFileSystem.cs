using Bhbk.Daemon.Aurora.SFTP.Helpers;
using Bhbk.Lib.Aurora.Data_EF6.Infrastructure;
using Bhbk.Lib.Aurora.Data_EF6.Models;
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
    internal class CompositeReadOnlyFileSystem : ReadOnlyFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private readonly User _userEntity;

        internal CompositeReadOnlyFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, User userEntity)
            : base(settings)
        {
            _factory = factory;
            _userEntity = userEntity;

            using (var scope = _factory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                CompositeFileSystemHelper.EnsureRootExists(uow, _userEntity);
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
                                folderEntity.LastAccessedUtc.GetValueOrDefault().UtcDateTime,
                                folderEntity.LastUpdatedUtc.GetValueOrDefault().UtcDateTime));

                    var fileEntity = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<UserFile>()
                        .Where(x => x.IdentityId == _userEntity.IdentityId && x.FolderId == folderParentEntity.Id && x.VirtualName == name).ToLambda())
                        .SingleOrDefault();

                    if (fileEntity != null)
                        child = new FileNode(fileEntity.VirtualName, parent,
                            new NodeTimeInfo(fileEntity.CreatedUtc.UtcDateTime,
                                fileEntity.LastAccessedUtc.GetValueOrDefault().UtcDateTime,
                                fileEntity.LastUpdatedUtc.GetValueOrDefault().UtcDateTime));

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

                    var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, parent.Path.StringPath);

                    _userEntity.Folders = uow.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolder>()
                        .Where(x => x.IdentityId == _userEntity.IdentityId).ToLambda())
                        .ToList();

                    foreach (var folder in _userEntity.Folders.Where(x => x.IdentityId == _userEntity.IdentityId && x.ParentId == folderEntity.Id))
                        children.Add(new DirectoryNode(folder.VirtualName, parent,
                            new NodeTimeInfo(folder.CreatedUtc.UtcDateTime,
                                folder.LastAccessedUtc.GetValueOrDefault().UtcDateTime,
                                folder.LastUpdatedUtc.GetValueOrDefault().UtcDateTime)));

                    _userEntity.Files = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<UserFile>()
                        .Where(x => x.IdentityId == _userEntity.IdentityId).ToLambda())
                        .ToList();

                    foreach (var file in _userEntity.Files.Where(x => x.IdentityId == _userEntity.IdentityId && x.FolderId == folderEntity.Id))
                        children.Add(new FileNode(file.VirtualName, parent,
                            new NodeTimeInfo(file.CreatedUtc.UtcDateTime,
                                file.LastAccessedUtc.GetValueOrDefault().UtcDateTime,
                                file.LastUpdatedUtc.GetValueOrDefault().UtcDateTime)));

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
                                    fileEntity.LastAccessedUtc.GetValueOrDefault().UtcDateTime,
                                    fileEntity.LastUpdatedUtc.GetValueOrDefault().UtcDateTime));
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folderEntity = CompositeFileSystemHelper.FolderPathToEntity(uow, _userEntity, node.Path.StringPath);

                                node.SetTimeInfo(new NodeTimeInfo(folderEntity.CreatedUtc.UtcDateTime,
                                    folderEntity.LastAccessedUtc.GetValueOrDefault().UtcDateTime,
                                    folderEntity.LastUpdatedUtc.GetValueOrDefault().UtcDateTime));
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
    }
}
