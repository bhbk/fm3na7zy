﻿using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
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
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class SmbReadOnlyFileSystem : ReadOnlyFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private readonly SafeAccessTokenHandle _userToken;
        private readonly tbl_Users _user;
        private readonly string _userMountPath;

        internal SmbReadOnlyFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, tbl_Users user)
            : base(settings)
        {
            /*
             * this file-system is functional only when the daemon is running a on windows platform. there is 
             * an interop call required to obtain a user credential outside the context of what the daemon runs as.
             */
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotImplementedException();

            _factory = factory;
            _user = user;

            using (var scope = factory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var userMount = uow.UserMounts.Get(x => x.UserId == _user.Id).SingleOrDefault();

                if (userMount == null)
                    throw new NetworkInformationException();

                _userMountPath = userMount.ServerName + userMount.ServerPath;

                var userCred = uow.SysCredentials.Get(x => x.Id == userMount.CredentialId).Single();

                var secret = conf["Databases:AuroraSecretKey"];
                var plainText = AES.DecryptString(userCred.Password, secret);
                var cipherText = AES.EncryptString(plainText, secret);

                if (userCred.Password != cipherText)
                    throw new ArithmeticException();

                _userToken = UserHelper.GetSafeAccessTokenHandle(userCred.Domain, userCred.UserName, plainText);
            }
        }

        protected override bool Exists(NodePath path, NodeType nodeType)
        {
            try
            {
                bool exists = false;

                if (nodeType == NodeType.Directory)
                {
                    WindowsIdentity.RunImpersonated(_userToken, () =>
                    {
                        var folder = SmbFileSystemCommon.ConvertPathToCifsFolder(_userMountPath + path.StringPath);

                        if (folder.Exists)
                            exists = true;
                    });
                }
                else if (nodeType == NodeType.File)
                {
                    WindowsIdentity.RunImpersonated(_userToken, () =>
                    {
                        var file = SmbFileSystemCommon.ConvertPathToCifsFile(_userMountPath + path.StringPath);

                        if (file.Exists)
                            exists = true;
                    });
                }
                else
                    throw new NotImplementedException();

                return exists;
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

                NodeAttributes attributes = null;

                if (node.NodeType == NodeType.Directory)
                {
                    WindowsIdentity.RunImpersonated(_userToken, () =>
                    {
                        var folder = SmbFileSystemCommon.ConvertPathToCifsFolder(_userMountPath + node.Path.StringPath);

                        attributes = new NodeAttributes(FileAttributes.Directory);
                    });
                }
                else if (node.NodeType == NodeType.File)
                {
                    WindowsIdentity.RunImpersonated(_userToken, () =>
                    {
                        var file = SmbFileSystemCommon.ConvertPathToCifsFile(_userMountPath + node.Path.StringPath);

                        attributes = new NodeAttributes(FileAttributes.Normal);
                    });
                }
                else
                    throw new NotImplementedException();

                return attributes;
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

                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    var folder = SmbFileSystemCommon.ConvertPathToCifsFolder(_userMountPath + parent.Path.StringPath
                        + Path.DirectorySeparatorChar + name);

                    if (folder.Exists)
                        child = new DirectoryNode(name, parent,
                            new NodeTimeInfo(folder.CreationTime, folder.LastAccessTime, folder.LastWriteTime));

                    var file = SmbFileSystemCommon.ConvertPathToCifsFile(_userMountPath + parent.Path.StringPath
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

        protected override IEnumerable<NodeBase> GetChildren(DirectoryNode parent, NodeType nodeType)
        {
            try
            {
                if (!parent.Exists())
                    return Enumerable.Empty<NodeBase>();

                var children = new List<NodeBase>();

                WindowsIdentity.RunImpersonated(_userToken, () =>
                {
                    var parentFolder = SmbFileSystemCommon.ConvertPathToCifsFolder(_userMountPath + parent.Path.StringPath);

                    foreach (var folderPath in Directory.GetDirectories(parentFolder.FullName))
                    {
                        var folder = new DirectoryInfo(folderPath);

                        children.Add(new DirectoryNode(folder.Name, parent,
                            new NodeTimeInfo(folder.CreationTime, folder.LastAccessTime, folder.LastWriteTime)));
                    }

                    foreach (var filePath in Directory.GetFiles(parentFolder.FullName))
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

        protected override NodeContent GetContent(NodeBase node, NodeContentParameters contentParameters)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                if (!node.Exists())
                    return NodeContent.CreateDelayedWriteContent(new MemoryStream());

                NodeContent content = null;

                if (node.NodeType == NodeType.File)
                {
                    WindowsIdentity.RunImpersonated(_userToken, () =>
                    {
                        var file = SmbFileSystemCommon.ConvertPathToCifsFile(_userMountPath + node.Path.StringPath);

                        content = NodeContent.CreateDelayedWriteContent(File.Open(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                    });
                }
                else
                    throw new NotImplementedException();

                return content;
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
                long length = 0L;

                if (node.NodeType == NodeType.File)
                {
                    WindowsIdentity.RunImpersonated(_userToken, () =>
                    {
                        var file = SmbFileSystemCommon.ConvertPathToCifsFile(_userMountPath + node.Path.StringPath);

                        length = file.Length;
                    });
                }
                else
                    throw new NotImplementedException();

                return length;
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
                NodeTimeInfo timeInfo = null;

                if (node.NodeType == NodeType.Directory)
                {
                    WindowsIdentity.RunImpersonated(_userToken, () =>
                    {
                        var folder = SmbFileSystemCommon.ConvertPathToCifsFolder(_userMountPath + node.Path.StringPath);

                        timeInfo = new NodeTimeInfo(folder.CreationTime, folder.LastAccessTime, folder.LastWriteTime);
                    });
                }
                else if (node.NodeType == NodeType.File)
                {
                    WindowsIdentity.RunImpersonated(_userToken, () =>
                    {
                        var file = SmbFileSystemCommon.ConvertPathToCifsFile(_userMountPath + node.Path.StringPath);

                        timeInfo = new NodeTimeInfo(file.CreationTime, file.LastAccessTime, file.LastWriteTime);
                    });
                }
                else
                    throw new NotImplementedException();

                return timeInfo;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }
    }
}
