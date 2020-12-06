﻿using Bhbk.Daemon.Aurora.SFTP.Factories;
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
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Principal;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class SmbReadOnlyFileSystem : ReadOnlyFileSystemProvider
    {
        private readonly SafeAccessTokenHandle _userToken;
        private readonly User _user;
        private readonly string _userMount;

        internal SmbReadOnlyFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, User user, 
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
                        decryptedPass = AES.DecryptString(ambassadorCred.EncryptedPassword, secret);
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
        protected override bool Exists(NodePath path, NodeType nodeType)
        {
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
                Log.Error(ex.ToString());
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
                Log.Error(ex.ToString());
                throw;
            }
        }

        [SupportedOSPlatform("windows")]
        protected override NodeTimeInfo GetTimeInfo(NodeBase node)
        {
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
                Log.Error(ex.ToString());
                throw;
            }
        }
    }
}
