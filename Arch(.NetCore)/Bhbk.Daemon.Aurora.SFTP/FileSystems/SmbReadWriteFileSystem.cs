using Bhbk.Daemon.Aurora.SFTP.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
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
using System.Net;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Principal;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class SmbReadWriteFileSystem : ReadWriteFileSystemProvider
    {
        private readonly SafeAccessTokenHandle _userToken;
        private readonly UserLogin _user;
        private readonly string _userMount;

        internal SmbReadWriteFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, UserLogin user,
            string identityUser, string identityPass)
            : base(settings)
        {
            _user = user;

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            using (var scope = factory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                var userMount = uow.UserMounts.Get(QueryExpressionFactory.GetQueryExpression<UserMount>()
                    .Where(x => x.IdentityId == _user.IdentityId).ToLambda())
                    .Single();

                _userMount = userMount.ServerAddress + userMount.ServerShare;

                if (userMount.CredentialId.HasValue)
                {
                    var ambassadorCred = uow.Credentials.Get(QueryExpressionFactory.GetQueryExpression<Credential>()
                        .Where(x => x.Id == userMount.CredentialId).ToLambda())
                        .Single();

                    var secret = conf["Databases:AuroraSecret"];
                    string decryptedPass = string.Empty;
                    string encryptedPass = string.Empty;

                    try
                    {
                        decryptedPass = AES.DecryptString(ambassadorCred.EncryptedPass, secret);
                        encryptedPass = AES.EncryptString(decryptedPass, secret);
                    }
                    catch (CryptographicException)
                    {
                        Log.Error($"'{callPath}' '{_user.IdentityAlias}' failure to decrypt the encrypted password used by mount credential. " +
                            $"Verify the system secret key is valid and/or reset the password for the mount credential.");
                        throw;
                    }

                    _userToken = UserHelper.GetSafeAccessTokenHandle(ambassadorCred.Domain, ambassadorCred.UserName, decryptedPass);
                }
                else
                {
                    _userToken = UserHelper.GetSafeAccessTokenHandle(null, identityUser, identityPass);
                }
            }
        }

        [SupportedOSPlatform("windows")]
        protected override DirectoryNode CreateDirectory(DirectoryNode parent, DirectoryNode child)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    var folder = SmbPathFactory.PathToFolder(_userMount + child.Path.StringPath);
                    folder.Create();

                    Log.Information($"'{callPath}' '{_user.IdentityAlias}' folder:'{child.Path}' at:'{folder.FullName}'" +
                        $" as:'{WindowsIdentity.GetCurrent().Name}'");
                });

                return child;
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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
                    var folder = SmbPathFactory.PathToFolder(_userMount + parent.Path.StringPath);

                    if (!folder.Exists)
                        folder.Create();

                    var file = SmbPathFactory.PathToFile(_userMount + child.Path.StringPath);

                    /*
                     * a zero size file will always be created first regardless of actual size of file. 
                     */

                    using (var fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)) { }

                    Log.Information($"'{callPath}' '{_user.IdentityAlias}' empty-file:'{child.Path}' at:'{file.FullName}'" +
                        $" as:'{WindowsIdentity.GetCurrent().Name}'");
                });

                return child;
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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
                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            {
                                var file = SmbPathFactory.PathToFile(_userMount + node.Path.StringPath);
                                file.Delete();

                                Log.Information($"'{callPath}' '{_user.IdentityAlias}' file:'{node.Path}' at:'{file.FullName}'" +
                                    $" as:'{WindowsIdentity.GetCurrent().Name}'");
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folder = SmbPathFactory.PathToFolder(_userMount + node.Path.StringPath);
                                folder.Delete();

                                Log.Information($"'{callPath}' '{_user.IdentityAlias}' folder:'{node.Path}' at:'{folder.FullName}'" +
                                    $" as:'{WindowsIdentity.GetCurrent().Name}'");
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                });

                return node;
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override bool Exists(NodePath path, NodeType nodeType)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                bool exists = false;

                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    switch (nodeType)
                    {
                        case NodeType.File:
                            {
                                var file = SmbPathFactory.PathToFile(_userMount + path.StringPath);

                                if (file.Exists)
                                    exists = true;
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folder = SmbPathFactory.PathToFolder(_userMount + path.StringPath);

                                if (folder.Exists)
                                    exists = true;
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                });

                return exists;
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override NodeAttributes GetAttributes(NodeBase node)
        {
            if (!node.Exists())
                return node.Attributes;

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            {
                                var file = SmbPathFactory.PathToFile(_userMount + node.Path.StringPath);

                                node.SetAttributes(new NodeAttributes(file.Attributes));
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folder = SmbPathFactory.PathToFolder(_userMount + node.Path.StringPath);

                                node.SetAttributes(new NodeAttributes(folder.Attributes));
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                });

                return node.Attributes;
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override NodeBase GetChild(string name, DirectoryNode parent)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                NodeBase child = null;

                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    var folder = SmbPathFactory.PathToFolder(_userMount + parent.Path.StringPath
                        + Path.DirectorySeparatorChar + name);

                    if (folder.Exists)
                        child = new DirectoryNode(name, parent,
                            new NodeTimeInfo(folder.CreationTime, folder.LastAccessTime, folder.LastWriteTime));

                    var file = SmbPathFactory.PathToFile(_userMount + parent.Path.StringPath
                        + Path.DirectorySeparatorChar + name);

                    if (file.Exists)
                        child = new FileNode(name, parent,
                            new NodeTimeInfo(file.CreationTime, file.LastAccessTime, file.LastWriteTime));
                });

                return child;
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override IEnumerable<NodeBase> GetChildren(DirectoryNode parent, NodeType nodeType)
        {
            if (!parent.Exists())
                return Enumerable.Empty<NodeBase>();

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                var children = new List<NodeBase>();

                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    var folderParent = SmbPathFactory.PathToFolder(_userMount + parent.Path.StringPath);

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
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override NodeContent GetContent(NodeBase node, NodeContentParameters parameters)
        {
            if (!node.Exists())
                return NodeContent.CreateDelayedWriteContent(new MemoryStream());

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                NodeContent content = null;

                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    var file = SmbPathFactory.PathToFile(_userMount + node.Path.StringPath);

                    var stream = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

                    Log.Information($"'{callPath}' '{_user.IdentityAlias}' file:'{node.Path}' size:'{stream.Length / 1048576f}MB' at:'{file.FullName}'" +
                        $" as:'{WindowsIdentity.GetCurrent().Name}'");

                    content = parameters.AccessType == NodeContentAccess.Read
                        ? NodeContent.CreateReadOnlyContent(stream)
                        : NodeContent.CreateDelayedWriteContent(stream);
                });

                return content;
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override long GetLength(NodeBase node)
        {
            if (!node.Exists())
                return 0L;

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                long length = 0L;

                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            {
                                var file = SmbPathFactory.PathToFile(_userMount + node.Path.StringPath);

                                length = file.Length;
                            }
                            break;

                        case NodeType.Directory:
                            {
                                length = 0L;
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                });

                return length;
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override NodeTimeInfo GetTimeInfo(NodeBase node)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            {
                                var file = SmbPathFactory.PathToFile(_userMount + node.Path.StringPath);

                                node.SetTimeInfo(new NodeTimeInfo(file.CreationTime, file.LastAccessTime, file.LastWriteTime));
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folder = SmbPathFactory.PathToFolder(_userMount + node.Path.StringPath);

                                node.SetTimeInfo(new NodeTimeInfo(folder.CreationTime, folder.LastAccessTime, folder.LastWriteTime));
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                });

                return node.TimeInfo;
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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

                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    switch (toBeMovedNode.NodeType)
                    {
                        case NodeType.File:
                            {
                                var file = SmbPathFactory.PathToFile(_userMount + toBeMovedNode.Path.StringPath);
                                var newPath = SmbPathFactory.PathToFile(_userMount + targetDirectory.Path.StringPath);

                                file.MoveTo(newPath.FullName);

                                Log.Information($"'{callPath}' '{_user.IdentityAlias}' from-file:'{file.Name}' at:'[{file.FullName}]' to-file:'{newPath.Name}' at:'[{newPath.FullName}]'" +
                                    $" as:'{WindowsIdentity.GetCurrent().Name}'");

                                toBeMovedNode = new FileNode(toBeMovedNode.Name, targetDirectory);
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folder = SmbPathFactory.PathToFolder(_userMount + toBeMovedNode.Path.StringPath);
                                var newPath = SmbPathFactory.PathToFolder(_userMount + targetDirectory.Path.StringPath);

                                folder.MoveTo(newPath.FullName);

                                Log.Information($"'{callPath}' '{_user.IdentityAlias}' from-folder:'{folder.Name}' at:'[{folder.FullName}]' to-folder:'{newPath.Name}' at:'[{newPath.FullName}]'" +
                                    $" as:'{WindowsIdentity.GetCurrent().Name}'");

                                toBeMovedNode = new DirectoryNode(toBeMovedNode.Name, targetDirectory);
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                });

                return toBeMovedNode;
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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

                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            {
                                var file = SmbPathFactory.PathToFile(_userMount + node.Path.StringPath);
                                var newFile = SmbPathFactory.PathToFile(_userMount + node.Parent.Path.StringPath
                                    + Path.DirectorySeparatorChar + newName);

                                file.MoveTo(newFile.FullName);

                                Log.Information($"'{callPath}' '{_user.IdentityAlias}' from-file:'{file.Name}' at:'[{file.FullName}]' to-file:'{newFile.Name}' at:'[{newFile.FullName}]'" +
                                    $" as:'{WindowsIdentity.GetCurrent().Name}'");

                                toBeNamedNode = new FileNode(newName, node.Parent);
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folder = SmbPathFactory.PathToFolder(_userMount + node.Path.StringPath);
                                var newFolder = SmbPathFactory.PathToFolder(_userMount + node.Parent.Path.StringPath
                                    + Path.DirectorySeparatorChar + newName);

                                folder.MoveTo(newFolder.FullName);

                                Log.Information($"'{callPath}' '{_user.IdentityAlias}' from-folder:'{folder.Name}' at:'[{folder.FullName}]' to-folder:'{newFolder.Name}' at:'[{newFolder.FullName}]'" +
                                    $" as:'{WindowsIdentity.GetCurrent().Name}'");

                                toBeNamedNode = new DirectoryNode(newName, node.Parent);
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                });

                return toBeNamedNode;
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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
                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    switch (node.NodeType)
                    {
                        case NodeType.File:
                            {
                                var folder = SmbPathFactory.PathToFolder(_userMount + node.Parent.Path.StringPath);

                                if (!folder.Exists)
                                    folder.Create();

                                var file = SmbPathFactory.PathToFile(_userMount + node.Path.StringPath);

                                using (var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                                    content.GetStream().CopyTo(fs);

                                Log.Information($"'{callPath}' '{_user.IdentityAlias}' file:'{node.Path}' size:'{content.Length / 1048576f}MB' at:'{file.FullName}'" +
                                    $" as:'{WindowsIdentity.GetCurrent().Name}'");
                            }
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                });

                return node;
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override NodeBase SetAttributes(NodeBase node, NodeAttributes attributes)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                WindowsIdentity.RunImpersonated(_userToken, () =>
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
                });

                return node;
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override NodeBase SetTimeInfo(NodeBase node, NodeTimeInfo timeInfo)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                WindowsIdentity.RunImpersonated(_userToken, () =>
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
                });

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
