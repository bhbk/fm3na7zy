using Bhbk.Daemon.Aurora.SSH.Helpers;
using Bhbk.Lib.Aurora.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Cryptography.Encryption;
using Microsoft.EntityFrameworkCore;
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
using System.Security.Principal;

namespace Bhbk.Daemon.Aurora.SSH.FileSystems
{
    public class SmbReadWriteFileSystem : ReadWriteFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private readonly string _basePath;
        private SafeAccessTokenHandle _access;
        private tbl_Users _user;

        public SmbReadWriteFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, tbl_Users user)
            : base(settings)
        {
            _factory = factory;
            _user = user;

            using (var scope = factory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var userMount = uow.UserMounts.Get(x => x.UserId == _user.Id).SingleOrDefault();

                if (userMount == null)
                    throw new NetworkInformationException();

                _basePath = userMount.ServerName + userMount.ServerPath;

                var userCred = uow.SysCredentials.Get(x => x.Id == userMount.CredentialId).Single();

                var secret = conf["Databases:AuroraSecretKey"];
                var plainText = AES.Decrypt(userCred.Password, secret);
                var cipherText = AES.Encrypt(plainText, secret);

                if (userCred.Password != cipherText)
                    throw new ArithmeticException();

                _access = UserHelper.GetSafeAccessTokenHandle(userCred.Domain, userCred.UserName, plainText);
            }
        }

        protected override DirectoryNode CreateDirectory(DirectoryNode parent, DirectoryNode child)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                WindowsIdentity.RunImpersonated(_access, () =>
                {
                    var folder = SmbFileSystemHelper.ConvertPathToCifsFolder(_basePath + child.Path.StringPath);
                    folder.Create();

                    Log.Information($"'{callPath}' '{_user.UserName}' empty file '{child.Path}' to '{folder.FullName}' run as '{WindowsIdentity.GetCurrent().Name}'");
                });

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
                WindowsIdentity.RunImpersonated(_access, () =>
                {
                    var folder = SmbFileSystemHelper.ConvertPathToCifsFolder(_basePath + parent.Path.StringPath);

                    if (!folder.Exists)
                        folder.Create();

                    var file = SmbFileSystemHelper.ConvertPathToCifsFile(_basePath + child.Path.StringPath);

                    using (var fs = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)) { }

                    Log.Information($"'{callPath}' '{_user.UserName}' empty file '{child.Path}' to '{file.FullName}' run as '{WindowsIdentity.GetCurrent().Name}'");
                });

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
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                if (!node.Exists())
                    return node;

                if (node.NodeType == NodeType.Directory)
                {
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var folder = SmbFileSystemHelper.ConvertPathToCifsFolder(_basePath + node.Path.StringPath);
                        folder.Delete();

                        Log.Information($"'{callPath}' '{_user.UserName}' folder '{node.Path}' from '{folder.FullName}' run as '{WindowsIdentity.GetCurrent().Name}'");
                    });
                }
                else if (node.NodeType == NodeType.File)
                {
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var file = SmbFileSystemHelper.ConvertPathToCifsFile(_basePath + node.Path.StringPath);
                        file.Delete();

                        Log.Information($"'{callPath}' '{_user.UserName}' file '{node.Path}' from '{file.FullName}' run as '{WindowsIdentity.GetCurrent().Name}'");
                    });
                }
                else
                    throw new NotImplementedException();

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
                bool exists = false;

                if (nodeType == NodeType.Directory)
                {
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var folder = SmbFileSystemHelper.ConvertPathToCifsFolder(_basePath + path.StringPath);

                        if (folder.Exists)
                            exists = true;
                    });
                }
                else if (nodeType == NodeType.File)
                {
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var file = SmbFileSystemHelper.ConvertPathToCifsFile(_basePath + path.StringPath);

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
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var folder = SmbFileSystemHelper.ConvertPathToCifsFolder(_basePath + node.Path.StringPath);

                        attributes = new NodeAttributes(FileAttributes.Directory);
                    });
                }
                else if (node.NodeType == NodeType.File)
                {
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var file = SmbFileSystemHelper.ConvertPathToCifsFile(_basePath + node.Path.StringPath);

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

                WindowsIdentity.RunImpersonated(_access, () =>
                {
                    var folder = SmbFileSystemHelper.ConvertPathToCifsFolder(_basePath + parent.Path.StringPath
                        + Path.DirectorySeparatorChar + name);

                    if (folder.Exists)
                        child = new DirectoryNode(name, parent,
                            new NodeTimeInfo(folder.CreationTime, folder.LastAccessTime, folder.LastWriteTime));

                    var file = SmbFileSystemHelper.ConvertPathToCifsFile(_basePath + parent.Path.StringPath
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

                WindowsIdentity.RunImpersonated(_access, () =>
                {
                    var parentFolder = SmbFileSystemHelper.ConvertPathToCifsFolder(_basePath + parent.Path.StringPath);

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
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var file = SmbFileSystemHelper.ConvertPathToCifsFile(_basePath + node.Path.StringPath);

                        Log.Information($"'{callPath}' '{_user.UserName}' file '{node.Path}' from '{file.FullName}' run as '{WindowsIdentity.GetCurrent().Name}'");

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
                if (!node.Exists()
                    || node.NodeType == NodeType.Directory)
                    return 0L;

                long length = 0L;

                if (node.NodeType == NodeType.File)
                {
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var file = SmbFileSystemHelper.ConvertPathToCifsFile(_basePath + node.Path.StringPath);

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
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var folder = SmbFileSystemHelper.ConvertPathToCifsFolder(_basePath + node.Path.StringPath);

                        timeInfo = new NodeTimeInfo(folder.CreationTime, folder.LastAccessTime, folder.LastWriteTime);
                    });
                }
                else if (node.NodeType == NodeType.File)
                {
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var file = SmbFileSystemHelper.ConvertPathToCifsFile(_basePath + node.Path.StringPath);

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

        protected override NodeBase Move(NodeBase toBeMovedNode, DirectoryNode targetDirectory)
        {
            throw new NotImplementedException();

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                WindowsIdentity.RunImpersonated(_access, () =>
                {

                });
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
                NodeBase newNode = null;
                var parentFolder = SmbFileSystemHelper.ConvertPathToCifsFolder(_basePath + node.Parent.Path.StringPath);

                if (node.NodeType == NodeType.Directory)
                {
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var folder = SmbFileSystemHelper.ConvertPathToCifsFolder(_basePath + node.Path.StringPath);
                        var newFolder = SmbFileSystemHelper.ConvertPathToCifsFolder(_basePath + node.Parent.Path.StringPath
                            + Path.DirectorySeparatorChar + newName);

                        Log.Information($"'{callPath}' '{_user.UserName}' from '{folder.Name}' [{folder.FullName}] " +
                            $"to '{newFolder.Name}' [{newFolder.FullName}] run as '{WindowsIdentity.GetCurrent().Name}'");

                        folder.MoveTo(newFolder.FullName);
                    });

                    newNode = new DirectoryNode(newName, node.Parent);
                }
                else if (node.NodeType == NodeType.File)
                {
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var file = SmbFileSystemHelper.ConvertPathToCifsFile(_basePath + node.Path.StringPath);
                        var newFile = SmbFileSystemHelper.ConvertPathToCifsFile(_basePath + node.Parent.Path.StringPath
                            + Path.DirectorySeparatorChar + newName);

                        Log.Information($"'{callPath}' '{_user.UserName}' from '{file.Name}' [{file.FullName}] " +
                            $"to '{newFile.Name}' [{newFile.FullName}] run as '{WindowsIdentity.GetCurrent().Name}'");

                        file.MoveTo(newFile.FullName);
                    });

                    newNode = new FileNode(newName, node.Parent);
                }
                else
                    throw new NotImplementedException();

                return newNode;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeBase SaveContent(NodeBase node, NodeContent content)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            DirectoryInfo folder = null;
            FileInfo file = null;

            try
            {
                if (!node.Exists())
                    return node;

                if (node.NodeType == NodeType.File)
                {
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        folder = SmbFileSystemHelper.ConvertPathToCifsFolder(_basePath + node.Parent.Path.StringPath);

                        if (!folder.Exists)
                            folder.Create();

                        file = SmbFileSystemHelper.ConvertPathToCifsFile(_basePath + node.Path.StringPath);

                        using (var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                            content.GetStream().CopyTo(fs);

                        Log.Information($"'{callPath}' '{_user.UserName}' file '{node.Path}' to '{file.FullName}' run as '{WindowsIdentity.GetCurrent().Name}'");
                    });
                }
                else
                    throw new NotImplementedException();

                return node;
            }
            catch (IOException ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
            catch (DbUpdateException ex)
            {
                if (file.Exists)
                    file.Delete();

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
                if (node.NodeType == NodeType.Directory)
                {
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var folder = SmbFileSystemHelper.ConvertPathToCifsFolder(_basePath + node.Path.StringPath);

                        folder.Attributes = attributes.FileAttributes;
                    });
                }
                else if (node.NodeType == NodeType.File)
                {
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var file = SmbFileSystemHelper.ConvertPathToCifsFile(_basePath + node.Path.StringPath);

                        file.Attributes = attributes.FileAttributes;
                    });
                }
                else
                    throw new NotImplementedException();

                return node;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeBase SetTimeInfo(NodeBase node, NodeTimeInfo newTimeInfo)
        {
            try
            {
                if (node.NodeType == NodeType.Directory)
                {
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var folder = SmbFileSystemHelper.ConvertPathToCifsFolder(_basePath + node.Path.StringPath);

                        folder.CreationTime = newTimeInfo.CreationTime;
                        folder.LastAccessTime = newTimeInfo.LastAccessTime;
                        folder.LastWriteTime = newTimeInfo.LastWriteTime;
                    });
                }
                else if (node.NodeType == NodeType.File)
                {
                    WindowsIdentity.RunImpersonated(_access, () =>
                    {
                        var file = SmbFileSystemHelper.ConvertPathToCifsFile(_basePath + node.Path.StringPath);

                        file.CreationTime = newTimeInfo.CreationTime;
                        file.LastAccessTime = newTimeInfo.LastAccessTime;
                        file.LastWriteTime = newTimeInfo.LastWriteTime;
                    });
                }
                else
                    throw new NotImplementedException();

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
