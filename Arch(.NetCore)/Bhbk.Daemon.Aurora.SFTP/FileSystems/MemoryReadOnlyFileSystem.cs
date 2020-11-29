using Bhbk.Daemon.Aurora.SFTP.Helpers;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Microsoft.Extensions.DependencyInjection;
using Rebex.IO.FileSystem;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System;

/*
 * https://forum.rebex.net/8453/implement-filesystem-almost-as-memoryfilesystemprovider
 */
namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class MemoryReadOnlyFileSystem : ReadOnlyFileSystemProvider
    {
        private readonly Dictionary<NodePath, NodeBase> _path;
        private readonly Dictionary<NodeBase, MemoryNodeData> _store;
        private readonly IServiceScopeFactory _factory;
        private readonly User _userEntity;

        internal MemoryReadOnlyFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, User userEntity)
            : base(settings)
        {
            _factory = factory;
            _userEntity = userEntity;

            _path = new Dictionary<NodePath, NodeBase>();
            _store = new Dictionary<NodeBase, MemoryNodeData>();

            MemoryFileSystemHelper.EnsureRootExists(Root, _path, _store, _userEntity);
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
    }
}
