using Bhbk.Lib.Aurora.Data.Models;
using Bhbk.Lib.Aurora.Data.Repositories;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
//using EntityFrameworkCore.Testing.Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;

namespace Bhbk.Lib.Aurora.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AuroraEntities _context;
        private readonly ILoggerFactory _logger;
        public InstanceContext InstanceType { get; private set; }
        public CredentialRepository Credentials { get; private set; }
        public NetworkRepository Networks { get; private set; }
        public PrivateKeyRepository PrivateKeys { get; private set; }
        public PublicKeyRepository PublicKeys { get; private set; }
        public SettingRepository Settings { get; private set; }
        public UserRepository Users { get; private set; }
        public UserAlertRepository UserAlerts { get; private set; }
        public UserFileRepository UserFiles { get; private set; }
        public UserFolderRepository UserFolders { get; private set; }
        public UserMountRepository UserMounts { get; private set; }

        public UnitOfWork(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWork(string connection, IContextService instance)
        {
            _logger = LoggerFactory.Create(opt =>
            {
                opt.AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddConsole();
            });

            switch (instance.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                    {
#if !RELEASE
                        var builder = new DbContextOptionsBuilder<AuroraEntities>()
                            .UseSqlServer(connection)
                            .UseLoggerFactory(_logger)
                            .EnableSensitiveDataLogging();
#else
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
#else
                        var builder = new DbContextOptionsBuilder<AuroraEntities>()
                            .UseInMemoryDatabase(":InMemory:");
#endif
                        _context = new AuroraEntities(builder.Options);
                        //_context = Create.MockedDbContextFor<IdentityEntities>();
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            _context.ChangeTracker.LazyLoadingEnabled = false;
            _context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;

            InstanceType = instance.InstanceType;

            Credentials = new CredentialRepository(_context);
            Networks = new NetworkRepository(_context);
            PrivateKeys = new PrivateKeyRepository(_context);
            PublicKeys = new PublicKeyRepository(_context);
            Settings = new SettingRepository(_context);
            Users = new UserRepository(_context);
            UserAlerts = new UserAlertRepository(_context);
            UserFiles = new UserFileRepository(_context);
            UserFolders = new UserFolderRepository(_context);
            UserMounts = new UserMountRepository(_context);
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
