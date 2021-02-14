﻿using Bhbk.Lib.Aurora.Data.ModelsMem;
using Bhbk.Lib.Aurora.Data.UnitOfWorksMem;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Domain.Providers;
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
using System.Net;
using System.Reflection;

namespace Bhbk.Daemon.Aurora.SFTP.Providers
{
    internal class MemoryReadOnlyFileProvider : ReadOnlyFileSystemProvider
    {
        private readonly IServiceScopeFactory _factory;
        private readonly IUnitOfWorkMem _uowMem;
        private FileSystemLogin_EF _fileSystemLogin;
        private FileSystemLoginMem _fileSystemLoginMem;

        internal MemoryReadOnlyFileProvider(FileSystemProviderSettings settings, IServiceScopeFactory factory, FileSystemLogin_EF fileSystemLogin)
            : base(settings)
        {
            _factory = factory;
            _fileSystemLogin = fileSystemLogin;

            using (var scope = _factory.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                _uowMem = new UnitOfWorkMem(conf["Databases:AuroraEntities_EFCore_Mem"]);
            }

            _fileSystemLoginMem = MemoryProvider.CheckFileSystemLogin(_uowMem, fileSystemLogin);

            MemoryProvider.CheckFolder(_uowMem, _fileSystemLoginMem);
        }

