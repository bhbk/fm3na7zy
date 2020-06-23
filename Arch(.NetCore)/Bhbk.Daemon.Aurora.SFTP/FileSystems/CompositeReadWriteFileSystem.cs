using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
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
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using Hashing = Bhbk.Lib.Cryptography.Hashing;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    public class CompositeReadWriteFileSystem : ReadWriteFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private tbl_Users _user;

        public CompositeReadWriteFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, tbl_Users user)
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

        protected override DirectoryNode CreateDirectory(DirectoryNode parent, DirectoryNode child)
        {
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var folderEntity = CompositeFileSystemResolver.ConvertPathToSqlFolder(uow, _user, parent.Path.StringPath);
                    var now = DateTime.UtcNow;

                    uow.UserFolders.Create(
                        new tbl_UserFolders
                        {
                            Id = Guid.NewGuid(),
                            UserId = _user.Id,
                            ParentId = folderEntity.Id,
                            VirtualName = child.Name,
                            Created = now,
                            LastAccessed = null,
                            LastUpdated = null,
                            ReadOnly = false,
                        });
                    uow.Commit();

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
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                    var folderEntity = CompositeFileSystemResolver.ConvertPathToSqlFolder(uow, _user, parent.Path.StringPath);
                    var filePath = HashHelper.GenerateDirectoryHash($"{_user.ToString()}{parent.Path.StringPath}{child.Name}");
                    var fileName = Hashing.MD5.Create(Guid.NewGuid().ToString());
                    var now = DateTime.UtcNow;

                    folder = new DirectoryInfo(conf["Storage:BaseLocalPath"]
                        + Path.DirectorySeparatorChar + filePath);

                    if (!folder.Exists)
                        folder.Create();

                    file = new FileInfo(conf["Storage:BaseLocalPath"]
                        + Path.DirectorySeparatorChar + filePath
                        + Path.DirectorySeparatorChar + fileName);

                    var fileEntity = new tbl_UserFiles
                    {
                        Id = Guid.NewGuid(),
                        UserId = _user.Id,
                        FolderId = folderEntity.Id,
                        VirtualName = child.Name,
                        RealPath = filePath,
                        RealFileName = fileName,
                        ReadOnly = false,
                        Created = now,
                        LastAccessed = null,
                        LastUpdated = null,
                        LastVerified = now,
                    };

                    using (var sha256 = new SHA256Managed())
                    using (var fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        var hash = sha256.ComputeHash(fs);

                        fileEntity.RealFileSize = fs.Length;
                        fileEntity.HashSHA256 = HashHelper.GetHexString(hash);
                    }

                    uow.UserFiles.Create(fileEntity);
                    uow.Commit();

                    return child;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeBase Delete(NodeBase node)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            FileInfo file = null;

            try
            {
                if (!node.Exists())
                    return node;

                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    if (node.NodeType == NodeType.Directory)
                    {
                        var folderEntity = CompositeFileSystemResolver.ConvertPathToSqlFolder(uow, _user, node.Path.StringPath);

                        uow.UserFolders.Delete(folderEntity);
                        uow.Commit();

                        Log.Information($"'{callPath}' '{_user.UserName}' folder '{node.Path}'");

                        return node;
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                        var fileEntity = CompositeFileSystemResolver.ConvertPathToSqlFile(uow, _user, node.Path.StringPath);

                        file = new FileInfo(conf["Storage:BaseLocalPath"]
                            + Path.DirectorySeparatorChar + fileEntity.RealPath
                            + Path.DirectorySeparatorChar + fileEntity.RealFileName);

                        File.Delete(file.FullName);

                        uow.UserFiles.Delete(fileEntity);
                        uow.Commit();

                        Log.Information($"'{callPath}' '{_user.UserName}' file '{node.Path}' from '{file.FullName}'");

                        return node;
                    }
                    else
                        throw new NotImplementedException();
                }
            }
            catch (IOException ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
            catch (DbUpdateException ex)
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
                        var toBeMovedEntity = CompositeFileSystemResolver.ConvertPathToSqlFolder(uow, _user, toBeMovedNode.Path.StringPath);
                        var toBeMovedPath = CompositeFileSystemResolver.ConvertSqlToPathFolder(uow, _user, toBeMovedEntity);

                        var targetEntity = CompositeFileSystemResolver.ConvertPathToSqlFolder(uow, _user, targetDirectory.Path.StringPath);
                        var targetPath = CompositeFileSystemResolver.ConvertSqlToPathFolder(uow, _user, targetEntity);

                        toBeMovedEntity.ParentId = targetEntity.Id;

                        uow.UserFolders.Update(toBeMovedEntity);
                        uow.Commit();

                        Log.Information($"'{callPath}' '{_user.UserName}' from '{toBeMovedPath}' to '{targetPath}'");

                        return new DirectoryNode(toBeMovedNode.Name, targetDirectory);
                    }
                    else if (toBeMovedNode.NodeType == NodeType.File)
                    {
                        var toBeMovedEntity = CompositeFileSystemResolver.ConvertPathToSqlFile(uow, _user, toBeMovedNode.Path.StringPath);
                        var toBeMovedPath = CompositeFileSystemResolver.ConvertSqlToPathFile(uow, _user, toBeMovedEntity);

                        var targetEntity = CompositeFileSystemResolver.ConvertPathToSqlFile(uow, _user, targetDirectory.Path.StringPath);
                        var targetPath = CompositeFileSystemResolver.ConvertSqlToPathFile(uow, _user, targetEntity);

                        toBeMovedEntity.FolderId = targetEntity.Id;

                        uow.UserFiles.Update(toBeMovedEntity);
                        uow.Commit();

                        Log.Information($"'{callPath}' '{_user.UserName}' from '{toBeMovedPath}' to '{targetPath}'");

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
                        var folderEntity = CompositeFileSystemResolver.ConvertPathToSqlFolder(uow, _user, node.Path.StringPath);

                        folderEntity.VirtualName = newName;

                        uow.UserFolders.Update(folderEntity);
                        uow.Commit();

                        var folderPath = CompositeFileSystemResolver.ConvertSqlToPathFolder(uow, _user, folderEntity);

                        Log.Information($"'{callPath}' '{_user.UserName}' from '{node.Path}' to '{folderPath}'");

                        return new DirectoryNode(newName, node.Parent);
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var fileEntity = CompositeFileSystemResolver.ConvertPathToSqlFile(uow, _user, node.Path.StringPath);

                        fileEntity.VirtualName = newName;

                        uow.UserFiles.Update(fileEntity);
                        uow.Commit();

                        var filePath = CompositeFileSystemResolver.ConvertSqlToPathFile(uow, _user, fileEntity);

                        Log.Information($"'{callPath}' '{_user.UserName}' from '{node.Path}' to '{filePath}'");

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
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            DirectoryInfo folder = null;
            FileInfo file = null;

            try
            {
                if (!node.Exists())
                    return node;

                using (var scope = _factory.CreateScope())
                {
                    if (node.NodeType == NodeType.File)
                    {
                        var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var fileEntity = CompositeFileSystemResolver.ConvertPathToSqlFile(uow, _user, node.Path.StringPath);
                        var now = DateTime.UtcNow;

                        folder = new DirectoryInfo(conf["Storage:BaseLocalPath"]
                            + Path.DirectorySeparatorChar + fileEntity.RealPath);

                        if (!folder.Exists)
                            folder.Create();

                        file = new FileInfo(conf["Storage:BaseLocalPath"]
                            + Path.DirectorySeparatorChar + fileEntity.RealPath
                            + Path.DirectorySeparatorChar + fileEntity.RealFileName);

                        using (var fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                            content.GetStream().CopyTo(fs);

                        using (var sha256 = new SHA256Managed())
                        using (var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            var hash = sha256.ComputeHash(fs);

                            fileEntity.RealFileSize = fs.Length;
                            fileEntity.HashSHA256 = HashHelper.GetHexString(hash);
                            fileEntity.ReadOnly = false;
                            fileEntity.Created = now;
                            fileEntity.LastAccessed = null;
                            fileEntity.LastUpdated = null;
                        }

                        uow.UserFiles.Update(fileEntity);
                        uow.Commit();

                        Log.Information($"'{callPath}' '{_user.UserName}' file '{node.Path}' to '{file.FullName}'");

                        return node;
                    }
                    else
                        throw new NotImplementedException();
                }
            }
            catch (IOException ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
            catch (DbUpdateException ex)
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
                        var folderEntity = CompositeFileSystemResolver.ConvertPathToSqlFolder(uow, _user, node.Path.StringPath);

                        folderEntity.ReadOnly = attributes.IsReadOnly;

                        uow.UserFolders.Update(folderEntity);
                        uow.Commit();

                        return node;
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var fileEntity = CompositeFileSystemResolver.ConvertPathToSqlFile(uow, _user, node.Path.StringPath);

                        fileEntity.ReadOnly = attributes.IsReadOnly;

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
                        var folderEntity = CompositeFileSystemResolver.ConvertPathToSqlFolder(uow, _user, node.Path.StringPath);

                        folderEntity.LastAccessed = timeInfo.LastAccessTime;
                        folderEntity.LastUpdated = timeInfo.LastWriteTime;

                        uow.UserFolders.Update(folderEntity);
                        uow.Commit();

                        return node;
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var fileEntity = CompositeFileSystemResolver.ConvertPathToSqlFile(uow, _user, node.Path.StringPath);

                        fileEntity.LastAccessed = timeInfo.LastAccessTime;
                        fileEntity.LastUpdated = timeInfo.LastWriteTime;

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
    }
}
