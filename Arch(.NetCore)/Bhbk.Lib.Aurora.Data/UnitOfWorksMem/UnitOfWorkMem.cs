using Bhbk.Lib.Aurora.Data.ModelsMem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace Bhbk.Lib.Aurora.Data.UnitOfWorksMem
{
    public class UnitOfWorkMem : IUnitOfWorkMem, IDisposable
    {
        private readonly AuroraEntitiesMem _context;
        private readonly ILoggerFactory _logger;
        public InstanceContext InstanceType { get; private set; }
        public IGenericRepository<FileMem_EF> Files { get; private set; }
        public IGenericRepository<FolderMem_EF> Folders { get; private set; }
        public IGenericRepository<LoginMem_EF> Logins { get; private set; }

        public UnitOfWorkMem(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWorkMem(string connection, IContextService env)
        {
            _logger = LoggerFactory.Create(opt =>
            {
                opt.AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddConsole();
            });

            switch (env.InstanceType)
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
             * with entity framework core it appears must open connection. the delete and create 
             * are pre-flight checks to guard against errors if schema has changed.
             */

            _context.Database.OpenConnection();
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            InstanceType = env.InstanceType;

            Files = new GenericRepository<FileMem_EF>(_context);
            Folders = new GenericRepository<FolderMem_EF>(_context);
            Logins = new GenericRepository<LoginMem_EF>(_context);
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
