using Bhbk.Daemon.Aurora.SSH.Helpers;
using Bhbk.Lib.Aurora.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
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
using System.Security.Cryptography;
using Hashing = Bhbk.Lib.Cryptography.Hashing;

namespace Bhbk.Daemon.Aurora.SSH.Providers
{
    public class ReadWriteFileSystem : ReadWriteFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private tbl_Users _user;

        public ReadWriteFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, tbl_Users user)
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

        protected override DirectoryNode CreateDirectory(DirectoryNode parent, DirectoryNode child)
        {
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var folder = FileSystemHelper.ConvertPathToSqlForFolder(uow, _user, parent.Path.StringPath);
                    var now = DateTime.UtcNow;

                    var newFolder = uow.UserFolders.Create(
                        new tbl_UserFolders
                        {
                            Id = Guid.NewGuid(),
                            UserId = _user.Id,
                            ParentId = folder.Id,
                            VirtualName = child.Name,
                            Created = now,
                            LastAccessed = null,
                            LastUpdated = null,
                            ReadOnly = false,
                        });
                    uow.Commit();

                    var folderPath = FileSystemHelper.ConvertSqlToPathForFolder(uow, _user, newFolder);

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
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var folder = FileSystemHelper.ConvertPathToSqlForFolder(uow, _user, parent.Path.StringPath);
                    var now = DateTime.UtcNow;

                    var newFile = uow.UserFiles.Create(
                        new tbl_UserFiles
                        {
                            Id = Guid.NewGuid(),
                            UserId = _user.Id,
                            FolderId = folder.Id,
                            VirtualName = child.Name,
                            RealPath = HashHelpers.GenerateDirectoryHash($"{_user.ToString()}{parent.Path.StringPath}{child.Name}"),
                            RealFileName = Hashing.MD5.Create(Guid.NewGuid().ToString()),
                            FileSize = 0,
                            FileHashSHA256 = null,
                            FileReadOnly = false,
                            FileCommitted = false,
                            Created = now,
                            LastAccessed = null,
                            LastUpdated = null,
                        });
                    uow.Commit();

                    var filePath = FileSystemHelper.ConvertSqlToPathForFile(uow, _user, newFile);

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
            try
            {
                if (!node.Exists())
                    return node;

                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    if (node.NodeType == NodeType.Directory)
                    {
                        var folder = FileSystemHelper.ConvertPathToSqlForFolder(uow, _user, node.Path.StringPath);

                        var deleteFolder = uow.UserFolders.Delete(folder);
                        uow.Commit();

                        var folderPath = FileSystemHelper.ConvertSqlToPathForFolder(uow, _user, deleteFolder);

                        return node;
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                        var file = FileSystemHelper.ConvertPathToSqlForFile(uow, _user, node.Path.StringPath);

                        var realFilePath = new FileInfo(conf["Storage:BaseLocalPath"]
                            + Path.DirectorySeparatorChar + file.RealPath
                            + Path.DirectorySeparatorChar + file.RealFileName);

                        /*
                         * any file with zero size is not stored on file-system.
                         */
                        if (file.FileSize == 0)
                        {
                            Log.Information($"Delete file '{node.Path}' with zero size");
                        }
                        else
                        {
                            File.Delete(realFilePath.FullName);

                            Log.Information($"Delete file '{node.Path}' from '{realFilePath.FullName}'");
                        }

                        var deleteFile = uow.UserFiles.Delete(file);
                        uow.Commit();

                        var filePath = FileSystemHelper.ConvertSqlToPathForFile(uow, _user, deleteFile);

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
                        var folder = FileSystemHelper.ConvertPathToSqlForFolder(uow, _user, node.Path.StringPath);

                        if (folder.ReadOnly)
                            return new NodeAttributes(FileAttributes.Directory | FileAttributes.ReadOnly);
                        else
                            return new NodeAttributes(FileAttributes.Directory);
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var file = FileSystemHelper.ConvertPathToSqlForFile(uow, _user, node.Path.StringPath);

                        if (file.FileReadOnly)
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
                if (!node.Exists())
                    return NodeContent.CreateDelayedWriteContent(new MemoryStream());

                using (var scope = _factory.CreateScope())
                {
                    if (node.NodeType == NodeType.File)
                    {
                        var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var folder = FileSystemHelper.ConvertPathToSqlForFolder(uow, _user, node.Parent.Path.StringPath);

                        var file = uow.UserFiles.Get(x => x.UserId == _user.Id
                            && x.FolderId == folder.Id
                            && x.VirtualName == node.Name).Single();

                        var realFilePath = new FileInfo(conf["Storage:BaseLocalPath"]
                            + Path.DirectorySeparatorChar + file.RealPath
                            + Path.DirectorySeparatorChar + file.RealFileName);

                        /*
                         * a un-committed file has been created in database but not on the file-system. this occurs in between calls 
                         * to CreateFile() and SaveContent() by provider.
                         */
                        if (!realFilePath.Exists && !file.FileCommitted)
                        {
                            file.FileCommitted = true;

                            uow.UserFiles.Update(file);
                            uow.Commit();

                            return NodeContent.CreateDelayedWriteContent(new MemoryStream());
                        }
                        /*
                         * a zero length file is created in the database but not on the file-system
                         */
                        else if (!realFilePath.Exists && file.FileCommitted)
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

                            return NodeContent.CreateImmediateWriteContent(File.Open(realFilePath.FullName, FileMode.Open, FileAccess.ReadWrite));
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

        protected override NodeBase Move(NodeBase toBeMovedNode, DirectoryNode targetDirectory)
        {
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    if (toBeMovedNode.NodeType == NodeType.Directory)
                    {
                        var toBeMovedFolder = FileSystemHelper.ConvertPathToSqlForFolder(uow, _user, toBeMovedNode.Path.StringPath);
                        var toBeMoved = FileSystemHelper.ConvertSqlToPathForFolder(uow, _user, toBeMovedFolder);

                        var targetFolder = FileSystemHelper.ConvertPathToSqlForFolder(uow, _user, targetDirectory.Path.StringPath);
                        var target = FileSystemHelper.ConvertSqlToPathForFolder(uow, _user, targetFolder);

                        toBeMovedFolder.ParentId = targetFolder.Id;

                        uow.UserFolders.Update(toBeMovedFolder);
                        uow.Commit();

                        return new DirectoryNode(toBeMovedNode.Name, targetDirectory);
                    }
                    else if (toBeMovedNode.NodeType == NodeType.File)
                    {
                        var toBeMovedFile = FileSystemHelper.ConvertPathToSqlForFile(uow, _user, toBeMovedNode.Path.StringPath);
                        var toBeMoved = FileSystemHelper.ConvertSqlToPathForFile(uow, _user, toBeMovedFile);

                        var targetFolder = FileSystemHelper.ConvertPathToSqlForFile(uow, _user, targetDirectory.Path.StringPath);
                        var target = FileSystemHelper.ConvertSqlToPathForFile(uow, _user, targetFolder);

                        toBeMovedFile.FolderId = targetFolder.Id;

                        uow.UserFiles.Update(toBeMovedFile);
                        uow.Commit();

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
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    if (node.NodeType == NodeType.Directory)
                    {
                        var folder = FileSystemHelper.ConvertPathToSqlForFolder(uow, _user, node.Path.StringPath);
                        var oldPath = FileSystemHelper.ConvertSqlToPathForFolder(uow, _user, folder);

                        folder.VirtualName = newName;

                        uow.UserFolders.Update(folder);
                        uow.Commit();

                        return new DirectoryNode(newName, node.Parent);
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var file = FileSystemHelper.ConvertPathToSqlForFile(uow, _user, node.Path.StringPath);
                        var oldPath = FileSystemHelper.ConvertSqlToPathForFile(uow, _user, file);

                        file.VirtualName = newName;

                        uow.UserFiles.Update(file);
                        uow.Commit();

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
            DirectoryInfo realDirectoryPath = null;
            FileInfo realFilePath = null;

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

                        var file = FileSystemHelper.ConvertPathToSqlForFile(uow, _user, node.Path.StringPath);
                        var now = DateTime.UtcNow;

                        realDirectoryPath = new DirectoryInfo(conf["Storage:BaseLocalPath"]
                            + Path.DirectorySeparatorChar + file.RealPath);

                        realFilePath = new FileInfo(conf["Storage:BaseLocalPath"]
                            + Path.DirectorySeparatorChar + file.RealPath
                            + Path.DirectorySeparatorChar + file.RealFileName);

                        if (!realDirectoryPath.Exists)
                            realDirectoryPath.Create();

                        using (var fs = new FileStream(realFilePath.FullName, FileMode.CreateNew, FileAccess.Write))
                            content.GetStream().CopyTo(fs);

                        using (var sha256 = new SHA256Managed())
                        using (var fs = new FileStream(realFilePath.FullName, FileMode.Open, FileAccess.Read))
                        {
                            var hash = sha256.ComputeHash(fs);

                            file.FileSize = fs.Length;
                            file.FileHashSHA256 = HashHelpers.GetHexString(hash);
                            file.FileReadOnly = false;
                            file.FileCommitted = true;
                            file.Created = now;
                            file.LastAccessed = null;
                            file.LastUpdated = null;

                            uow.UserFiles.Update(file);
                            uow.Commit();
                        }

                        Log.Information($"SaveContent for file '{node.Path}' to '{realFilePath.FullName}'");

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
                if (realFilePath.Exists)
                    realDirectoryPath.Delete();

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
                        var folder = FileSystemHelper.ConvertPathToSqlForFolder(uow, _user, node.Path.StringPath);

                        folder.ReadOnly = attributes.IsReadOnly;

                        uow.UserFolders.Update(folder);
                        uow.Commit();

                        return node;
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var file = FileSystemHelper.ConvertPathToSqlForFile(uow, _user, node.Path.StringPath);

                        file.FileReadOnly = attributes.IsReadOnly;

                        uow.UserFiles.Update(file);
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
                        var folder = FileSystemHelper.ConvertPathToSqlForFolder(uow, _user, node.Path.StringPath);

                        folder.LastAccessed = timeInfo.LastAccessTime;
                        folder.LastUpdated = timeInfo.LastWriteTime;

                        uow.UserFolders.Update(folder);
                        uow.Commit();

                        return node;
                    }
                    else if (node.NodeType == NodeType.File)
                    {
                        var file = FileSystemHelper.ConvertPathToSqlForFile(uow, _user, node.Path.StringPath);

                        file.LastAccessed = timeInfo.LastAccessTime;
                        file.LastUpdated = timeInfo.LastWriteTime;

                        uow.UserFiles.Update(file);
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
