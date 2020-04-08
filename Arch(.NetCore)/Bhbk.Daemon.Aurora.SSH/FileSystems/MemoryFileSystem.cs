﻿using Rebex.IO.FileSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bhbk.Daemon.Aurora.SSH.FileSystems
{
    public class MemoryFileSystem : ReadWriteFileSystemProvider
    {
        private readonly Dictionary<NodeBase, MemoryNodeData> _store;
        private readonly Dictionary<NodePath, NodeBase> _path;

        public MemoryFileSystem()
            : this(null) { }

        public MemoryFileSystem(FileSystemProviderSettings settings)
            : base(settings)
        {
            _store = new Dictionary<NodeBase, MemoryNodeData>();
            _store.Add(Root, new MemoryNodeData());

            _path = new Dictionary<NodePath, NodeBase>();
            _path.Add(Root.Path, Root);
        }

        protected override DirectoryNode CreateDirectory(DirectoryNode parent, DirectoryNode child)
        {
            _store.Add(child, new MemoryNodeData());
            _store[parent].Children.Add(child);
            _path.Add(child.Path, child);

            Log.Information($"Create directory {child.Path}");

            return child;
        }

        protected override FileNode CreateFile(DirectoryNode parent, FileNode child)
        {
            _store.Add(child, new MemoryNodeData());
            _store[parent].Children.Add(child);
            _path.Add(child.Path, child);

            Log.Information($"Create file {child.Path}");

            return child;
        }

        protected override NodeBase Delete(NodeBase node)
        {
            if (!node.Exists())
                return node;

            _store.Remove(node);
            _store[node.Parent].Children.Remove(node);
            _path.Remove(node.Path);

            Log.Information($"Delete {node.Path}");

            return node;
        }

        protected override bool Exists(NodePath path, NodeType nodeType)
        {
            NodeBase node;
            _path.TryGetValue(path, out node);

            return node != null && node.NodeType == nodeType;
        }

        protected override NodeAttributes GetAttributes(NodeBase node)
        {
            if (!node.Exists())
                return node.Attributes;

            return _store[node].Attributes;
        }

        protected override NodeBase GetChild(string name, DirectoryNode parent)
        {
            return _store[parent].Children
                .FirstOrDefault(child => child.Name == name);
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
            //error
            if (!node.Exists())
                return NodeContent.CreateDelayedWriteContent(new MemoryStream());

            var stream = new MemoryStream();
            _store[node].Content.CopyTo(stream);

            stream.Position = 0;
            _store[node].Content.Position = 0;

            return contentParameters.AccessType == NodeContentAccess.Read
                ? NodeContent.CreateReadOnlyContent(stream)
                : NodeContent.CreateDelayedWriteContent(stream);
        }

        protected override long GetLength(NodeBase node)
        {
            if (!node.Exists())
                return 0L;

            return _store[node].Content.Length;
        }

        protected override NodeTimeInfo GetTimeInfo(NodeBase node)
        {
            return _store[node].TimeInfo;
        }

        protected override NodeBase Rename(NodeBase node, string newName)
        {
            NodeBase newNode;

            if (node.NodeType == NodeType.File)
                newNode = new FileNode(newName, node.Parent);

            else if (node.NodeType == NodeType.Directory)
                newNode = new DirectoryNode(newName, node.Parent);

            else
                throw new NotImplementedException();

            var newNodeData = _store[node];
            newNodeData.Attributes = _store[node].Attributes;
            newNodeData.Content = _store[node].Content;
            newNodeData.Children = _store[node].Children;
            newNodeData.TimeInfo = _store[node].TimeInfo;

            _store.Remove(node);
            _store[node.Parent].Children.Remove(node);
            _path.Remove(node.Path);

            _store.Add(newNode, newNodeData);
            _store[newNode.Parent].Children.Add(newNode);
            _path.Add(newNode.Path, newNode);

            Log.Information($"Rename {node.Path} to {newNode.Path}");

            return newNode;
        }

        protected override NodeBase SaveContent(NodeBase node, NodeContent content)
        {
            if (!node.Exists())
                return node;

            var stream = new MemoryStream();
            content.GetStream().CopyTo(stream);
            stream.Position = 0;

            _store[node].Content = stream;

            return node;
        }

        protected override NodeBase SetAttributes(NodeBase node, NodeAttributes attributes)
        {
            _store[node].Attributes = attributes;

            return node;
        }

        protected override NodeBase SetTimeInfo(NodeBase node, NodeTimeInfo timeInfo)
        {
            _store[node].TimeInfo = timeInfo;

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

        public List<NodeBase> Children
        {
            get;
            set;
        }

        public MemoryStream Content 
        { 
            get; 
            set; 
        }

        public NodeTimeInfo TimeInfo
        {
            get;
            set;
        }
    }
}