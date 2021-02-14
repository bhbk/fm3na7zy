using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Cryptography.Encryption;
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

namespace Bhbk.Daemon.Aurora.SFTP.Providers
{
    internal class SmbReadOnlyFileProvider : ReadOnlyFileSystemProvider
    {
        private readonly SafeAccessTokenHandle _userToken;
        private FileSystemLogin_EF _fileSystemLogin;

        internal SmbReadOnlyFileProvider(FileSystemProviderSettings settings, IServiceScopeFactory factory, FileSystemLogin_EF fileSystemLogin, 
            string identityUser, string identityPass)
            : base(settings)
        {
            _fileSystemLogin = fileSystemLogin;

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            using (var scope = factory.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                if (fileSystemLogin.AmbassadorId.HasValue)
                {
                    string decryptedPass;

                    try
                    {
                        var secret = conf["Databases:AuroraSecret"];
                        decryptedPass = AES.DecryptString(fileSystemLogin.Ambassador.EncryptedPass, secret);
                    }
                    catch (CryptographicException)
                    {
                        Log.Error($"'{callPath}' '{_fileSystemLogin.Login.UserName}' failure to decrypt the encrypted password used by mount credential. " +
                            $"Verify the system secret key is valid and/or reset the password for the mount credential.");
                        throw;
                    }

                    _userToken = UserHelper.GetSafeAccessTokenHandle(null, fileSystemLogin.Ambassador.UserPrincipalName, decryptedPass);
                }
                else
                {
                    _userToken = UserHelper.GetSafeAccessTokenHandle(null, identityUser, identityPass);
                }
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
                                var file = FilePathHelper.PathToFile(_fileSystemLogin.FileSystem.UncPath + path.StringPath);

                                if (file.Exists)
                                    exists = true;
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folder = FilePathHelper.PathToFolder(_fileSystemLogin.FileSystem.UncPath + path.StringPath);

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
                                var file = FilePathHelper.PathToFile(_fileSystemLogin.FileSystem.UncPath + node.Path.StringPath);

                                node.SetAttributes(new NodeAttributes(file.Attributes));
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folder = FilePathHelper.PathToFolder(_fileSystemLogin.FileSystem.UncPath + node.Path.StringPath);

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
                    var folder = FilePathHelper.PathToFolder(_fileSystemLogin.FileSystem.UncPath + parent.Path.StringPath
                        + Path.DirectorySeparatorChar + name);

                    if (folder.Exists)
                        child = new DirectoryNode(name, parent,
                            new NodeTimeInfo(folder.CreationTime, folder.LastAccessTime, folder.LastWriteTime));

                    var file = FilePathHelper.PathToFile(_fileSystemLogin.FileSystem.UncPath + parent.Path.StringPath
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
                    var folderParent = FilePathHelper.PathToFolder(_fileSystemLogin.FileSystem.UncPath + parent.Path.StringPath);

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
                    var file = FilePathHelper.PathToFile(_fileSystemLogin.FileSystem.UncPath + node.Path.StringPath);

                    var stream = File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

                    Log.Information($"'{callPath}' '{_fileSystemLogin.Login.UserName}' file:'{node.Path}' size:'{stream.Length / 1048576f}MB' at:'{file.FullName}'" +
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
                                var file = FilePathHelper.PathToFile(_fileSystemLogin.FileSystem.UncPath + node.Path.StringPath);

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
                                var file = FilePathHelper.PathToFile(_fileSystemLogin.FileSystem.UncPath + node.Path.StringPath);

                                node.SetTimeInfo(new NodeTimeInfo(file.CreationTime, file.LastAccessTime, file.LastWriteTime));
                            }
                            break;

                        case NodeType.Directory:
                            {
                                var folder = FilePathHelper.PathToFolder(_fileSystemLogin.FileSystem.UncPath + node.Path.StringPath);

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
    }
}