        protected override bool Exists(NodePath path, NodeType nodeType)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                switch (nodeType)
                {
                    case NodeType.File:
                        {
                            var fileEntity = MemoryProvider.PathToFile(_uowMem, _fileSystemLoginMem, path.StringPath);

                            if (fileEntity != null)
                                return true;

                            return false;
                        }

                    case NodeType.Directory:
                        {
                            var folderEntity = MemoryProvider.PathToFolder(_uowMem, _fileSystemLoginMem, path.StringPath);

                            if (folderEntity != null)
                                return true;

                            return false;
                        }

                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        protected override NodeAttributes GetAttributes(NodeBase node)
        {
            if (!node.Exists())
                return node.Attributes;

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                switch (node.NodeType)
                {
                    case NodeType.File:
                        {
                            var fileEntity = MemoryProvider.PathToFile(_uowMem, _fileSystemLoginMem, node.Path.StringPath);

                            if (fileEntity.IsReadOnly)
                                node.SetAttributes(new NodeAttributes(FileAttributes.Normal | FileAttributes.ReadOnly));

                            node.SetAttributes(new NodeAttributes(FileAttributes.Normal));
                        }
                        break;

                    case NodeType.Directory:
                        {
                            var folderEntity = MemoryProvider.PathToFolder(_uowMem, _fileSystemLoginMem, node.Path.StringPath);

                            if (folderEntity.IsReadOnly)
                                node.SetAttributes(new NodeAttributes(FileAttributes.Directory | FileAttributes.ReadOnly));

                            node.SetAttributes(new NodeAttributes(FileAttributes.Directory));
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return node.Attributes;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        protected override NodeBase GetChild(string name, DirectoryNode parent)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                NodeBase child = null;

                var folderParentEntity = MemoryProvider.PathToFolder(_uowMem, _fileSystemLoginMem, parent.Path.StringPath);

                var folderEntity = _uowMem.Folders.Get(QueryExpressionFactory.GetQueryExpression<FolderMem>()
                    .Where(x => x.FileSystemId == _fileSystemLogin.FileSystemId && x.ParentId == folderParentEntity.Id && x.VirtualName == name).ToLambda())
                    .SingleOrDefault();

                if (folderEntity != null)
                    child = new DirectoryNode(folderEntity.VirtualName, parent,
                        new NodeTimeInfo(folderEntity.CreatedUtc.UtcDateTime,
                            folderEntity.LastAccessedUtc.UtcDateTime, folderEntity.LastUpdatedUtc.UtcDateTime));

                var fileEntity = _uowMem.Files.Get(QueryExpressionFactory.GetQueryExpression<FileMem>()
                    .Where(x => x.FileSystemId == _fileSystemLogin.FileSystemId && x.FolderId == folderParentEntity.Id && x.VirtualName == name).ToLambda())
                    .SingleOrDefault();

                if (fileEntity != null)
                    child = new FileNode(fileEntity.VirtualName, parent,
                        new NodeTimeInfo(fileEntity.CreatedUtc.UtcDateTime,
                            fileEntity.LastAccessedUtc.UtcDateTime, fileEntity.LastUpdatedUtc.UtcDateTime));

                return child;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        protected override IEnumerable<NodeBase> GetChildren(DirectoryNode parent, NodeType nodeType)
        {
            if (!parent.Exists())
                return Enumerable.Empty<NodeBase>();

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                var children = new List<NodeBase>();

                var folderParentEntity = MemoryProvider.PathToFolder(_uowMem, _fileSystemLoginMem, parent.Path.StringPath);

                var folders = _uowMem.Folders.Get(QueryExpressionFactory.GetQueryExpression<FolderMem>()
                    .Where(x => x.FileSystemId == _fileSystemLogin.FileSystemId && x.ParentId == folderParentEntity.Id).ToLambda())
                    .ToList();

                foreach (var folder in folders)
                    children.Add(new DirectoryNode(folder.VirtualName, parent,
                        new NodeTimeInfo(folder.CreatedUtc.UtcDateTime,
                            folder.LastAccessedUtc.UtcDateTime, folder.LastUpdatedUtc.UtcDateTime)));

                var files = _uowMem.Files.Get(QueryExpressionFactory.GetQueryExpression<FileMem>()
                    .Where(x => x.FileSystemId == _fileSystemLogin.FileSystemId && x.FolderId == folderParentEntity.Id).ToLambda())
                    .ToList();

                foreach (var file in files)
                    children.Add(new FileNode(file.VirtualName, parent,
                        new NodeTimeInfo(file.CreatedUtc.UtcDateTime,
                            file.LastAccessedUtc.UtcDateTime, file.LastUpdatedUtc.UtcDateTime)));

                return children;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        protected override NodeContent GetContent(NodeBase node, NodeContentParameters parameters)
        {
            if (!node.Exists())
                return NodeContent.CreateDelayedWriteContent(new MemoryStream());

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                switch (node.NodeType)
                {
                    case NodeType.File:
                        {
                            MemoryStream stream = null;

                            var fileEntity = MemoryProvider.PathToFile(_uowMem, _fileSystemLoginMem, node.Path.StringPath);

                            if (fileEntity.Data == null)
                                stream = new MemoryStream();
                            else
                                stream = new MemoryStream(fileEntity.Data);

                            Log.Information($"'{callPath}' '{_fileSystemLogin.Login.UserName}' file:'{node.Path}' size:'{stream.Length / 1048576f}MB' at:memory");

                            return parameters.AccessType == NodeContentAccess.Read
                                ? NodeContent.CreateReadOnlyContent(stream)
                                : NodeContent.CreateDelayedWriteContent(stream);
                        }

                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        protected override long GetLength(NodeBase node)
        {
            if (!node.Exists())
                return 0L;

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                switch (node.NodeType)
                {
                    case NodeType.File:
                        {
                            var fileEntity = MemoryProvider.PathToFile(_uowMem, _fileSystemLoginMem, node.Path.StringPath);

                            if (fileEntity.Data == null)
                                return 0L;

                            return fileEntity.Data.Length;
                        }

                    case NodeType.Directory:
                        {
                            return 0L;
                        }

                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }

        protected override NodeTimeInfo GetTimeInfo(NodeBase node)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                switch (node.NodeType)
                {
                    case NodeType.File:
                        {
                            var fileEntity = MemoryProvider.PathToFile(_uowMem, _fileSystemLoginMem, node.Path.StringPath);

                            node.SetTimeInfo(new NodeTimeInfo(fileEntity.CreatedUtc.UtcDateTime,
                                fileEntity.LastAccessedUtc.UtcDateTime, fileEntity.LastUpdatedUtc.UtcDateTime));
                        }
                        break;

                    case NodeType.Directory:
                        {
                            var folderEntity = MemoryProvider.PathToFolder(_uowMem, _fileSystemLoginMem, node.Path.StringPath);

                            node.SetTimeInfo(new NodeTimeInfo(folderEntity.CreatedUtc.UtcDateTime,
                                folderEntity.LastAccessedUtc.UtcDateTime, folderEntity.LastUpdatedUtc.UtcDateTime));
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }

                return node.TimeInfo;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                throw;
            }
        }
    }
}
