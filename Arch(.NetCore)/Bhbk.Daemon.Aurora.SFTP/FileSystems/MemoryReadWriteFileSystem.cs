using Bhbk.Daemon.Aurora.SFTP.Factories;
using Bhbk.Lib.Aurora.Data.ModelsMem;
using Bhbk.Lib.Aurora.Data.UnitOfWorkMem;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Common.Primitives;
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
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Hashing = Bhbk.Lib.Cryptography.Hashing;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class MemoryReadWriteFileSystem : ReadWriteFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private readonly IUnitOfWorkMem _uowMem;
        private E_LoginMem _userMem;

        internal MemoryReadWriteFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, Login_EF user)
            : base(settings)
        {
            _factory = factory;

            using (var scope = _factory.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                _uowMem = new UnitOfWorkMem(conf["Databases:AuroraEntities_EFCore_Mem"]);
            }

            _userMem = MemoryPathFactory.CheckUser(_uowMem, user);
            _userMem = MemoryPathFactory.CheckContent(_uowMem, _userMem);
            _userMem.QuotaInBytes = 104857600;
            _userMem.QuotaUsedInBytes = 0;

            MemoryPathFactory.CheckFolder(_uowMem, _userMem);

            //var folderKeysNode = new DirectoryNode(".ssh", Root);
            //var fileKeysNode = new FileNode("authorized_keys", folderKeysNode);

            //if (!Exists(folderKeysNode.Path, NodeType.Directory))
            //    CreateDirectory(Root, folderKeysNode);

            //if (Exists(fileKeysNode.Path, NodeType.File))
            //    Delete(fileKeysNode);

            //var pubKeysContent = KeyHelper.ExportPubKeyBase64(user, user.PublicKeys);

            //CreateFile(folderKeysNode, fileKeysNode);
            //SaveContent(fileKeysNode, NodeContent.CreateDelayedWriteContent(
            //    new MemoryStream(Encoding.UTF8.GetBytes(pubKeysContent.ToString()))));
        }

        protected override DirectoryNode CreateDirectory(DirectoryNode parent, DirectoryNode child)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                var folderEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, parent.Path.StringPath);
                var now = DateTime.UtcNow;

                _uowMem.Folders.Create(
                    new E_FolderMem
                    {
                        Id = Guid.NewGuid(),
                        UserId = _userMem.UserId,
                        ParentId = folderEntity.Id,
                        VirtualName = child.Name,
                        IsReadOnly = false,
                        CreatedUtc = now,
                        LastAccessedUtc = now,
                        LastUpdatedUtc = now,
                    });
                _uowMem.Commit();

                Log.Information($"'{callPath}' '{_userMem.UserName}' folder:'{child.Path}' at:memory");

                return child;
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

            try
            {
                /*
                 * enforce quota if user is already over. we do not know size of incoming strea until has
                 * all been received. quota enforcement not possible until after exceeded.
                 */

                if (_userMem.QuotaUsedInBytes >= _userMem.QuotaInBytes)
                    throw new FileSystemOperationCanceledException($"'{callPath}' '{_userMem.UserName}' file:'{child.Path}' size:'{child.Length / 1048576f}MB' " +
                        $"at:memory quota-maximum:'{_userMem.QuotaInBytes / 1048576f}MB' quota-used:'{_userMem.QuotaUsedInBytes / 1048576f}MB'");

                var folderEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, parent.Path.StringPath);
                var fileName = Hashing.MD5.Create(Guid.NewGuid().ToString());
                var now = DateTime.UtcNow;

                var fileEntity = new E_FileMem
                {
                    Id = Guid.NewGuid(),
                    UserId = _userMem.UserId,
                    FolderId = folderEntity.Id,
                    VirtualName = child.Name,
                    IsReadOnly = false,
                    CreatedUtc = now,
                    LastAccessedUtc = now,
                    LastUpdatedUtc = now,
                };

                /*
                 * a zero size file will always be created first regardless of actual size of file. 
                 */

                using (var sha256 = new SHA256Managed())
                using (var fs = new MemoryStream())
                {
                    var hash = sha256.ComputeHash(fs);

                    fileEntity.HashSHA256 = Strings.GetHexString(hash);
                }

                _uowMem.Files.Create(fileEntity);
                _uowMem.Commit();

                Log.Information($"'{callPath}' '{_userMem.UserName}' empty-file:'{child.Path}' at:memory");

                return child;
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

                switch (node.NodeType)
                {
                    case NodeType.File:
                        {
                            var fileEntity = MemoryPathFactory.PathToFile(_uowMem, _userMem, node.Path.StringPath);

                            _userMem.QuotaUsedInBytes -= fileEntity.Data.Length;

                            _uowMem.Files.Delete(fileEntity);
                            _uowMem.Commit();

                            Log.Information($"'{callPath}' '{_userMem.UserName}' file:'{node.Path}' at:memory");
                        }
                        break;

                    case NodeType.Directory:
                        {
                            var folderEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, node.Path.StringPath);

                            _uowMem.Folders.Delete(folderEntity);
                            _uowMem.Commit();

                            Log.Information($"'{callPath}' '{_userMem.UserName}' folder:'{node.Path}' at:memory");
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return node;
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
                switch (nodeType)
                {
                    case NodeType.File:
                        {
                            var fileEntity = MemoryPathFactory.PathToFile(_uowMem, _userMem, path.StringPath);

                            if (fileEntity != null)
                                return true;

                            return false;
                        }

                    case NodeType.Directory:
                        {
                            var folderEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, path.StringPath);

                            if (folderEntity != null)
                                return true;

                            return false;
                        }

                    default:
                        throw new NotImplementedException();
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
                switch (node.NodeType)
                {
                    case NodeType.File:
                        {
                            var fileEntity = MemoryPathFactory.PathToFile(_uowMem, _userMem, node.Path.StringPath);

                            if (fileEntity.IsReadOnly)
                                node.SetAttributes(new NodeAttributes(FileAttributes.Normal | FileAttributes.ReadOnly));

                            node.SetAttributes(new NodeAttributes(FileAttributes.Normal));
                        }
                        break;

                    case NodeType.Directory:
                        {
                            var folderEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, node.Path.StringPath);

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

                var folderParentEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, parent.Path.StringPath);

                var folderEntity = _uowMem.Folders.Get(QueryExpressionFactory.GetQueryExpression<E_FolderMem>()
                    .Where(x => x.UserId == _userMem.UserId && x.ParentId == folderParentEntity.Id && x.VirtualName == name).ToLambda())
                    .SingleOrDefault();

                if (folderEntity != null)
                    child = new DirectoryNode(folderEntity.VirtualName, parent,
                        new NodeTimeInfo(folderEntity.CreatedUtc.UtcDateTime,
                            folderEntity.LastAccessedUtc.UtcDateTime, folderEntity.LastUpdatedUtc.UtcDateTime));

                var fileEntity = _uowMem.Files.Get(QueryExpressionFactory.GetQueryExpression<E_FileMem>()
                    .Where(x => x.UserId == _userMem.UserId && x.FolderId == folderParentEntity.Id && x.VirtualName == name).ToLambda())
                    .SingleOrDefault();

                if (fileEntity != null)
                    child = new FileNode(fileEntity.VirtualName, parent,
                        new NodeTimeInfo(fileEntity.CreatedUtc.UtcDateTime,
                            fileEntity.LastAccessedUtc.UtcDateTime, fileEntity.LastUpdatedUtc.UtcDateTime));

                return child;
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

                var folderParentEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, parent.Path.StringPath);

                var folders = _uowMem.Folders.Get(QueryExpressionFactory.GetQueryExpression<E_FolderMem>()
                    .Where(x => x.UserId == _userMem.UserId).ToLambda())
                    .ToList();

                foreach (var folder in folders.Where(x => x.UserId == _userMem.UserId && x.ParentId == folderParentEntity.Id))
                    children.Add(new DirectoryNode(folder.VirtualName, parent,
                        new NodeTimeInfo(folder.CreatedUtc.UtcDateTime,
                            folder.LastAccessedUtc.UtcDateTime, folder.LastUpdatedUtc.UtcDateTime)));

                var files = _uowMem.Files.Get(QueryExpressionFactory.GetQueryExpression<E_FileMem>()
                    .Where(x => x.UserId == _userMem.UserId).ToLambda())
                    .ToList();

                foreach (var file in files.Where(x => x.UserId == _userMem.UserId && x.FolderId == folderParentEntity.Id))
                    children.Add(new FileNode(file.VirtualName, parent,
                        new NodeTimeInfo(file.CreatedUtc.UtcDateTime,
                            file.LastAccessedUtc.UtcDateTime, file.LastUpdatedUtc.UtcDateTime)));

                return children;
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
                switch (node.NodeType)
                {
                    case NodeType.File:
                        {
                            MemoryStream stream = null;

                            var fileEntity = MemoryPathFactory.PathToFile(_uowMem, _userMem, node.Path.StringPath);

                            if (fileEntity.Data == null)
                                stream = new MemoryStream();
                            else
                                stream = new MemoryStream(fileEntity.Data);

                            Log.Information($"'{callPath}' '{_userMem.UserName}' file:'{node.Path}' size:'{stream.Length / 1048576f}MB' at:memory");

                            return parameters.AccessType == NodeContentAccess.Read
                                ? NodeContent.CreateReadOnlyContent(stream)
                                : NodeContent.CreateDelayedWriteContent(stream);
                        }

                    default:
                        throw new NotImplementedException();
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
                switch (node.NodeType)
                {
                    case NodeType.File:
                        {
                            var fileEntity = MemoryPathFactory.PathToFile(_uowMem, _userMem, node.Path.StringPath);

                            if (fileEntity.Data == null)
                                return 0L;

                            return fileEntity.Data.Length;
                        }

                    case NodeType.Directory:
                        {
                            return 0L;
                        }

                    default:
                        throw new NotImplementedException();
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
                switch (node.NodeType)
                {
                    case NodeType.File:
                        {
                            var fileEntity = MemoryPathFactory.PathToFile(_uowMem, _userMem, node.Path.StringPath);

                            node.SetTimeInfo(new NodeTimeInfo(fileEntity.CreatedUtc.UtcDateTime,
                                fileEntity.LastAccessedUtc.UtcDateTime, fileEntity.LastUpdatedUtc.UtcDateTime));
                        }
                        break;

                    case NodeType.Directory:
                        {
                            var folderEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, node.Path.StringPath);

                            node.SetTimeInfo(new NodeTimeInfo(folderEntity.CreatedUtc.UtcDateTime,
                                folderEntity.LastAccessedUtc.UtcDateTime, folderEntity.LastUpdatedUtc.UtcDateTime));
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return node.TimeInfo;
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

                switch (toBeMovedNode.NodeType)
                {
                    case NodeType.File:
                        {
                            var toBeMovedEntity = MemoryPathFactory.PathToFile(_uowMem, _userMem, toBeMovedNode.Path.StringPath);
                            var toBeMovedPath = MemoryPathFactory.FileToPath(_uowMem, _userMem, toBeMovedEntity);

                            var targetEntity = MemoryPathFactory.PathToFile(_uowMem, _userMem, targetDirectory.Path.StringPath);
                            var targetPath = MemoryPathFactory.FileToPath(_uowMem, _userMem, targetEntity);

                            toBeMovedEntity.FolderId = targetEntity.Id;

                            _uowMem.Files.Update(toBeMovedEntity);
                            _uowMem.Commit();

                            Log.Information($"'{callPath}' '{_userMem.UserName}' from-file:'{toBeMovedPath}' to-file:'{targetPath}' at:memory");

                            return new FileNode(toBeMovedNode.Name, targetDirectory);
                        }

                    case NodeType.Directory:
                        {
                            var toBeMovedEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, toBeMovedNode.Path.StringPath);
                            var toBeMovedPath = MemoryPathFactory.FolderToPath(_uowMem, _userMem, toBeMovedEntity);

                            var targetEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, targetDirectory.Path.StringPath);
                            var targetPath = MemoryPathFactory.FolderToPath(_uowMem, _userMem, targetEntity);

                            toBeMovedEntity.ParentId = targetEntity.Id;

                            _uowMem.Folders.Update(toBeMovedEntity);
                            _uowMem.Commit();

                            Log.Information($"'{callPath}' '{_userMem.UserName}' from-folder:'{toBeMovedPath}' to-folder:'{targetPath}' at:memory");

                            return new DirectoryNode(toBeMovedNode.Name, targetDirectory);
                        }

                    default:
                        throw new NotImplementedException();
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

                switch (node.NodeType)
                {
                    case NodeType.File:
                        {
                            var fileEntity = MemoryPathFactory.PathToFile(_uowMem, _userMem, node.Path.StringPath);

                            fileEntity.VirtualName = newName;
                            fileEntity.LastUpdatedUtc = DateTime.UtcNow;

                            _uowMem.Files.Update(fileEntity);
                            _uowMem.Commit();

                            Log.Information($"'{callPath}' '{_userMem.UserName}' from-file:'{node.Path}' to-file:'{node.Parent.Path}/{newName}' at:memory");

                            return new FileNode(newName, node.Parent);
                        }

                    case NodeType.Directory:
                        {
                            var folderEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, node.Path.StringPath);

                            folderEntity.VirtualName = newName;
                            folderEntity.LastUpdatedUtc = DateTime.UtcNow;

                            _uowMem.Folders.Update(folderEntity);
                            _uowMem.Commit();

                            Log.Information($"'{callPath}' '{_userMem.UserName}' from-folder:'{node.Path}' to-folder:'{node.Parent.Path}{newName}' at:memory");

                            return new DirectoryNode(newName, node.Parent);
                        }

                    default:
                        throw new NotImplementedException();
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

            try
            {
                switch (node.NodeType)
                {
                    case NodeType.File:
                        {
                            var fileEntity = MemoryPathFactory.PathToFile(_uowMem, _userMem, node.Path.StringPath);

                            using (var fs = new MemoryStream())
                            {
                                content.GetStream().CopyTo(fs);
                                fileEntity.Data = fs.ToArray();
                            }

                            using (var sha256 = new SHA256Managed())
                            using (var fs = new MemoryStream(fileEntity.Data))
                            {
                                var hash = sha256.ComputeHash(fs);

                                fileEntity.HashSHA256 = Strings.GetHexString(hash);
                            }

                            _userMem.QuotaUsedInBytes += content.Length;

                            _uowMem.Files.Update(fileEntity);
                            _uowMem.Commit();

                            Log.Information($"'{callPath}' '{_userMem.UserName}' file:'{node.Path}' size:'{content.Length / 1048576f}MB' at:memory");
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return node;
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
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }
    }
}