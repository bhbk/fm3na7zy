using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebex.IO.FileSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.DaemonSSH.Aurora.FileSystems
{
    public class DatabaseFileSystemProvider : ReadWriteFileSystemProvider
    {
        private readonly Dictionary<NodeBase, DatabaseNodeData> _store;
        private readonly Dictionary<NodePath, NodeBase> _paths;
        private readonly IServiceScopeFactory _factory;
        private readonly IConfiguration _conf;
        private readonly tbl_Users _user;

        public DatabaseFileSystemProvider(IServiceScopeFactory factory, IConfiguration conf, tbl_Users user)
        {
            _factory = factory;
            _conf = conf;
            _user = user;

            _store = new Dictionary<NodeBase, DatabaseNodeData>();
            _store.Add(Root, new DatabaseNodeData());

            _paths = new Dictionary<NodePath, NodeBase>();
            _paths.Add(Root.Path, Root);
        }

        protected override DirectoryNode CreateDirectory(DirectoryNode parent, DirectoryNode child)
        {
            try
            {
                _store.Add(child, new DatabaseNodeData());
                _store[parent].Children.Add(child);
                _paths.Add(child.Path, child);

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
            try
            {
                _store.Add(child, new DatabaseNodeData());
                _store[parent].Children.Add(child);
                _paths.Add(child.Path, child);

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
            try
            {
                if (!node.Exists())
                    return node;

                _store.Remove(node);
                _store[node.Parent].Children.Remove(node);
                _paths.Remove(node.Path);

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
                NodeBase node;
                _paths.TryGetValue(path, out node);

                return node != null && node.NodeType == nodeType;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeAttributes GetAttributes(NodeBase node)
        {
            if (!node.Exists())
                return node.Attributes;

            return _store[node].Attributes;
        }

        protected override NodeBase GetChild(string name, DirectoryNode parent)
        {
            try
            {
                return _store[parent].Children
                    .FirstOrDefault(child => child.Name == name);
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

                var children = _store[parent].Children;

                foreach(var child in children)
                    Log.Information(child.Path.StringPath);

                return children;

                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                        .Where(x => x.Id == _user.Id).ToLambda(),
                            new List<Expression<Func<tbl_Users, object>>>()
                            {
                                x => x.tbl_UserFiles,
                                x => x.tbl_UserFolders,
                            }).SingleOrDefault();

                    if (parent.IsRootDirectory)
                    {
                        var nodes = new List<NodeBase>();

                        var folders = user.tbl_UserFolders.Where(x => x.UserId == _user.Id && x.VirtualParentId == null);

                        foreach (var folder in folders)
                            nodes.Add(new DirectoryNode(folder.VirtualFolderName, parent));

                        var files = user.tbl_UserFiles.Where(x => x.UserId == _user.Id && x.VirtualParentId == null);

                        foreach (var file in files)
                            nodes.Add(new FileNode(file.VirtualFileName, parent));

                        return nodes;
                    }

                    throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeContent GetContent(NodeBase node, NodeContentParameters contentParameters)
        {
            try
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

                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                        .Where(x => x.Id == _user.Id).ToLambda(),
                            new List<Expression<Func<tbl_Users, object>>>()
                            {
                                x => x.tbl_UserFiles,
                                x => x.tbl_UserFolders,
                            }).SingleOrDefault();

                    if (node.IsFile)
                    {
                        var file = user.tbl_UserFiles.Where(x => x.UserId == _user.Id
                            && x.VirtualParentId == null && x.VirtualFileName == node.Name).SingleOrDefault();

                        return file.FileSize;
                    }

                    throw new NotImplementedException();
                }
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

                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                        .Where(x => x.Id == _user.Id).ToLambda(),
                            new List<Expression<Func<tbl_Users, object>>>()
                            {
                                x => x.tbl_UserFiles,
                                x => x.tbl_UserFolders,
                            }).SingleOrDefault();

                    if (node.IsDirectory)
                    {
                        var folder = user.tbl_UserFolders.Where(x => x.UserId == _user.Id
                            && x.VirtualParentId == null && x.VirtualFolderName == node.Name).SingleOrDefault();

                        return new NodeTimeInfo(folder.Created, folder.LastAccessed, folder.LastUpdated);
                    }
                    else if (node.IsFile)
                    {
                        var file = user.tbl_UserFiles.Where(x => x.UserId == _user.Id
                            && x.VirtualParentId == null && x.VirtualFileName == node.Name).SingleOrDefault();

                        return new NodeTimeInfo(file.Created, file.LastAccessed, file.LastUpdated);
                    }

                    throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeBase Rename(NodeBase node, string newName)
        {
            try
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
                _paths.Remove(node.Path);

                _store.Add(newNode, newNodeData);
                _store[newNode.Parent].Children.Add(newNode);
                _paths.Add(newNode.Path, newNode);

                return newNode;
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
                _store[node].Attributes = attributes;

                return node;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        protected override NodeBase SaveContent(NodeBase node, NodeContent content)
        {
            try
            {
                if (!node.Exists())
                    return node;

                var stream = new MemoryStream();
                content.GetStream().CopyTo(stream);
                stream.Position = 0;

                _store[node].Content = stream;

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
            _store[node].TimeInfo = timeInfo;

            return node;
        }
    }

    internal class DatabaseNodeData
    {
        public DatabaseNodeData()
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

        public MemoryStream Content { get; set; }
    }
}
