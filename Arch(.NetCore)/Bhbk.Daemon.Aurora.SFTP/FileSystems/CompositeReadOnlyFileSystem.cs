using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
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
using System.Linq.Expressions;
using System.Reflection;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    public class CompositeReadOnlyFileSystem : ReadOnlyFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private tbl_Users _user;

        public CompositeReadOnlyFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, tbl_Users user)
            : base(settings)
        {
            _factory = factory;
            _user = user;

            using (var scope = factory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                CompositeFileSystemResolver.EnsureRootFolderExists(uow, user);
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
                        var folderEntity = CompositeFileSystemResolver.ConvertPathToSqlFolder(uow, _user, path.StringPath);

                        if (folderEntity != null)
                            return true;

                        return false;
                    }
                    else if (nodeType == NodeType.File)
                    {
                        var fileEntity = CompositeFileSystemResolver.ConvertPathToSqlFile(uow, _user, path.StringPath);

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
                        var folderEntity = CompositeFileSystemResolver.ConvertPathToSqlFolder(uow, _user, node.Path.StringPath);

                        if (folderEntity.ReadOnly)
                            return new NodeAttributes(FileAttributes.Directory | FileAttributes.ReadOnly);

                        else
                            return new NodeAttributes(FileAttributes.Directory);
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var fileEntity = CompositeFileSystemResolver.ConvertPathToSqlFile(uow, _user, node.Path.StringPath);

                        if (fileEntity.ReadOnly)
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

                    var parentFolder = CompositeFileSystemResolver.ConvertPathToSqlFolder(uow, _user, parent.Path.StringPath);

                    var folderEntities = uow.UserFolders.Get(x => x.UserId == _user.Id
                        && x.ParentId == parentFolder.Id
                        && x.VirtualName == name).SingleOrDefault();

                    if (folderEntities != null)
                        return new DirectoryNode(folderEntities.VirtualName, parent,
                            new NodeTimeInfo(folderEntities.Created, folderEntities.LastAccessed, folderEntities.LastUpdated));

                    var fileEntities = uow.UserFiles.Get(x => x.UserId == _user.Id
                        && x.FolderId == parentFolder.Id
                        && x.VirtualName == name).SingleOrDefault();

                    if (fileEntities != null)
                        return new FileNode(fileEntities.VirtualName, parent,
                            new NodeTimeInfo(fileEntities.Created, fileEntities.LastAccessed, fileEntities.LastUpdated));

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

                var children = new List<NodeBase>();

                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    _user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                        .Where(x => x.Id == _user.Id).ToLambda(),
                            new List<Expression<Func<tbl_Users, object>>>()
                            {
                                x => x.tbl_UserFiles,
                                x => x.tbl_UserFolders,
                            }).SingleOrDefault();

                    var parentFolder = CompositeFileSystemResolver.ConvertPathToSqlFolder(uow, _user, parent.Path.StringPath);

                    foreach (var folder in _user.tbl_UserFolders.Where(x => x.UserId == _user.Id && x.ParentId == parentFolder.Id))
                        children.Add(new DirectoryNode(folder.VirtualName, parent,
                            new NodeTimeInfo(folder.Created, folder.LastAccessed, folder.LastUpdated)));

                    foreach (var file in _user.tbl_UserFiles.Where(x => x.UserId == _user.Id && x.FolderId == parentFolder.Id))
                        children.Add(new FileNode(file.VirtualName, parent,
                            new NodeTimeInfo(file.Created, file.LastAccessed, file.LastUpdated)));

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

                        var folderEntity = CompositeFileSystemResolver.ConvertPathToSqlFolder(uow, _user, node.Parent.Path.StringPath);

                        var fileEntity = uow.UserFiles.Get(x => x.UserId == _user.Id
                            && x.FolderId == folderEntity.Id
                            && x.VirtualName == node.Name).Single();

                        var file = new FileInfo(conf["Storage:BaseLocalPath"]
                            + Path.DirectorySeparatorChar + fileEntity.RealPath
                            + Path.DirectorySeparatorChar + fileEntity.RealFileName);

                        fileEntity.LastAccessed = DateTime.UtcNow;

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

                        var fileEntity = CompositeFileSystemResolver.ConvertPathToSqlFile(uow, _user, node.Path.StringPath);

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
                        var folderEntity = CompositeFileSystemResolver.ConvertPathToSqlFolder(uow, _user, node.Path.StringPath);

                        return new NodeTimeInfo(folderEntity.Created, folderEntity.LastAccessed, folderEntity.LastUpdated);
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var fileEntity = CompositeFileSystemResolver.ConvertPathToSqlFile(uow, _user, node.Path.StringPath);

                        return new NodeTimeInfo(fileEntity.Created, fileEntity.LastAccessed, fileEntity.LastUpdated);
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
    }
}
