using Bhbk.Daemon.Aurora.SFTP.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
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
using System.Net;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class DatabaseReadOnlyFileSystem : ReadOnlyFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private UserLogin _user;

        internal DatabaseReadOnlyFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, UserLogin user)
            : base(settings)
        {
            _factory = factory;
            _user = user;

            using (var scope = _factory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                DatabasePathFactory.CheckFolder(uow, _user);
            }
        }

        protected override bool Exists(NodePath path, NodeType nodeType)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

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
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        protected override NodeAttributes GetAttributes(NodeBase node)
        {
            if (!node.Exists())
                return node.Attributes;

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

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
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        protected override NodeBase GetChild(string name, DirectoryNode parent)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

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
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        protected override IEnumerable<NodeBase> GetChildren(DirectoryNode parent, NodeType nodeType)
        {
            if (!parent.Exists())
                return Enumerable.Empty<NodeBase>();

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

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
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        protected override long GetLength(NodeBase node)
        {
            if (!node.Exists())
                return 0L;

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

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
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        protected override NodeTimeInfo GetTimeInfo(NodeBase node)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

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
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }
    }
}
