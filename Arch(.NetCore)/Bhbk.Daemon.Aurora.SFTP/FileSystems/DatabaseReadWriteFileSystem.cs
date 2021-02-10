using Bhbk.Daemon.Aurora.SFTP.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.Common.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebex.IO.FileSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Hashing = Bhbk.Lib.Cryptography.Hashing;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class DatabaseReadWriteFileSystem : ReadWriteFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private Login_EF _user;

        internal DatabaseReadWriteFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, Login_EF user)
            : base(settings)
        {
            _factory = factory;
            _user = user;

            using (var scope = _factory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                DatabasePathFactory.CheckFolder(uow, _user);
            }

            //var folderKeysNode = new DirectoryNode(".ssh", Root);
            //var fileKeysNode = new FileNode("authorized_keys", folderKeysNode);

            //if (!Exists(folderKeysNode.Path, NodeType.Directory))
            //    CreateDirectory(Root, folderKeysNode);

            //if (Exists(fileKeysNode.Path, NodeType.File))
            //    Delete(fileKeysNode);

            //var pubKeysContent = KeyHelper.ExportPubKeyBase64(_user, _user.PublicKeys);

            //CreateFile(folderKeysNode, fileKeysNode);
            //SaveContent(fileKeysNode, NodeContent.CreateDelayedWriteContent(
            //    new MemoryStream(Encoding.UTF8.GetBytes(pubKeysContent.ToString()))));
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

                    uow.Folders.Create(
                        new Folder_EF
                        {
                            UserId = _user.UserId,
                            ParentId = folderEntity.Id,
                            VirtualName = child.Name,
                            IsReadOnly = false,
                        });
                    uow.Commit();

                    Log.Information($"'{callPath}' '{_user.UserName}' folder:'{child.Path}' at:database");

                    return child;
                }
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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

                    _user = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                        .Where(x => x.UserId == _user.UserId).ToLambda(),
                            new List<Expression<Func<Login_EF, object>>>()
                            {
                                x => x.Usage,
                            })
                        .Single();

                    var folderEntity = DatabasePathFactory.PathToFolder(uow, _user, parent.Path.StringPath);
                    var folderHash = Strings.GetDirectoryHash($"{_user.UserId}{parent.Path.StringPath}{child.Name}");
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

                    if (_user.Usage.QuotaUsedInBytes >= _user.Usage.QuotaInBytes)
                        throw new FileSystemOperationCanceledException($"'{callPath}' '{_user.UserName}' file:'{child.Path}' size:'{child.Length / 1048576f}MB' " +
                            $"at:'{file.FullName}' quota-maximum:'{_user.Usage.QuotaInBytes / 1048576f}MB' quota-used:'{_user.Usage.QuotaUsedInBytes / 1048576f}MB'");

                    var fileEntity = new File_EF
                    {
                        UserId = _user.UserId,
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
                        fileEntity.HashTypeId = (int)HashAlgorithmType_E.SHA256;
                        fileEntity.HashValue = Strings.GetHexString(hash);
                    }

                    uow.Files.Create(fileEntity);
                    uow.Commit();

                    Log.Information($"'{callPath}' '{_user.UserName}' empty-file:'{child.Path}' at:'{file.FullName}'");

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

                                _user = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                                    .Where(x => x.UserId == _user.UserId).ToLambda(),
                                        new List<Expression<Func<Login_EF, object>>>()
                                        {
                                            x => x.Usage,
                                        })
                                    .Single();

                                _user.Usage.QuotaUsedInBytes -= file.Length;

                                File.Delete(file.FullName);

                                uow.Files.Delete(fileEntity);
                                uow.Usages.Update(_user.Usage);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_user.UserName}' file:'{node.Path}' at:'{file.FullName}'");
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folderEntity = DatabasePathFactory.PathToFolder(uow, _user, node.Path.StringPath);

                                uow.Folders.Delete(folderEntity);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_user.UserName}' folder:'{node.Path}' at:database");
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
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
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

                    var folderEntity = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<Folder_EF>()
                        .Where(x => x.UserId == _user.UserId && x.ParentId == folderParentEntity.Id && x.VirtualName == name).ToLambda())
                        .SingleOrDefault();

                    if (folderEntity != null)
                        child = new DirectoryNode(folderEntity.VirtualName, parent,
                            new NodeTimeInfo(folderEntity.CreatedUtc.UtcDateTime,
                                folderEntity.LastAccessedUtc.UtcDateTime, folderEntity.LastUpdatedUtc.UtcDateTime));

                    var fileEntity = uow.Files.Get(QueryExpressionFactory.GetQueryExpression<File_EF>()
                        .Where(x => x.UserId == _user.UserId && x.FolderId == folderParentEntity.Id && x.VirtualName == name).ToLambda())
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

                    var folders = uow.Folders.Get(QueryExpressionFactory.GetQueryExpression<Folder_EF>()
                        .Where(x => x.UserId == _user.UserId).ToLambda())
                        .ToList();

                    foreach (var folder in folders.Where(x => x.UserId == _user.UserId && x.ParentId == folderParentEntity.Id))
                        children.Add(new DirectoryNode(folder.VirtualName, parent,
                            new NodeTimeInfo(folder.CreatedUtc.UtcDateTime,
                                folder.LastAccessedUtc.UtcDateTime, folder.LastUpdatedUtc.UtcDateTime)));

                    var files = uow.Files.Get(QueryExpressionFactory.GetQueryExpression<File_EF>()
                        .Where(x => x.UserId == _user.UserId).ToLambda())
                        .ToList();

                    foreach (var file in files.Where(x => x.UserId == _user.UserId && x.FolderId == folderParentEntity.Id))
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

                                Log.Information($"'{callPath}' '{_user.UserName}' file:'{node.Path}' size:'{stream.Length / 1048576f}MB' at:'{file.FullName}'");

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

                                uow.Files.Update(toBeMovedEntity);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_user.UserName}' from-file:'{toBeMovedPath}' to-file:'{targetPath}' at:database");

                                return new FileNode(toBeMovedNode.Name, targetDirectory);
                            }

                        case NodeType.Directory:
                            {
                                var toBeMovedEntity = DatabasePathFactory.PathToFolder(uow, _user, toBeMovedNode.Path.StringPath);
                                var toBeMovedPath = DatabasePathFactory.FolderToPath(uow, _user, toBeMovedEntity);

                                var targetEntity = DatabasePathFactory.PathToFolder(uow, _user, targetDirectory.Path.StringPath);
                                var targetPath = DatabasePathFactory.FolderToPath(uow, _user, targetEntity);

                                toBeMovedEntity.ParentId = targetEntity.Id;

                                uow.Folders.Update(toBeMovedEntity);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_user.UserName}' from-folder:'{toBeMovedPath}' to-folder:'{targetPath}' at:database");

                                return new DirectoryNode(toBeMovedNode.Name, targetDirectory);
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

                                uow.Files.Update(fileEntity);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_user.UserName}' from-file:'{node.Path}' to-file:'{node.Parent.Path}/{newName}' at:database");

                                return new FileNode(newName, node.Parent);
                            }

                        case NodeType.Directory:
                            {
                                var folderEntity = DatabasePathFactory.PathToFolder(uow, _user, node.Path.StringPath);

                                folderEntity.VirtualName = newName;
                                folderEntity.LastUpdatedUtc = DateTime.UtcNow;

                                uow.Folders.Update(folderEntity);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_user.UserName}' from-folder:'{node.Path}' to-folder:'{node.Parent.Path}{newName}' at:database");

                                return new DirectoryNode(newName, node.Parent);
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

                                _user = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                                    .Where(x => x.UserId == _user.UserId).ToLambda(),
                                        new List<Expression<Func<Login_EF, object>>>()
                                        {
                                            x => x.Usage,
                                        })
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
                                    fileEntity.HashTypeId = (int)HashAlgorithmType_E.SHA256;
                                    fileEntity.HashValue = Strings.GetHexString(hash);
                                }

                                _user.Usage.QuotaUsedInBytes += content.Length;

                                uow.Files.Update(fileEntity);
                                uow.Usages.Update(_user.Usage);
                                uow.Commit();

                                Log.Information($"'{callPath}' '{_user.UserName}' file:'{node.Path}' size:'{content.Length / 1048576f}MB' at:'{file.FullName}'");
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

                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        protected override NodeBase SetAttributes(NodeBase node, NodeAttributes attributes)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

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
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        protected override NodeBase SetTimeInfo(NodeBase node, NodeTimeInfo timeInfo)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

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
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }
    }
}
