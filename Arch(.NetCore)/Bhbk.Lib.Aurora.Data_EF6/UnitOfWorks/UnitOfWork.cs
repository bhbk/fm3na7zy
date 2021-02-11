using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EF.Repositories;
using System;
using System.Diagnostics;

namespace Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AuroraEntities _context;
        public InstanceContext InstanceType { get; private set; }
        public IGenericRepository<Alert_EF> Alerts { get; private set; }
        public IGenericRepository<Ambassador_EF> Ambassadors { get; private set; }
        public IGenericRepository<File_EF> Files { get; private set; }
        public IGenericRepository<FileSystem_EF> FileSystems { get; private set; }
        public IGenericRepository<FileSystemLogin_EF> FileSystemLogins { get; private set; }
        public IGenericRepository<FileSystemUsage_EF> FileSystemUsages { get; private set; }
        public IGenericRepository<Folder_EF> Folders { get; private set; }
        public IGenericRepository<Login_EF> Logins { get; private set; }
        public IGenericRepository<LoginUsage_EF> LoginUsages { get; private set; }
        public IGenericRepository<Network_EF> Networks { get; private set; }
        public IGenericRepository<PrivateKey_EF> PrivateKeys { get; private set; }
        public IGenericRepository<PublicKey_EF> PublicKeys { get; private set; }
        public IGenericRepository<Session_EF> Sessions { get; private set; }
        public IGenericRepository<Setting_EF> Settings { get; private set; }

        public UnitOfWork(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWork(string connection, IContextService env)
        {
            switch (env.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                case InstanceContext.End2EndTest:
                case InstanceContext.SystemTest:
                case InstanceContext.IntegrationTest:
                    {
                        _context = new AuroraEntitiesFactory(connection).Create();
#if !RELEASE
                        _context.Database.Log = x => Debug.WriteLine(x);
#endif
                    }
                    break;

                case InstanceContext.UnitTest:
                    {
                        throw new NotImplementedException();
                    }

                default:
                    throw new NotImplementedException();
            }

            _context.Configuration.LazyLoadingEnabled = false;
            _context.Configuration.ProxyCreationEnabled = true;

            InstanceType = env.InstanceType;

            Alerts = new GenericRepository<Alert_EF>(_context);
            Ambassadors = new GenericRepository<Ambassador_EF>(_context);
            Files = new GenericRepository<File_EF>(_context);
            FileSystems = new GenericRepository<FileSystem_EF>(_context);
            FileSystemLogins = new GenericRepository<FileSystemLogin_EF>(_context);
            FileSystemUsages = new GenericRepository<FileSystemUsage_EF>(_context);
            Folders = new GenericRepository<Folder_EF>(_context);
            Logins = new GenericRepository<Login_EF>(_context);
            LoginUsages = new GenericRepository<LoginUsage_EF>(_context);
            Networks = new GenericRepository<Network_EF>(_context);
            PrivateKeys = new GenericRepository<PrivateKey_EF>(_context);
            PublicKeys = new GenericRepository<PublicKey_EF>(_context);
            Sessions = new GenericRepository<Session_EF>(_context);
            Settings = new GenericRepository<Setting_EF>(_context);
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
