using Bhbk.Daemon.Aurora.SFTP.Helpers;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
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
    internal class MemoryReadOnlyFileSystem : ReadOnlyFileSystemProvider, IDisposable
    {
        private readonly IServiceScopeFactory _factory;
        private readonly Dictionary<NodePath, NodeBase> _path;
        private readonly Dictionary<NodeBase, MemoryNodeData> _store;
        private readonly tbl_Users _userEntity;

        internal MemoryReadOnlyFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, tbl_Users userEntity)
            : base(settings)
        {
            _factory = factory;
            _userEntity = userEntity;

            _path = new Dictionary<NodePath, NodeBase>();
            _store = new Dictionary<NodeBase, MemoryNodeData>();

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' initialize '{typeof(MemoryReadOnlyFileSystem).Name}'");

            using (var scope = _factory.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var pubKeys = uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
                    .Where(x => x.IdentityId == _userEntity.IdentityId).ToLambda()).ToList();

                var pubKeysContent = KeyHelper.ExportPubKeyBase64(_userEntity, pubKeys);

                MemoryFileSystemHelper.EnsureRootExists(Root, _path, _store);
                MemoryFileSystemHelper.CreatePubKeysFile(Root, _path, _store, _userEntity, pubKeysContent);
            }
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

        protected override void Dispose(bool disposing)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            Log.Information($"'{callPath}' '{_userEntity.IdentityAlias}' dispose '{typeof(MemoryReadOnlyFileSystem).Name}'");

            base.Dispose(disposing);
        }
    }
}
