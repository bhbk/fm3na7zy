using Bhbk.Lib.Aurora.Data.Models;
using Bhbk.Lib.Aurora.Data.Repositories;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
//using EntityFrameworkCore.Testing.Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;

namespace Bhbk.Lib.Aurora.Data.Infrastructure
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AuroraEntities _context;
        private readonly ILoggerFactory _logger;
        public InstanceContext InstanceType { get; private set; }
        public ActivityRepository Activities { get; private set; }
        public IGenericRepository<uvw_Credential> Credentials { get; private set; }
        public IGenericRepository<uvw_Network> Networks { get; private set; }
        public IGenericRepository<uvw_PrivateKey> PrivateKeys { get; private set; }
        public IGenericRepository<uvw_PublicKey> PublicKeys { get; private set; }
        public IGenericRepository<uvw_Setting> Settings { get; private set; }
        public IGenericRepository<uvw_User> Users { get; private set; }
        public IGenericRepository<uvw_UserAlert> UserAlerts { get; private set; }
        public IGenericRepository<uvw_UserFile> UserFiles { get; private set; }
        public IGenericRepository<uvw_UserFolder> UserFolders { get; private set; }
        public IGenericRepository<uvw_UserMount> UserMounts { get; private set; }

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
                        //_context = Create.MockedDbContextFor<IdentityEntities>();
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            _context.ChangeTracker.LazyLoadingEnabled = false;
            _context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.Immediate;

            InstanceType = instance.InstanceType;

            Activities = new ActivityRepository(_context);
            Credentials = new GenericRepository<uvw_Credential>(_context);
            Networks = new GenericRepository<uvw_Network>(_context);
            PrivateKeys = new GenericRepository<uvw_PrivateKey>(_context);
            PublicKeys = new GenericRepository<uvw_PublicKey>(_context);
            Settings = new GenericRepository<uvw_Setting>(_context);
            Users = new GenericRepository<uvw_User>(_context);
            UserAlerts = new GenericRepository<uvw_UserAlert>(_context);
            UserFiles = new GenericRepository<uvw_UserFile>(_context);
            UserFolders = new GenericRepository<uvw_UserFolder>(_context);
            UserMounts = new GenericRepository<uvw_UserMount>(_context);
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
