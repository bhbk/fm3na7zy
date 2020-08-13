﻿using Bhbk.Daemon.Aurora.SFTP.Helpers;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Identity.Services;
using Microsoft.Extensions.DependencyInjection;
using Rebex.IO.FileSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

/*
 * https://forum.rebex.net/8453/implement-filesystem-almost-as-memoryfilesystemprovider
 */
namespace Bhbk.Daemon.Aurora.SFTP.FileSystems
{
    internal class MemoryReadWriteFileSystem : ReadWriteFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private readonly Dictionary<NodePath, NodeBase> _path;
        private readonly Dictionary<NodeBase, MemoryNodeData> _store;
        private readonly tbl_Users _user;

        internal MemoryReadWriteFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, tbl_Users user)
            : base(settings)
        {
            _factory = factory;
            _user = user;

            _path = new Dictionary<NodePath, NodeBase>();
            _store = new Dictionary<NodeBase, MemoryNodeData>();

            MemoryFileSystemHelper.EnsureRootExists(Root, _path, _store);

            using (var scope = _factory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var me = scope.ServiceProvider.GetRequiredService<IMeService>();

                MemoryFileSystemHelper.GenerateContentForMOTD(Root, _path, _store, me);
            }
        }

        protected override DirectoryNode CreateDirectory(DirectoryNode parent, DirectoryNode child)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            _store.Add(child, new MemoryNodeData());
            _store[parent].Children.Add(child);
            _path.Add(child.Path, child);

            Log.Information($"'{callPath}' '{_user.UserName}' folder '{child.Path}'");

            return child;
        }

        protected override FileNode CreateFile(DirectoryNode parent, FileNode child)
        {
            _store.Add(child, new MemoryNodeData());
            _store[parent].Children.Add(child);
            _path.Add(child.Path, child);

            return child;
        }

        protected override NodeBase Delete(NodeBase node)
        {
            if (!node.Exists())
                return node;

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            _store.Remove(node);
            _store[node.Parent].Children.Remove(node);
            _path.Remove(node.Path);

            if (node.NodeType == NodeType.Directory)
                Log.Information($"'{callPath}' '{_user.UserName}' folder '{node.Path}'");

            else if (node.NodeType == NodeType.File)
                Log.Information($"'{callPath}' '{_user.UserName}' file '{node.Path}'");

            else
                throw new NotImplementedException();

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
            //error
            if (!node.Exists())
                return NodeContent.CreateDelayedWriteContent(new MemoryStream());

            var resultStream = new MemoryStream();
            _store[node].Content.CopyTo(resultStream);

            resultStream.Position = 0;
            _store[node].Content.Position = 0;

            return contentParameters.AccessType == NodeContentAccess.Read
                ? NodeContent.CreateReadOnlyContent(resultStream)
                : NodeContent.CreateDelayedWriteContent(resultStream);
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

        protected override NodeBase Move(NodeBase toBeMovedNode, DirectoryNode targetDirectory)
        {
            throw new NotImplementedException();
        }

        protected override NodeBase Rename(NodeBase node, string newName)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            NodeBase newNode = null;
            NodePath newNodePath = null;
            MemoryNodeData newNodeData = null;

            if (node.NodeType == NodeType.Directory)
                newNode = new DirectoryNode(newName, node.Parent);

            else if (node.NodeType == NodeType.File)
                newNode = new FileNode(newName, node.Parent);

            else
                throw new NotImplementedException();

            newNodeData = _store[node];
            newNodePath = new NodePath(node.Path + newName);

            _store.Add(newNode, newNodeData);
            _store[node.Parent].Children.Add(newNode);
            _path.Add(newNodePath, newNode);

            _store.Remove(node);
            _store[node.Parent].Children.Remove(node);
            _path.Remove(node.Path);

            Log.Information($"'{callPath}' '{_user.UserName}' from '{node.Path}' to '{node.Parent.Path}/{newName}'");

            return newNode;
        }

        protected override NodeBase SaveContent(NodeBase node, NodeContent content)
        {
            if (!node.Exists())
                return node;

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var newStream = new MemoryStream();
            content.GetStream().CopyTo(newStream);
            newStream.Position = 0;

            _store[node].Content = newStream;

            Log.Information($"'{callPath}' '{_user.UserName}' file '{node.Path}'");

            return node;
        }

        protected override NodeBase SetAttributes(NodeBase node, NodeAttributes attributes)
        {
            _store[node].Attributes = attributes;

            return node;
        }

        protected override NodeBase SetTimeInfo(NodeBase node, NodeTimeInfo newTimeInfo)
        {
            _store[node].TimeInfo = newTimeInfo;

            return node;
        }
    }
}