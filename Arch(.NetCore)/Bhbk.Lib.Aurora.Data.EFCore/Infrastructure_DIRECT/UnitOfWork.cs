﻿using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Aurora.Data.EFCore.Repositories_DIRECT;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;

namespace Bhbk.Lib.Aurora.Data.EFCore.Infrastructure_DIRECT
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AuroraEntities _context;
        private readonly ILoggerFactory _logger;
        public InstanceContext InstanceType { get; private set; }
        public SysSettingRepository SysSettings { get; private set; }
        public SysCredentialRepository SysCredentials { get; private set; }
        public SysPrivateKeyRepository SysPrivateKeys { get; private set; }
        public UserFileRepository UserFiles { get; private set; }
        public UserFolderRepository UserFolders { get; private set; }
        public UserMountRepository UserMounts { get; private set; }
        public UserPasswordRepository UserPasswords { get; private set; }
        public UserPrivateKeyRepository UserPrivateKeys { get; private set; }
        public UserPublicKeyRepository UserPublicKeys { get; private set; }
        public UserRepository Users { get; private set; }

        public UnitOfWork(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWork(string connection, IContextService instance)
        {
            _logger = LoggerFactory.Create(opt =>
            {
                opt.AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information)
                    .AddFilter("System", LogLevel.Warning)
                    .AddConsole();
            });

            switch (instance.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                    {
#if !RELEASE
                        var builder = new DbContextOptionsBuilder<AuroraEntities>()
                            .UseSqlServer(connection);
                            //.UseLoggerFactory(_logger)
                            //.EnableSensitiveDataLogging();
#elif RELEASE
                        var builder = new DbContextOptionsBuilder<AuroraEntities>()
                            .UseSqlServer(connection);
#endif
                        _context = new AuroraEntities(builder.Options);
                    }
                    break;

                case InstanceContext.End2EndTest:
                case InstanceContext.IntegrationTest:
                case InstanceContext.UnitTest:
                    {
#if !RELEASE
                        var builder = new DbContextOptionsBuilder<AuroraEntities>()
                            .UseInMemoryDatabase(":InMemory:")
                            .UseLoggerFactory(_logger)
                            .EnableSensitiveDataLogging();
#elif RELEASE
                        var builder = new DbContextOptionsBuilder<AuroraEntities>()
                            .UseInMemoryDatabase(":InMemory:");
#endif
                        _context = new AuroraEntities(builder.Options);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            _context.ChangeTracker.LazyLoadingEnabled = false;
            _context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;

            InstanceType = instance.InstanceType;

            SysCredentials = new SysCredentialRepository(_context);
            SysPrivateKeys = new SysPrivateKeyRepository(_context);
            SysSettings = new SysSettingRepository(_context);
            UserFiles = new UserFileRepository(_context);
            UserFolders = new UserFolderRepository(_context);
            UserMounts = new UserMountRepository(_context);
            UserPasswords = new UserPasswordRepository(_context);
            UserPrivateKeys = new UserPrivateKeyRepository(_context);
            UserPublicKeys = new UserPublicKeyRepository(_context);
            Users = new UserRepository(_context);
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _logger.Dispose();
            _context.Dispose();
        }
    }
}
