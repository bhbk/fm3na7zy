using Bhbk.Daemon.Aurora.SFTP.Helpers;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Microsoft.Extensions.DependencyInjection;
using Rebex.IO.FileSystem;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

/*
 * https://forum.rebex.net/8453/implement-filesystem-almost-as-memoryfilesystemprovider
 */
namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class MemoryReadOnlyFileSystem : ReadOnlyFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private readonly Dictionary<NodePath, NodeBase> _path;
        private readonly Dictionary<NodeBase, MemoryNodeData> _store;
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
            NodeBase node;

            _path.TryGetValue(path, out node);

            return node != null && node.NodeType == nodeType;
        }

        protected override NodeBase GetChild(string name, DirectoryNode parent)
        {
            return _store[parent].Children.FirstOrDefault(child => child.Name == name);
        }

        protected override IEnumerable<NodeBase> GetChildren(DirectoryNode parent, NodeType nodeType)
        {
            if (!parent.Exists())
                return Enumerable.Empty<NodeBase>();

            var children = _store[parent].Children;

            return children;
        }

        protected override NodeContent GetContent(NodeBase node, NodeContentParameters contentParameters)
        {
            if (!node.Exists())
                return NodeContent.CreateDelayedWriteContent(new MemoryStream());

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var content = new MemoryStream();
            _store[node].Content.CopyTo(content);

            content.Position = 0;
            _store[node].Content.Position = 0;

            Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' file '{node.Path}' from memory");

            return contentParameters.AccessType == NodeContentAccess.Read
                ? NodeContent.CreateReadOnlyContent(content)
                : NodeContent.CreateDelayedWriteContent(content);
        }

        protected override long GetLength(NodeBase node)
        {
            if (!node.Exists())
                return 0L;

            return _store[node].Length;
        }

        protected override NodeTimeInfo GetTimeInfo(NodeBase node)
        {
            return _store[node].TimeInfo;
        }
    }
}
