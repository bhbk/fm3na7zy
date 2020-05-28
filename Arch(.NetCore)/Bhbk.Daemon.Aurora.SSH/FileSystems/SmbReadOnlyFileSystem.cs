using Bhbk.Daemon.Aurora.SSH.Helpers;
using Bhbk.Lib.Aurora.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
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
using System.Reflection;

namespace Bhbk.Daemon.Aurora.SSH.FileSystems
{
    public class SmbReadOnlyFileSystem : ReadOnlyFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private tbl_Users _user;

        public SmbReadOnlyFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, tbl_Users user)
            : base(settings)
        {
            _factory = factory;
            _user = user;
        }

        protected override bool Exists(NodePath path, NodeType nodeType)
        {
            throw new NotImplementedException();
        }

        protected override NodeBase GetChild(string name, DirectoryNode parent)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<NodeBase> GetChildren(DirectoryNode parent, NodeType nodeType)
        {
            throw new NotImplementedException();
        }

        protected override NodeContent GetContent(NodeBase node, NodeContentParameters contentParameters)
        {
            throw new NotImplementedException();
        }

        protected override long GetLength(NodeBase node)
        {
            throw new NotImplementedException();
        }

        protected override NodeTimeInfo GetTimeInfo(NodeBase node)
        {
            throw new NotImplementedException();
        }
    }
}
