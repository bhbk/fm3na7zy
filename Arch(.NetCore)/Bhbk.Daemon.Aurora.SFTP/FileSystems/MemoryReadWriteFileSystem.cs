using Bhbk.Daemon.Aurora.SFTP.Helpers;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Rebex.IO.FileSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

/*
 * https://forum.rebex.net/8453/implement-filesystem-almost-as-memoryfilesystemprovider
 */
namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class MemoryReadWriteFileSystem : ReadWriteFileSystemProvider
    {
        private readonly Dictionary<NodePath, NodeBase> _path;
        private readonly Dictionary<NodeBase, MemoryNodeData> _store;
        private readonly IServiceScopeFactory _factory;
        private readonly User _userEntity;

        internal MemoryReadWriteFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, User userEntity)
            : base(settings)
        {
            _path = new Dictionary<NodePath, NodeBase>();
            _store = new Dictionary<NodeBase, MemoryNodeData>();

            _factory = factory;
            _userEntity = userEntity;

            MemoryFileSystemHelper.EnsureRootExists(Root, _path, _store, _userEntity);

            var folderKeysNode = new DirectoryNode(".ssh", Root);
            var fileKeysNode = new FileNode("authorized_users", folderKeysNode);

            if (!Exists(folderKeysNode.Path, NodeType.Directory))
                CreateDirectory(Root, folderKeysNode);

            if (Exists(fileKeysNode.Path, NodeType.File))
                Delete(fileKeysNode);

            var pubKeysContent = KeyHelper.ExportPubKeyBase64(_userEntity, _userEntity.PublicKeys);

            CreateFile(folderKeysNode, fileKeysNode);
            SaveContent(fileKeysNode, NodeContent.CreateDelayedWriteContent(
                new MemoryStream(Encoding.UTF8.GetBytes(pubKeysContent.ToString()))));
        }

        protected override DirectoryNode CreateDirectory(DirectoryNode parent, DirectoryNode child)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                _store.Add(child, new MemoryNodeData());
                _store[parent].Children.Add(child);
                _path.Add(child.Path, child);

                Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' folder '{child.Path}' in memory");

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
                if (_userEntity.QuotaUsedInBytes >= _userEntity.QuotaInBytes)
                {
                    Log.Warning($"'{callPath}' '{_userEntity.IdentityAlias}' file '{child.Path}' cancelled, " +
                        $"totoal quota '{_userEntity.QuotaInBytes}' used quota '{_userEntity.QuotaUsedInBytes}'");

                    throw new FileSystemOperationCanceledException();
                }

                /*
                 * a zero size file will always be created first regardless of actual size of file. 
                 */

                _store.Add(child, new MemoryNodeData());
                _store[parent].Children.Add(child);
                _path.Add(child.Path, child);

                Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' empty file '{child.Path}' in memory");

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
                switch (node.NodeType)
                {
                    case NodeType.File:
                        {
                            _userEntity.QuotaUsedInBytes -= _store[node].Content.Length;

                            _store[node.Parent].Children.Remove(node);
                            _store.Remove(node);
                            _path.Remove(node.Path);

                            Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' file '{node.Path}' in memory");
                        }
                        break;

                    case NodeType.Directory:
                        {
                            _store[node.Parent].Children.Remove(node);
                            _store.Remove(node);
                            _path.Remove(node.Path);

                            Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' folder '{node.Path}' in memory");
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
                var node = _path.GetValueOrDefault(path);

                return node == null ? false : true;
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

                return _store[node].Attributes;
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
                return _store[parent].Children.FirstOrDefault(x => x.Name == name);
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

                return _store[parent].Children;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeContent GetContent(NodeBase node, NodeContentParameters contentParameters)
        {
            if (!node.Exists())
                return NodeContent.CreateDelayedWriteContent(new MemoryStream());

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                var stream = new MemoryStream();
                _store[node].Content.CopyTo(stream);

                stream.Position = 0;
                _store[node].Content.Position = 0;

                Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' file '{node.Path}' from memory");

                return contentParameters.AccessType == NodeContentAccess.Read
                    ? NodeContent.CreateReadOnlyContent(stream)
                    : NodeContent.CreateDelayedWriteContent(stream);
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

                return _store[node].Length;
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
                return _store[node].TimeInfo;
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

            NodeBase newNode;
            MemoryNodeData newNodeData;

            try
            {
                switch (toBeMovedNode.NodeType)
                {
                    case NodeType.File:
                        {
                            newNode = new FileNode(toBeMovedNode.Name, targetDirectory);
                        }
                        break;

                    case NodeType.Directory:
                        {
                            newNode = new DirectoryNode(toBeMovedNode.Name, targetDirectory);
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }

                newNodeData = _store[toBeMovedNode];

                _store.Add(newNode, newNodeData);
                _store[targetDirectory].Children.Add(newNode);
                _path.Add(newNode.Path, newNode);

                _store[toBeMovedNode.Parent].Children.Remove(toBeMovedNode);
                _store.Remove(toBeMovedNode);
                _path.Remove(toBeMovedNode.Path);

                Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' from '{toBeMovedNode.Path}' to '{newNode.Path}' in memory");

                return toBeMovedNode;
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

            NodeBase newNode;
            MemoryNodeData newNodeData;

            try
            {
                switch (node.NodeType)
                {
                    case NodeType.File:
                        {
                            newNode = new FileNode(newName, node.Parent);
                            newNodeData = _store[node];

                            _store.Add(newNode, newNodeData);
                            _store[node.Parent].Children.Add(newNode);
                            _path.Add(newNode.Path, newNode);

                            _store[node.Parent].Children.Remove(node);
                            _store.Remove(node);
                            _path.Remove(node.Path);
                        }
                        break;

                    case NodeType.Directory:
                        {
                            newNode = new DirectoryNode(newName, node.Parent);
                            newNodeData = _store[node];

                            _store.Add(newNode, newNodeData);
                            _store[node.Parent].Children.Add(newNode);
                            _path.Add(newNode.Path, newNode);

                            _store[node.Parent].Children.Remove(node);
                            _store.Remove(node);
                            _path.Remove(node.Path);
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
                
                Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' from '{node.Path}' to '{newNode.Path}' in memory");

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
            if (!node.Exists())
                return node;

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                var stream = new MemoryStream();
                content.GetStream().CopyTo(stream);
                stream.Position = 0;

                _store[node].Content = stream;
                _store[node].Content.Position = 0;

                _userEntity.QuotaUsedInBytes += stream.Length;

                Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' file '{node.Path}' to memory");

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