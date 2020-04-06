using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebex.IO.FileSystem;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using Rebex;

namespace Bhbk.DaemonSSH.Aurora.FileSystems
{
    public class CompositeReadOnlyFileSystem : ReadOnlyFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private readonly Guid _id;
        private LogLevel _level;

        public CompositeReadOnlyFileSystem(FileSystemProviderSettings settings, IServiceScopeFactory factory, Guid id)
            : base(settings)
        {
            _factory = factory;
            _id = id;

            var scope = _factory.CreateScope();
            _conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            _uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
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
