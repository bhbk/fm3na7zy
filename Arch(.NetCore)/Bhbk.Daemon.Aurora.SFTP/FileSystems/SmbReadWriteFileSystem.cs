using Bhbk.Daemon.Aurora.SFTP.Helpers;
using Bhbk.Lib.Aurora.Data_EF6.Infrastructure;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Cryptography.Encryption;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32.SafeHandles;
using Rebex.IO.FileSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Text;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class SmbReadWriteFileSystem : ReadWriteFileSystemProvider
    {
        private readonly SafeAccessTokenHandle _userToken;
        private readonly User _userEntity;
        private readonly string _userMount;

        internal SmbReadWriteFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, User userEntity,
            string identityUser, string identityPass)
            : base(settings)
        {
            _userEntity = userEntity;

            /*
             * this file-system is functional only when the daemon is running a on windows platform. there is 
             * an interop call required to obtain a user credential outside the context of what the daemon runs as.
             */

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotImplementedException();

            var folderKeysNode = new DirectoryNode(".ssh", Root);
            var fileKeysNode = new FileNode("authorized_users", folderKeysNode);

            using (var scope = factory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                var userMount = uow.UserMounts.Get(QueryExpressionFactory.GetQueryExpression<UserMount>()
                    .Where(x => x.IdentityId == _userEntity.IdentityId).ToLambda())
                    .Single();

                _userMount = userMount.ServerAddress + userMount.ServerShare;

                if (userMount.CredentialId.HasValue)
                {
                    var userCred = uow.Credentials.Get(QueryExpressionFactory.GetQueryExpression<Credential>()
                        .Where(x => x.Id == userMount.CredentialId).ToLambda())
                        .Single();

                    var secret = conf["Databases:AuroraSecret"];

                    var plainText = AES.DecryptString(userCred.Password, secret);
                    var cipherText = AES.EncryptString(plainText, secret);

                    if (userCred.Password != cipherText)
                        throw new UnauthorizedAccessException();

                    _userToken = UserHelper.GetSafeAccessTokenHandle(userCred.Domain, userCred.UserName, plainText);
                }
                else
                {
                    _userToken = UserHelper.GetSafeAccessTokenHandle(null, identityUser, identityPass);
                }
            }

            if (!Exists(folderKeysNode.Path, NodeType.Directory))
                CreateDirectory(Root, folderKeysNode);

            if (Exists(fileKeysNode.Path, NodeType.File))
                Delete(fileKeysNode);

            var pubKeysContent = KeyHelper.ExportPubKeyBase64(_userEntity, _userEntity.PublicKeys);

            CreateFile(folderKeysNode, fileKeysNode);
            SaveContent(fileKeysNode, NodeContent.CreateDelayedWriteContent(
                new MemoryStream(Encoding.UTF8.GetBytes(pubKeysContent.ToString()))));
        }

        [SupportedOSPlatform("windows")]
        protected override DirectoryNode CreateDirectory(DirectoryNode parent, DirectoryNode child)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    var folder = SmbFileSystemHelper.FolderPathToCIFS(_userMount + child.Path.StringPath);
                    folder.Create();

                    Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' folder '{child.Path}' at '{folder.FullName}'" +
                        $" run as '{WindowsIdentity.GetCurrent().Name}'");
                });

                return child;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override FileNode CreateFile(DirectoryNode parent, FileNode child)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    var folder = SmbFileSystemHelper.FolderPathToCIFS(_userMount + parent.Path.StringPath);

                    if (!folder.Exists)
                        folder.Create();

                    var file = SmbFileSystemHelper.FilePathToCIFS(_userMount + child.Path.StringPath);

                    /*
                     * a zero size file will always be created first regardless of actual size of file. 
                     */

                    using (var fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)) { }

                    Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' empty file '{child.Path}' at '{file.FullName}'" +
                        $" run as '{WindowsIdentity.GetCurrent().Name}'");
                });

                return child;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override NodeBase Delete(NodeBase node)
        {
            if (!node.Exists())
                return node;

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                switch (node.NodeType)
                {
                    case NodeType.File:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {
                            var file = SmbFileSystemHelper.FilePathToCIFS(_userMount + node.Path.StringPath);
                            file.Delete();

                            Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' file '{node.Path}' from '{file.FullName}'" +
                                $" run as '{WindowsIdentity.GetCurrent().Name}'");
                        });
                        break;

                    case NodeType.Directory:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {
                            var folder = SmbFileSystemHelper.FolderPathToCIFS(_userMount + node.Path.StringPath);
                            folder.Delete();

                            Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' folder '{node.Path}' from '{folder.FullName}'" +
                                $" run as '{WindowsIdentity.GetCurrent().Name}'");
                        });
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

        [SupportedOSPlatform("windows")]
        protected override bool Exists(NodePath path, NodeType nodeType)
        {
            try
            {
                bool exists = false;

                switch (nodeType)
                {
                    case NodeType.File:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {
                            var file = SmbFileSystemHelper.FilePathToCIFS(_userMount + path.StringPath);

                            if (file.Exists)
                                exists = true;
                        });
                        break;

                    case NodeType.Directory:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {
                            var folder = SmbFileSystemHelper.FolderPathToCIFS(_userMount + path.StringPath);

                            if (folder.Exists)
                                exists = true;
                        });
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return exists;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override NodeAttributes GetAttributes(NodeBase node)
        {
            if (!node.Exists())
                return node.Attributes;

            try
            {
                switch (node.NodeType)
                {
                    case NodeType.File:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {
                            var file = SmbFileSystemHelper.FilePathToCIFS(_userMount + node.Path.StringPath);

                            node.SetAttributes(new NodeAttributes(file.Attributes));
                        });
                        break;

                    case NodeType.Directory:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {
                            var folder = SmbFileSystemHelper.FolderPathToCIFS(_userMount + node.Path.StringPath);

                            node.SetAttributes(new NodeAttributes(folder.Attributes));
                        });
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

        [SupportedOSPlatform("windows")]
        protected override NodeBase GetChild(string name, DirectoryNode parent)
        {
            try
            {
                NodeBase child = null;

                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    var folder = SmbFileSystemHelper.FolderPathToCIFS(_userMount + parent.Path.StringPath
                        + Path.DirectorySeparatorChar + name);

                    if (folder.Exists)
                        child = new DirectoryNode(name, parent,
                            new NodeTimeInfo(folder.CreationTime, folder.LastAccessTime, folder.LastWriteTime));

                    var file = SmbFileSystemHelper.FilePathToCIFS(_userMount + parent.Path.StringPath
                        + Path.DirectorySeparatorChar + name);

                    if (file.Exists)
                        child = new FileNode(name, parent,
                            new NodeTimeInfo(file.CreationTime, file.LastAccessTime, file.LastWriteTime));
                });

                return child;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override IEnumerable<NodeBase> GetChildren(DirectoryNode parent, NodeType nodeType)
        {
            if (!parent.Exists())
                return Enumerable.Empty<NodeBase>();

            try
            {
                var children = new List<NodeBase>();

                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    var folderParent = SmbFileSystemHelper.FolderPathToCIFS(_userMount + parent.Path.StringPath);

                    foreach (var folderPath in Directory.GetDirectories(folderParent.FullName))
                    {
                        var folder = new DirectoryInfo(folderPath);

                        children.Add(new DirectoryNode(folder.Name, parent,
                            new NodeTimeInfo(folder.CreationTime, folder.LastAccessTime, folder.LastWriteTime)));
                    }

                    foreach (var filePath in Directory.GetFiles(folderParent.FullName))
                    {
                        var file = new FileInfo(filePath);

                        children.Add(new FileNode(file.Name, parent,
                            new NodeTimeInfo(file.CreationTime, file.LastAccessTime, file.LastWriteTime)));
                    }
                });

                return children;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override NodeContent GetContent(NodeBase node, NodeContentParameters contentParameters)
        {
            if (!node.Exists())
                return NodeContent.CreateDelayedWriteContent(new MemoryStream());

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                NodeContent content = null;

                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    var file = SmbFileSystemHelper.FilePathToCIFS(_userMount + node.Path.StringPath);

                    var stream = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

                    content = contentParameters.AccessType == NodeContentAccess.Read
                        ? NodeContent.CreateReadOnlyContent(stream)
                        : NodeContent.CreateDelayedWriteContent(stream);

                    Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' file '{node.Path}' from '{file.FullName}'");
                });

                return content;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override long GetLength(NodeBase node)
        {
            if (!node.Exists())
                return 0L;

            try
            {
                long length = 0L;

                switch (node.NodeType)
                {
                    case NodeType.File:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {
                            var file = SmbFileSystemHelper.FilePathToCIFS(_userMount + node.Path.StringPath);

                            length = file.Length;
                        });
                        break;

                    case NodeType.Directory:
                        length = 0L;
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return length;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override NodeTimeInfo GetTimeInfo(NodeBase node)
        {
            try
            {
                switch (node.NodeType)
                {
                    case NodeType.File:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {
                            var file = SmbFileSystemHelper.FilePathToCIFS(_userMount + node.Path.StringPath);

                            node.SetTimeInfo(new NodeTimeInfo(file.CreationTime, file.LastAccessTime, file.LastWriteTime));
                        });
                        break;

                    case NodeType.Directory:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {
                            var folder = SmbFileSystemHelper.FolderPathToCIFS(_userMount + node.Path.StringPath);

                            node.SetTimeInfo(new NodeTimeInfo(folder.CreationTime, folder.LastAccessTime, folder.LastWriteTime));
                        });
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

        [SupportedOSPlatform("windows")]
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
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {
                            var file = SmbFileSystemHelper.FilePathToCIFS(_userMount + toBeMovedNode.Path.StringPath);
                            var newPath = SmbFileSystemHelper.FilePathToCIFS(_userMount + targetDirectory.Path.StringPath);

                            file.MoveTo(newPath.FullName);

                            Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' from '{file.Name}' [{file.FullName}] to '{newPath.Name}' [{newPath.FullName}]" +
                                $" run as '{WindowsIdentity.GetCurrent().Name}'");
                        });

                        toBeMovedNode = new FileNode(toBeMovedNode.Name, targetDirectory);
                        break;

                    case NodeType.Directory:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {
                            var folder = SmbFileSystemHelper.FolderPathToCIFS(_userMount + toBeMovedNode.Path.StringPath);
                            var newPath = SmbFileSystemHelper.FolderPathToCIFS(_userMount + targetDirectory.Path.StringPath);

                            folder.MoveTo(newPath.FullName);

                            Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' from '{folder.Name}' [{folder.FullName}] to '{newPath.Name}' [{newPath.FullName}]" +
                                $" run as '{WindowsIdentity.GetCurrent().Name}'");
                        });

                        toBeMovedNode = new DirectoryNode(toBeMovedNode.Name, targetDirectory);
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return toBeMovedNode;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override NodeBase Rename(NodeBase node, string newName)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                NodeBase toBeNamedNode = null;

                switch (node.NodeType)
                {
                    case NodeType.File:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {
                            var file = SmbFileSystemHelper.FilePathToCIFS(_userMount + node.Path.StringPath);
                            var newFile = SmbFileSystemHelper.FilePathToCIFS(_userMount + node.Parent.Path.StringPath
                                + Path.DirectorySeparatorChar + newName);

                            file.MoveTo(newFile.FullName);

                            Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' from '{file.Name}' [{file.FullName}] to '{newFile.Name}' [{newFile.FullName}]" +
                                $" run as '{WindowsIdentity.GetCurrent().Name}'");
                        });

                        toBeNamedNode = new FileNode(newName, node.Parent);
                        break;

                    case NodeType.Directory:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {
                            var folder = SmbFileSystemHelper.FolderPathToCIFS(_userMount + node.Path.StringPath);
                            var newFolder = SmbFileSystemHelper.FolderPathToCIFS(_userMount + node.Parent.Path.StringPath
                                + Path.DirectorySeparatorChar + newName);

                            folder.MoveTo(newFolder.FullName);

                            Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' from '{folder.Name}' [{folder.FullName}] to '{newFolder.Name}' [{newFolder.FullName}]" +
                                $" run as '{WindowsIdentity.GetCurrent().Name}'");
                        });

                        toBeNamedNode = new DirectoryNode(newName, node.Parent);
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return toBeNamedNode;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
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
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {
                            var folder = SmbFileSystemHelper.FolderPathToCIFS(_userMount + node.Parent.Path.StringPath);

                            if (!folder.Exists)
                                folder.Create();

                            var file = SmbFileSystemHelper.FilePathToCIFS(_userMount + node.Path.StringPath);

                            using (var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                                content.GetStream().CopyTo(fs);

                            Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' file '{node.Path}' at '{file.FullName}'" +
                                $" run as '{WindowsIdentity.GetCurrent().Name}'");
                        });
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

        [SupportedOSPlatform("windows")]
        protected override NodeBase SetAttributes(NodeBase node, NodeAttributes attributes)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                switch (node.NodeType)
                {
                    case NodeType.File:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {

                        });
                        break;

                    case NodeType.Directory:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {

                        });
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

        [SupportedOSPlatform("windows")]
        protected override NodeBase SetTimeInfo(NodeBase node, NodeTimeInfo timeInfo)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                switch (node.NodeType)
                {
                    case NodeType.File:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {

                        });
                        break;

                    case NodeType.Directory:
                        WindowsIdentity.RunImpersonated(_userToken, () =>
                        {

                        });
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
