using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rebex.IO.FileSystem;

/*
 * https://forum.rebex.net/8453/implement-filesystem-almost-as-memoryfilesystemprovider
 */
namespace Bhbk.Daemon.Aurora.SSH.FileSystems
{
    public class MemoryReadWriteFileSystem : ReadWriteFileSystemProvider
    {
        private readonly Dictionary<NodePath, NodeBase> _path;
        private readonly Dictionary<NodeBase, MemoryNodeData> _store;

        public MemoryReadWriteFileSystem() : this(null) { }

        public MemoryReadWriteFileSystem(FileSystemProviderSettings settings) : base(settings)
        {
            _path = new Dictionary<NodePath, NodeBase>();
            _store = new Dictionary<NodeBase, MemoryNodeData>();
        }

        public void AddRoot()
        {
            if (_store.Count == 0)
            {
                _store.Add(Root, new MemoryNodeData());
                _path.Add(Root.Path, Root);
            }
        }

        private Dictionary<NodeBase, MemoryNodeData> Store
        {
            get
            {
                return _store;
            }
        }

        private Dictionary<NodePath, NodeBase> Paths
        {
            get
            {
                return _path;
            }
        }

        protected override DirectoryNode CreateDirectory(DirectoryNode parent, DirectoryNode child)
        {
            Store.Add(child, new MemoryNodeData());
            Store[parent].Children.Add(child);
            Paths.Add(child.Path, child);

            return child;
        }

        protected override FileNode CreateFile(DirectoryNode parent, FileNode child)
        {
            Store.Add(child, new MemoryNodeData());
            Store[parent].Children.Add(child);
            Paths.Add(child.Path, child);

            return child;
        }

        protected override NodeBase Delete(NodeBase node)
        {
            if (!node.Exists())
                return node;

            Store.Remove(node);
            Paths.Remove(node.Path);
            Store[node.Parent].Children.Remove(node);

            return node;
        }

        protected override bool Exists(NodePath path, NodeType nodeType)
        {
            NodeBase node;
            Paths.TryGetValue(path, out node);

            return node != null && node.NodeType == nodeType;
        }

        protected override NodeAttributes GetAttributes(NodeBase node)
        {
            if (!node.Exists())
                return node.Attributes;

            return Store[node].Attributes;
        }

        protected override NodeBase GetChild(string name, DirectoryNode parent)
        {
            return Store[parent].Children.FirstOrDefault(child => child.Name == name);
        }

        protected override IEnumerable<NodeBase> GetChildren(DirectoryNode parent, NodeType nodeType)
        {
            if (!parent.Exists())
                return Enumerable.Empty<NodeBase>();

            var children = Store[parent].Children;

            return children;
        }

        protected override NodeContent GetContent(NodeBase node, NodeContentParameters contentParameters)
        {
            //error
            if (!node.Exists())
                return NodeContent.CreateDelayedWriteContent(new MemoryStream());

            var resultStream = new MemoryStream();
            Store[node].Content.CopyTo(resultStream);
            resultStream.Position = 0;
            Store[node].Content.Position = 0;

            return contentParameters.AccessType == NodeContentAccess.Read
                ? NodeContent.CreateReadOnlyContent(resultStream)
                : NodeContent.CreateDelayedWriteContent(resultStream);
        }

        protected override long GetLength(NodeBase node)
        {
            if (!node.Exists())
                return 0L;

            return Store[node].Length;
        }

        protected override NodeTimeInfo GetTimeInfo(NodeBase node)
        {
            return Store[node].TimeInfo;
        }

        protected override NodeBase Move(NodeBase toBeMovedNode, DirectoryNode targetDirectory)
        {
            throw new NotImplementedException();
        }

        protected override NodeBase Rename(NodeBase node, string newName)
        {
            var isFile = node.NodeType == NodeType.File;
            var newNode = isFile
                ? (NodeBase)new FileNode(newName,
                    node.Parent)
                : new DirectoryNode(newName, node.Parent);

            Delete(node);

            return newNode;
        }

        protected override NodeBase SaveContent(NodeBase node, NodeContent content)
        {
            if (!node.Exists())
                return node;

            var newStream = new MemoryStream();
            content.GetStream().CopyTo(newStream);
            newStream.Position = 0;

            Store[node].Content = newStream;

            return node;
        }

        protected override NodeBase SetAttributes(NodeBase node, NodeAttributes attributes)
        {
            Store[node].Attributes = attributes;

            return node;
        }

        protected override NodeBase SetTimeInfo(NodeBase node, NodeTimeInfo newTimeInfo)
        {
            Store[node].TimeInfo = newTimeInfo;

            return node;
        }
    }

    internal class MemoryNodeData
    {
        public MemoryNodeData()
        {
            Content = new MemoryStream();
            TimeInfo = new NodeTimeInfo();
            Children = new List<NodeBase>();
            Attributes = new NodeAttributes(FileAttributes.Offline);
        }

        public NodeAttributes Attributes
        {
            get;
            set;
        }

        public NodeTimeInfo TimeInfo
        {
            get;
            set;
        }

        public List<NodeBase> Children
        {
            get;
            set;
        }

        public long Length
        {
            get
            {
                return Content.Length;
            }
        }

        public MemoryStream Content 
        { 
            get; 
            set; 
        }
    }
}