using Bhbk.Daemon.Aurora.SFTP.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data.ModelsMem;
using Bhbk.Lib.Aurora.Data.UnitOfWorkMem;
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
        private User _user;
        private UserMem _userMem;

        internal MemoryReadWriteFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, User user)
            : base(settings)
        {
            _factory = factory;
            _user = user;

            using (var scope = _factory.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                _uowMem = new UnitOfWorkMem(conf["Databases:AuroraEntitiesMem"]);
            }

            _userMem = MemoryPathFactory.CheckUser(_uowMem, _user);
            _userMem = MemoryPathFactory.CheckContent(_uowMem, _userMem);

            MemoryPathFactory.CheckRoot(_uowMem, _userMem);

            var folderKeysNode = new DirectoryNode(".ssh", Root);
            var fileKeysNode = new FileNode("authorized_users", folderKeysNode);

            if (!Exists(folderKeysNode.Path, NodeType.Directory))
                CreateDirectory(Root, folderKeysNode);

            if (Exists(fileKeysNode.Path, NodeType.File))
                Delete(fileKeysNode);

            var pubKeysContent = KeyHelper.ExportPubKeyBase64(_user, _user.PublicKeys);

            CreateFile(folderKeysNode, fileKeysNode);
            SaveContent(fileKeysNode, NodeContent.CreateDelayedWriteContent(
                new MemoryStream(Encoding.UTF8.GetBytes(pubKeysContent.ToString()))));
        }

        protected override DirectoryNode CreateDirectory(DirectoryNode parent, DirectoryNode child)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                var folderEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, parent.Path.StringPath);
                var now = DateTime.UtcNow;

                _uowMem.UserFolders.Create(
                    new UserFolderMem
                    {
                        Id = Guid.NewGuid(),
                        IdentityId = _user.IdentityId,
                        ParentId = folderEntity.Id,
                        VirtualName = child.Name,
                        IsReadOnly = false,
                        CreatedUtc = now,
                        LastAccessedUtc = now,
                        LastUpdatedUtc = now,
                    });
                _uowMem.Commit();

                Log.Information($"'{callPath}' '{_userMem.IdentityAlias}' directory '{child.Path}' in memory");

                return child;
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

            try
            {
                if (_user.QuotaUsedInBytes >= _user.QuotaInBytes)
                {
                    Log.Warning($"'{callPath}' '{_userMem.IdentityAlias}' file '{child.Path}' cancelled, " +
                        $"totoal quota '{_user.QuotaInBytes}' used quota '{_user.QuotaUsedInBytes}'");

                    throw new FileSystemOperationCanceledException();
                }

                var folderEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, parent.Path.StringPath);
                var fileName = Hashing.MD5.Create(Guid.NewGuid().ToString());
                var now = DateTime.UtcNow;

                var fileEntity = new UserFileMem
                {
                    Id = Guid.NewGuid(),
                    IdentityId = _userMem.IdentityId,
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

                _uowMem.UserFiles.Create(fileEntity);
                _uowMem.Commit();

                Log.Information($"'{callPath}' '{_userMem.IdentityAlias}' empty file '{child.Path}' in memory");

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

                            _user.QuotaUsedInBytes -= fileEntity.Data.Length;

                            _uowMem.UserFiles.Delete(fileEntity);
                            _uowMem.Commit();

                            Log.Information($"'{callPath}' '{_userMem.IdentityAlias}' file '{node.Path}' from memory");
                        }
                        break;

                    case NodeType.Directory:
                        {
                            var folderEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, node.Path.StringPath);

                            _uowMem.UserFolders.Delete(folderEntity);
                            _uowMem.Commit();

                            Log.Information($"'{callPath}' '{_userMem.IdentityAlias}' folder '{node.Path}' from memory");
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return node;
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
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeBase GetChild(string name, DirectoryNode parent)
        {
            try
            {
                NodeBase child = null;

                var folderParentEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, parent.Path.StringPath);

                var folderEntity = _uowMem.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolderMem>()
                    .Where(x => x.IdentityId == _userMem.IdentityId && x.ParentId == folderParentEntity.Id && x.VirtualName == name).ToLambda())
                    .SingleOrDefault();

                if (folderEntity != null)
                    child = new DirectoryNode(folderEntity.VirtualName, parent,
                        new NodeTimeInfo(folderEntity.CreatedUtc.UtcDateTime,
                            folderEntity.LastAccessedUtc.UtcDateTime, folderEntity.LastUpdatedUtc.UtcDateTime));

                var fileEntity = _uowMem.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<UserFileMem>()
                    .Where(x => x.IdentityId == _userMem.IdentityId && x.FolderId == folderParentEntity.Id && x.VirtualName == name).ToLambda())
                    .SingleOrDefault();

                if (fileEntity != null)
                    child = new FileNode(fileEntity.VirtualName, parent,
                        new NodeTimeInfo(fileEntity.CreatedUtc.UtcDateTime,
                            fileEntity.LastAccessedUtc.UtcDateTime, fileEntity.LastUpdatedUtc.UtcDateTime));

                return child;
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

                var folderParentEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, parent.Path.StringPath);

                var folders = _uowMem.UserFolders.Get(QueryExpressionFactory.GetQueryExpression<UserFolderMem>()
                    .Where(x => x.IdentityId == _userMem.IdentityId).ToLambda())
                    .ToList();

                foreach (var folder in folders.Where(x => x.IdentityId == _userMem.IdentityId && x.ParentId == folderParentEntity.Id))
                    children.Add(new DirectoryNode(folder.VirtualName, parent,
                        new NodeTimeInfo(folder.CreatedUtc.UtcDateTime,
                            folder.LastAccessedUtc.UtcDateTime, folder.LastUpdatedUtc.UtcDateTime)));

                var files = _uowMem.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<UserFileMem>()
                    .Where(x => x.IdentityId == _userMem.IdentityId).ToLambda())
                    .ToList();

                foreach (var file in files.Where(x => x.IdentityId == _userMem.IdentityId && x.FolderId == folderParentEntity.Id))
                    children.Add(new FileNode(file.VirtualName, parent,
                        new NodeTimeInfo(file.CreatedUtc.UtcDateTime,
                            file.LastAccessedUtc.UtcDateTime, file.LastUpdatedUtc.UtcDateTime)));

                return children;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
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

                            Log.Information($"'{callPath}' '{_userMem.IdentityAlias}' file '{node.Path}' from memory");

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
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeTimeInfo GetTimeInfo(NodeBase node)
        {
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
                Log.Error(ex.ToString());
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

                            _uowMem.UserFiles.Update(toBeMovedEntity);
                            _uowMem.Commit();

                            Log.Information($"'{callPath}' '{_userMem.IdentityAlias}' from '{toBeMovedPath}' to '{targetPath}' in memory");

                            return new FileNode(toBeMovedNode.Name, targetDirectory);
                        }

                    case NodeType.Directory:
                        {
                            var toBeMovedEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, toBeMovedNode.Path.StringPath);
                            var toBeMovedPath = MemoryPathFactory.FolderToPath(_uowMem, _userMem, toBeMovedEntity);

                            var targetEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, targetDirectory.Path.StringPath);
                            var targetPath = MemoryPathFactory.FolderToPath(_uowMem, _userMem, targetEntity);

                            toBeMovedEntity.ParentId = targetEntity.Id;

                            _uowMem.UserFolders.Update(toBeMovedEntity);
                            _uowMem.Commit();

                            Log.Information($"'{callPath}' '{_userMem.IdentityAlias}' from '{toBeMovedPath}' to '{targetPath}' in memory");

                            return new DirectoryNode(toBeMovedNode.Name, targetDirectory);
                        }

                    default:
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
                if (node.Attributes.IsReadOnly)
                    throw new FileSystemOperationCanceledException();

                switch (node.NodeType)
                {
                    case NodeType.File:
                        {
                            var fileEntity = MemoryPathFactory.PathToFile(_uowMem, _userMem, node.Path.StringPath);

                            fileEntity.VirtualName = newName;
                            fileEntity.LastUpdatedUtc = DateTime.UtcNow;

                            _uowMem.UserFiles.Update(fileEntity);
                            _uowMem.Commit();

                            Log.Information($"'{callPath}' '{_userMem.IdentityAlias}' from '{node.Path}' to '{node.Parent.Path}/{newName}' in memory");

                            return new FileNode(newName, node.Parent);
                        }

                    case NodeType.Directory:
                        {
                            var folderEntity = MemoryPathFactory.PathToFolder(_uowMem, _userMem, node.Path.StringPath);

                            folderEntity.VirtualName = newName;
                            folderEntity.LastUpdatedUtc = DateTime.UtcNow;

                            _uowMem.UserFolders.Update(folderEntity);
                            _uowMem.Commit();

                            Log.Information($"'{callPath}' '{_userMem.IdentityAlias}' from '{node.Path}' to '{node.Parent.Path}{newName}' in memory");

                            return new DirectoryNode(newName, node.Parent);
                        }

                    default:
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

                            _user.QuotaUsedInBytes += content.Length;

                            _uowMem.UserFiles.Update(fileEntity);
                            _uowMem.Commit();

                            Log.Information($"'{callPath}' '{_userMem.IdentityAlias}' file '{node.Path}' in memory");
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return node;
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
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeBase SetTimeInfo(NodeBase node, NodeTimeInfo timeInfo)
        {
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
                Log.Error(ex.ToString());
                throw;
            }
        }
    }
}