using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Data.Repositories_DIRECT;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;

namespace Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AuroraEntities _context;
        private readonly ILoggerFactory _logger;
        public InstanceContext InstanceType { get; private set; }
        public ActivityRepository Activities { get; private set; }
        public IGenericRepository<tbl_Credential> Credentials { get; private set; }
        public IGenericRepository<tbl_Network> Networks { get; private set; }
        public IGenericRepository<tbl_PrivateKey> PrivateKeys { get; private set; }
        public IGenericRepository<tbl_PublicKey> PublicKeys { get; private set; }
        public IGenericRepository<tbl_Setting> Settings { get; private set; }
        public IGenericRepository<tbl_User> Users { get; private set; }
        public IGenericRepository<tbl_UserAlert> UserAlerts { get; private set; }
        public IGenericRepository<tbl_UserFile> UserFiles { get; private set; }
        public IGenericRepository<tbl_UserFolder> UserFolders { get; private set; }
        public IGenericRepository<tbl_UserMount> UserMounts { get; private set; }

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

            Activities = new ActivityRepository(_context);
            Credentials = new GenericRepository<tbl_Credential>(_context);
            Networks = new GenericRepository<tbl_Network>(_context);
            PrivateKeys = new GenericRepository<tbl_PrivateKey>(_context);
            PublicKeys = new GenericRepository<tbl_PublicKey>(_context);
            Settings = new GenericRepository<tbl_Setting>(_context);
            Users = new GenericRepository<tbl_User>(_context);
            UserAlerts = new GenericRepository<tbl_UserAlert>(_context);
            UserFiles = new GenericRepository<tbl_UserFile>(_context);
            UserFolders = new GenericRepository<tbl_UserFolder>(_context);
            UserMounts = new GenericRepository<tbl_UserMount>(_context);
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
