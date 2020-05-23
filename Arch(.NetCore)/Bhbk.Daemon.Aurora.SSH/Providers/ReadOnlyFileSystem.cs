using Bhbk.Daemon.Aurora.SSH.Helpers;
using Bhbk.Lib.Aurora.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
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

namespace Bhbk.Daemon.Aurora.SSH.Providers
{
    public class ReadOnlyFileSystem : ReadOnlyFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private tbl_Users _user;

        public ReadOnlyFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, tbl_Users user)
            : base(settings)
        {
            _factory = factory;
            _user = user;

            using (var scope = factory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                FileSystemHelper.EnsureRootFolderExists(uow, user);
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
                        var folder = FileSystemHelper.ConvertPathToSqlForFolder(uow, _user, path.StringPath);

                        if (folder != null)
                            return true;

                        return false;
                    }
                    else if (nodeType == NodeType.File)
                    {
                        var file = FileSystemHelper.ConvertPathToSqlForFile(uow, _user, path.StringPath);

                        if (file != null)
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

        protected override NodeBase GetChild(string name, DirectoryNode parent)
        {
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var parentFolder = FileSystemHelper.ConvertPathToSqlForFolder(uow, _user, parent.Path.StringPath);

                    var folder = uow.UserFolders.Get(x => x.UserId == _user.Id
                        && x.ParentId == parentFolder.Id
                        && x.VirtualName == name).SingleOrDefault();

                    if (folder != null)
                        return new DirectoryNode(folder.VirtualName, parent, new NodeTimeInfo(folder.Created, folder.LastAccessed, folder.LastUpdated));

                    var file = uow.UserFiles.Get(x => x.UserId == _user.Id
                        && x.FolderId == parentFolder.Id
                        && x.VirtualName == name).SingleOrDefault();

                    if (file != null)
                        return new FileNode(file.VirtualName, parent, new NodeTimeInfo(file.Created, file.LastAccessed, file.LastUpdated));

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

                    var folder = FileSystemHelper.ConvertPathToSqlForFolder(uow, _user, parent.Path.StringPath);

                    foreach (var childFolder in _user.tbl_UserFolders.Where(x => x.UserId == _user.Id && x.ParentId == folder.Id))
                        children.Add(new DirectoryNode(childFolder.VirtualName, parent, new NodeTimeInfo(childFolder.Created, childFolder.LastAccessed, childFolder.LastUpdated)));

                    foreach (var file in _user.tbl_UserFiles.Where(x => x.UserId == _user.Id && x.FolderId == folder.Id))
                        children.Add(new FileNode(file.VirtualName, parent, new NodeTimeInfo(file.Created, file.LastAccessed, file.LastUpdated)));

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
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    if (node.NodeType == NodeType.File)
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                        var folder = FileSystemHelper.ConvertPathToSqlForFolder(uow, _user, node.Parent.Path.StringPath);

                        var file = uow.UserFiles.Get(x => x.UserId == _user.Id
                            && x.FolderId == folder.Id
                            && x.VirtualName == node.Name).Single();

                        var realFilePath = new FileInfo(conf["Storage:BaseLocalPath"]
                            + Path.DirectorySeparatorChar + file.RealPath
                            + Path.DirectorySeparatorChar + file.RealFileName);

                        /*
                         * a zero length file is created in the database but not on the file-system
                         */
                        if (!realFilePath.Exists)
                        {
                            file.LastAccessed = DateTime.UtcNow;

                            uow.UserFiles.Update(file);
                            uow.Commit();

                            return NodeContent.CreateImmediateWriteContent(new MemoryStream());
                        }
                        /*
                         * a non-zero length file that is created in the database and on the file-system
                         */
                        else
                        {
                            file.LastAccessed = DateTime.UtcNow;

                            uow.UserFiles.Update(file);
                            uow.Commit();

                            return NodeContent.CreateReadOnlyContent(File.Open(realFilePath.FullName, FileMode.Open, FileAccess.Read));
                        }
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
                if (!node.Exists())
                    return 0L;

                using (var scope = _factory.CreateScope())
                {
                    if (node.NodeType == NodeType.Directory)
                        return 0L;

                    else if (node.NodeType == NodeType.File)
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var file = FileSystemHelper.ConvertPathToSqlForFile(uow, _user, node.Path.StringPath);

                        return file.FileSize;
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
                        var folder = FileSystemHelper.ConvertPathToSqlForFolder(uow, _user, node.Path.StringPath);

                        return new NodeTimeInfo(folder.Created, folder.LastAccessed, folder.LastUpdated);
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var file = FileSystemHelper.ConvertPathToSqlForFile(uow, _user, node.Path.StringPath);

                        return new NodeTimeInfo(file.Created, file.LastAccessed, file.LastUpdated);
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
