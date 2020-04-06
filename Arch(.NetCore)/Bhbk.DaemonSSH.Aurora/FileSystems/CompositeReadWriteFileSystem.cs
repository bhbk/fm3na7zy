using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebex;
using Rebex.IO.FileSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using Hashing = Bhbk.Lib.Cryptography.Hashing;

namespace Bhbk.DaemonSSH.Aurora.FileSystems
{
    public class CompositeReadWriteFileSystem : ReadWriteFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private readonly Guid _userId;
        private LogLevel _level;

        public CompositeReadWriteFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, Guid userId)
            : base(settings)
        {
            _factory = factory;
            _userId = userId;

            var scope = _factory.CreateScope();
            _conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            _uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            UtilEnsureUserHasRootFolder();
        }

        protected override FileNode CreateFile(DirectoryNode parent, FileNode child)
        {
            try
            {
                var folder = UtilConvertPathToSqlForFolder(parent.Path.StringPath);
                var moment = DateTime.UtcNow;

                var newFile = _uow.UserFiles.Create(
                    new tbl_UserFiles
                    {
                        Id = Guid.NewGuid(),
                        UserId = _userId,
                        FolderId = folder.Id,
                        VirtualName = child.Name,
                        RealPath = HashHelpers.GenerateDirectoryHash($"{_userId.ToString()}{parent.Path.StringPath}{child.Name}"),
                        RealFileName = Hashing.MD5.Create(Guid.NewGuid().ToString()),
                        FileSize = child.Length,
                        FileHashSHA256 = string.Empty,
                        Created = moment,
                        LastAccessed = null,
                        LastUpdated = null,
                        ReadOnly = false,
                    });
                _uow.Commit();

                var filePath = UtilConvertSqlToPathForFile(newFile);

                Log.Information($"CREATE for file '{filePath}'");

                return child;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override DirectoryNode CreateDirectory(DirectoryNode parent, DirectoryNode child)
        {
            try
            {
                var folder = UtilConvertPathToSqlForFolder(parent.Path.StringPath);
                var moment = DateTime.UtcNow;

                var newFolder = _uow.UserFolders.Create(
                    new tbl_UserFolders
                    {
                        Id = Guid.NewGuid(),
                        UserId = _userId,
                        ParentId = folder.Id,
                        VirtualName = child.Name,
                        Created = moment,
                        LastAccessed = null,
                        LastUpdated = null,
                        ReadOnly = false,
                    });
                _uow.Commit();

                var folderPath = UtilConvertSqlToPathForFolder(newFolder);

                Log.Information($"CREATE for directory '{folderPath}'");

                return child;
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

                if (node.NodeType == NodeType.Directory)
                {
                    var folder = UtilConvertPathToSqlForFolder(node.Path.StringPath);

                    var deleteFolder = _uow.UserFolders.Delete(folder);
                    _uow.Commit();

                    var folderPath = UtilConvertSqlToPathForFolder(deleteFolder);

                    Log.Information($"DELETE for directory '{folderPath}'");

                    return node;
                }
                else if (node.NodeType == NodeType.File)
                {
                    var file = UtilConvertPathToSqlForFile(node.Path.StringPath);

                    var realFilePath = _conf["Storage:LocalBasePath"]
                        + Path.DirectorySeparatorChar + file.RealPath
                        + Path.DirectorySeparatorChar + file.RealFileName;

                    File.Delete(realFilePath);

                    var deleteFile = _uow.UserFiles.Delete(file);
                    _uow.Commit();

                    var filePath = UtilConvertSqlToPathForFile(deleteFile);

                    Log.Information($"DELETE for file '{filePath}'");

                    return node;
                }
                else
                    throw new NotImplementedException();
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
                if (nodeType == NodeType.Directory)
                {
                    var folder = UtilConvertPathToSqlForFolder(path.StringPath);

                    if (folder != null)
                    {
                        var folderPath = UtilConvertSqlToPathForFolder(folder);
                        Log.Information($"EXISTS found directory '{folderPath}'");

                        return true;
                    }

                    Log.Information($"EXISTS found no directory '{path.StringPath}'");

                    return false;
                }
                else if (nodeType == NodeType.File)
                {
                    var file = UtilConvertPathToSqlForFile(path.StringPath);

                    if (file != null)
                    {
                        var filePath = UtilConvertSqlToPathForFile(file);
                        Log.Information($"EXISTS found file '{filePath}'");

                        return true;
                    }

                    Log.Information($"EXISTS found no file '{path.StringPath}'");

                    return false;
                }
                else
                    throw new NotImplementedException();

                return false;
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

                if (node.NodeType == NodeType.Directory)
                {
                    var folder = UtilConvertPathToSqlForFolder(node.Path.StringPath);
                    var folderPath = UtilConvertSqlToPathForFolder(folder);

                    Log.Information($"GETATTRIBUTES for directory '{folderPath}'");

                    if (folder.ReadOnly)
                        return new NodeAttributes(FileAttributes.Directory | FileAttributes.ReadOnly);
                    else
                        return new NodeAttributes(FileAttributes.Directory);
                }
                else if (node.NodeType == NodeType.File)
                {
                    var file = UtilConvertPathToSqlForFile(node.Path.StringPath);
                    var filePath = UtilConvertSqlToPathForFile(file);

                    Log.Information($"GETATTRIBUTES for file '{filePath}'");

                    if (file.ReadOnly)
                        return new NodeAttributes(FileAttributes.ReadOnly);
                    else
                        return new NodeAttributes(FileAttributes.Normal);
                }
                else
                    throw new NotImplementedException();
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
                var parentFolder = UtilConvertPathToSqlForFolder(parent.Path.StringPath);

                var folder = _uow.UserFolders.Get(x => x.UserId == _userId
                    && x.ParentId == parentFolder.Id
                    && x.VirtualName == name).SingleOrDefault();

                if (folder != null)
                {
                    Log.Information($"GETCHILD found '{folder.VirtualName}' in directory '{parent.Path.StringPath}'");

                    return new DirectoryNode(folder.VirtualName, parent, new NodeTimeInfo(folder.Created, folder.LastAccessed, folder.LastUpdated));
                }

                var file = _uow.UserFiles.Get(x => x.UserId == _userId
                    && x.FolderId == parentFolder.Id
                    && x.VirtualName == name).SingleOrDefault();

                if (file != null)
                {
                    Log.Information($"GETCHILD found '{file.VirtualName}' in directory '{parent.Path.StringPath}'");

                    return new FileNode(file.VirtualName, parent, new NodeTimeInfo(file.Created, file.LastAccessed, file.LastUpdated));
                }

                Log.Information($"GETCHILD found no '{name}' in directory '{parent.Path.StringPath}'");

                return null;
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

                var user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                    .Where(x => x.Id == _userId).ToLambda(),
                        new List<Expression<Func<tbl_Users, object>>>()
                        {
                            x => x.tbl_UserFiles,
                            x => x.tbl_UserFolders,
                        }).SingleOrDefault();

                var folder = UtilConvertPathToSqlForFolder(parent.Path.StringPath);

                foreach (var childFolder in user.tbl_UserFolders.Where(x => x.UserId == _userId && x.ParentId == folder.Id))
                    children.Add(new DirectoryNode(childFolder.VirtualName, parent, new NodeTimeInfo(childFolder.Created, childFolder.LastAccessed, childFolder.LastUpdated)));

                foreach (var file in user.tbl_UserFiles.Where(x => x.UserId == _userId && x.FolderId == folder.Id))
                    children.Add(new FileNode(file.VirtualName, parent, new NodeTimeInfo(file.Created, file.LastAccessed, file.LastUpdated)));

                Log.Information($"GETCHILDREN for directory '{parent.Path.StringPath}'");

                return children;
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

                if (node.NodeType == NodeType.File)
                {
                    var folder = UtilConvertPathToSqlForFolder(node.Parent.Path.StringPath);

                    var file = _uow.UserFiles.Get(x => x.UserId == _userId
                        && x.FolderId == folder.Id
                        && x.VirtualName == node.Name).Single();

                    var realFilePath = new FileInfo(_conf["Storage:LocalBasePath"]
                        + Path.DirectorySeparatorChar + file.RealPath
                        + Path.DirectorySeparatorChar + file.RealFileName);

                    Log.Information($"GETCONTENT for file '{node.Path.StringPath}' from '{realFilePath.FullName}'");

                    /*
                     * check if file has been created in database but not on file-system
                     */
                    if (!realFilePath.Exists)
                        return NodeContent.CreateDelayedWriteContent(new MemoryStream());

                    FileStream fileStream;

                    if (contentParameters.AccessType == NodeContentAccess.Read)
                        fileStream = File.OpenRead(realFilePath.FullName);
                    else
                        fileStream = File.Open(realFilePath.FullName, FileMode.Open, FileAccess.ReadWrite);

                    file.LastAccessed = DateTime.UtcNow;

                    _uow.UserFiles.Update(file);
                    _uow.Commit();

                    if (contentParameters.AccessType == NodeContentAccess.Read)
                        return NodeContent.CreateReadOnlyContent(fileStream);
                    else
                        return NodeContent.CreateImmediateWriteContent(fileStream);
                }
                else
                    throw new NotImplementedException();
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

                if (node.NodeType == NodeType.Directory)
                {
                    var folder = UtilConvertPathToSqlForFolder(node.Path.StringPath);
                    var folderPath = UtilConvertSqlToPathForFolder(folder);

                    Log.Information($"GETLENGTH for directory '{node.Path.StringPath}' of '{0L}'");

                    return 0L;
                }
                else if (node.NodeType == NodeType.File)
                {
                    var file = UtilConvertPathToSqlForFile(node.Path.StringPath);
                    var filePath = UtilConvertSqlToPathForFile(file);

                    Log.Information($"GETLENGTH for file '{node.Path.StringPath}' of '{file.FileSize}'");

                    return file.FileSize;
                }
                else
                    throw new NotImplementedException();
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
                if (node.NodeType == NodeType.Directory)
                {
                    var folder = UtilConvertPathToSqlForFolder(node.Path.StringPath);
                    var folderPath = UtilConvertSqlToPathForFolder(folder);

                    Log.Information($"GETTIMEINFO for directory '{folderPath}'");

                    return new NodeTimeInfo(folder.Created, folder.LastAccessed, folder.LastUpdated);
                }
                else if (node.NodeType == NodeType.File)
                {
                    var file = UtilConvertPathToSqlForFile(node.Path.StringPath);
                    var filePath = UtilConvertSqlToPathForFile(file);

                    Log.Information($"GETTIMEINFO for file '{filePath}'");

                    return new NodeTimeInfo(file.Created, file.LastAccessed, file.LastUpdated);
                }
                else
                    throw new NotImplementedException();
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
                if (toBeMovedNode.NodeType == NodeType.Directory)
                {
                    var toBeMovedFolder = UtilConvertPathToSqlForFolder(toBeMovedNode.Path.StringPath);
                    var toBeMoved = UtilConvertSqlToPathForFolder(toBeMovedFolder);

                    var targetFolder = UtilConvertPathToSqlForFolder(targetDirectory.Path.StringPath);
                    var target = UtilConvertSqlToPathForFolder(targetFolder);

                    toBeMovedFolder.ParentId = targetFolder.Id;

                    _uow.UserFolders.Update(toBeMovedFolder);
                    _uow.Commit();

                    Log.Information($"MOVE for directory '{toBeMoved}' to '{target}'");

                    return new DirectoryNode(toBeMovedNode.Name, targetDirectory);
                }
                else if (toBeMovedNode.NodeType == NodeType.File)
                {
                    var toBeMovedFile = UtilConvertPathToSqlForFile(toBeMovedNode.Path.StringPath);
                    var toBeMoved = UtilConvertSqlToPathForFile(toBeMovedFile);

                    var targetFolder = UtilConvertPathToSqlForFile(targetDirectory.Path.StringPath);
                    var target = UtilConvertSqlToPathForFile(targetFolder);

                    toBeMovedFile.FolderId = targetFolder.Id;

                    _uow.UserFiles.Update(toBeMovedFile);
                    _uow.Commit();

                    Log.Information($"MOVE for file '{toBeMoved}' to '{target}'");

                    return new FileNode(toBeMovedNode.Name, targetDirectory);
                }
                else
                    throw new NotImplementedException();
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
                if (node.NodeType == NodeType.Directory)
                {
                    var folder = UtilConvertPathToSqlForFolder(node.Path.StringPath);
                    var oldPath = UtilConvertSqlToPathForFolder(folder);

                    folder.VirtualName = newName;

                    _uow.UserFolders.Update(folder);
                    _uow.Commit();

                    var newPath = UtilConvertSqlToPathForFolder(folder);

                    Log.Information($"RENAME for directory '{oldPath}' to '{newPath}'");

                    return new DirectoryNode(newName, node.Parent);
                }
                else if (node.NodeType == NodeType.File)
                {
                    var file = UtilConvertPathToSqlForFile(node.Path.StringPath);
                    var oldPath = UtilConvertSqlToPathForFile(file);

                    file.VirtualName = newName;

                    _uow.UserFiles.Update(file);
                    _uow.Commit();

                    var newPath = UtilConvertSqlToPathForFile(file);

                    Log.Information($"RENAME for file '{oldPath}' to '{newPath}'");

                    return new FileNode(newName, node.Parent);
                }
                else
                    throw new NotImplementedException();
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
                if (node.NodeType == NodeType.Directory)
                {
                    var folder = UtilConvertPathToSqlForFolder(node.Path.StringPath);

                    folder.ReadOnly = attributes.IsReadOnly;

                    _uow.UserFolders.Update(folder);
                    _uow.Commit();

                    var directoryPath = UtilConvertSqlToPathForFolder(folder);

                    Log.Information($"SETATTRIBUTES for directory '{directoryPath}'");

                    return node;
                }
                else if (node.NodeType == NodeType.File)
                {
                    var file = UtilConvertPathToSqlForFile(node.Path.StringPath);

                    file.ReadOnly = attributes.IsReadOnly;

                    _uow.UserFiles.Update(file);
                    _uow.Commit();

                    var filePath = UtilConvertSqlToPathForFile(file);

                    Log.Information($"SETATTRIBUTES for file '{filePath}'");

                    return node;
                }
                else
                    throw new NotImplementedException();
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

                MemoryStream stream = new MemoryStream();

                if (node.NodeType == NodeType.File)
                {
                    var file = UtilConvertPathToSqlForFile(node.Path.StringPath);
                    var moment = DateTime.UtcNow;

                    realDirectoryPath = new DirectoryInfo(_conf["Storage:LocalBasePath"]
                        + Path.DirectorySeparatorChar + file.RealPath);

                    realFilePath = new FileInfo(_conf["Storage:LocalBasePath"]
                        + Path.DirectorySeparatorChar + file.RealPath
                        + Path.DirectorySeparatorChar + file.RealFileName);

                    if (!realDirectoryPath.Exists)
                        realDirectoryPath.Create();

                    using (var fs = new FileStream(realFilePath.FullName, FileMode.CreateNew, FileAccess.Write))
                    {
                        content.GetStream().CopyTo(fs);

                        file.FileSize = fs.Length;
                        file.Created = moment;
                        file.LastAccessed = null;
                        file.LastUpdated = null;
                    }

                    using (var sha256 = new SHA256Managed())
                    using (var fs = new FileStream(realFilePath.FullName, FileMode.Open, FileAccess.Read))
                    {
                        var hash = sha256.ComputeHash(fs);

                        file.FileHashSHA256 = HashHelpers.GetHexString(hash);
                    }

                    _uow.UserFiles.Update(file);
                    _uow.Commit();

                    Log.Information($"SAVECONTENT for file '{node.Path}' to '{realFilePath.FullName}'");

                    return node;
                }
                else
                    throw new NotImplementedException();
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

        protected override NodeBase SetTimeInfo(NodeBase node, NodeTimeInfo timeInfo)
        {
            try
            {
                if (node.NodeType == NodeType.Directory)
                {
                    var folder = UtilConvertPathToSqlForFolder(node.Path.StringPath);

                    folder.LastAccessed = timeInfo.LastAccessTime;
                    folder.LastUpdated = timeInfo.LastWriteTime;

                    _uow.UserFolders.Update(folder);
                    _uow.Commit();

                    var directoryPath = UtilConvertSqlToPathForFolder(folder);

                    Log.Information($"SETTIMEINFO for directory '{directoryPath}'");

                    return node;
                }
                else if (node.NodeType == NodeType.File)
                {
                    var file = UtilConvertPathToSqlForFile(node.Path.StringPath);

                    file.LastAccessed = timeInfo.LastAccessTime;
                    file.LastUpdated = timeInfo.LastWriteTime;

                    _uow.UserFiles.Update(file);
                    _uow.Commit();

                    var filePath = UtilConvertSqlToPathForFile(file);

                    Log.Information($"SETTIMEINFO for file '{filePath}'");

                    return node;
                }
                else
                    throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        internal tbl_UserFolders UtilConvertPathToSqlForFolder(string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var folder = _uow.UserFolders.Get(x => x.UserId == _userId
                && x.ParentId == null).SingleOrDefault();

            if (string.IsNullOrWhiteSpace(path))
                return folder;

            foreach (var entry in path.Split("/"))
            {
                folder = _uow.UserFolders.Get(x => x.UserId == _userId
                    && x.ParentId == folder.Id
                    && x.VirtualName == entry).SingleOrDefault();
            };

            return folder;
        }

        internal tbl_UserFiles UtilConvertPathToSqlForFile(string path)
        {
            if (path.FirstOrDefault() == '/')
                path = path.Substring(1);

            var pathBits = path.Split("/");
            var filePath = path.Split("/").Last();
            var folderPath = string.Empty;

            for (int i = 0; i <= pathBits.Count() - 2; i++)
                folderPath += "/" + pathBits.ElementAt(i);

            var folder = UtilConvertPathToSqlForFolder(folderPath);

            var file = _uow.UserFiles.Get(x => x.UserId == _userId
                && x.FolderId == folder.Id
                && x.VirtualName == filePath).SingleOrDefault();

            return file;
        }

        internal string UtilConvertSqlToPathForFolder(tbl_UserFolders folder)
        {
            var path = string.Empty;
            var paths = new List<string> { };

            while (folder.ParentId != null)
            {
                paths.Add(folder.VirtualName);
                folder = folder.Parent;
            }

            for (int i = paths.Count() - 1; i >= 0; i--)
                path += "/" + paths.ElementAt(i);

            return path;
        }

        internal string UtilConvertSqlToPathForFile(tbl_UserFiles file)
        {
            var path = string.Empty;
            var paths = new List<string> { };

            var folder = _uow.UserFolders.Get(x => x.UserId == _userId
                && x.Id == file.FolderId).Single();

            while (folder.ParentId != null)
            {
                paths.Add(folder.VirtualName);
                folder = folder.Parent;
            }

            for (int i = paths.Count() - 1; i >= 0; i--)
                path += "/" + paths.ElementAt(i);

            path += "/" + file.VirtualName;

            return path;
        }

        internal void UtilEnsureUserHasRootFolder()
        {
            var folder = _uow.UserFolders.Get(x => x.UserId == _userId
                && x.ParentId == null).SingleOrDefault();

            if (folder == null)
            {
                var moment = DateTime.UtcNow;

                var newFolder = _uow.UserFolders.Create(
                    new tbl_UserFolders
                    {
                        Id = Guid.NewGuid(),
                        UserId = _userId,
                        ParentId = null,
                        VirtualName = string.Empty,
                        Created = moment,
                        LastAccessed = null,
                        LastUpdated = null,
                        ReadOnly = true,
                    });
                _uow.Commit();
            }
        }
    }
}
