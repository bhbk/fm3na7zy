using Bhbk.Lib.Aurora.Data.ModelsMem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace Bhbk.Lib.Aurora.Data.UnitOfWorkMem
{
    public class UnitOfWorkMem : IUnitOfWorkMem, IDisposable
    {
        private readonly AuroraEntitiesMem _context;
        private readonly ILoggerFactory _logger;
        public InstanceContext InstanceType { get; private set; }
        public IGenericRepository<UserMem> Users { get; private set; }
        public IGenericRepository<UserFileMem> UserFiles { get; private set; }
        public IGenericRepository<UserFolderMem> UserFolders { get; private set; }

        public UnitOfWorkMem(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWorkMem(string connection, IContextService instance)
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
                case InstanceContext.End2EndTest:
                case InstanceContext.SystemTest:
                case InstanceContext.IntegrationTest:
                case InstanceContext.UnitTest:
                    {
                        if (string.IsNullOrEmpty(connection))
                        {
                            var options = new SqliteConnectionStringBuilder()
                            {
                                DataSource = ":memory:",
                                Mode = SqliteOpenMode.Memory,
                                Cache = SqliteCacheMode.Private,
                            };

                            connection = options.ConnectionString;
                        }
#if !RELEASE
                        var builder = new DbContextOptionsBuilder<AuroraEntitiesMem>()
                            .UseSqlite(connection)
                            .UseLoggerFactory(_logger)
                            .EnableSensitiveDataLogging();
#else
                        var builder = new DbContextOptionsBuilder<AuroraEntitiesMem>()
                            .UseSqlite(connection);
#endif
                        _context = new AuroraEntitiesMem(builder.Options);
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            /*
             * observed persistence of sqlite databases seems to vary depending on the .net architecture that is
             * consuming it as well as parameters defined in configuration files. with entity framework core it appears must
             * open connection. the delete and create are pre-flight checks to guard against errors if schema has changed.
             */

            _context.Database.OpenConnection();
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            InstanceType = instance.InstanceType;

            Users = new GenericRepository<UserMem>(_context);
            UserFiles = new GenericRepository<UserFileMem>(_context);
            UserFolders = new GenericRepository<UserFolderMem>(_context);
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
