using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EF.Repositories;
using System;
using System.Diagnostics;

namespace Bhbk.Lib.Aurora.Data_EF6.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AuroraEntities _context;
        public InstanceContext InstanceType { get; private set; }
        public IGenericRepository<Credential> Credentials { get; private set; }
        public IGenericRepository<Network> Networks { get; private set; }
        public IGenericRepository<PrivateKey> PrivateKeys { get; private set; }
        public IGenericRepository<PublicKey> PublicKeys { get; private set; }
        public IGenericRepository<Session> Sessions { get; private set; }
        public IGenericRepository<Setting> Settings { get; private set; }
        public IGenericRepository<User> Users { get; private set; }
        public IGenericRepository<UserAlert> UserAlerts { get; private set; }
        public IGenericRepository<UserFile> UserFiles { get; private set; }
        public IGenericRepository<UserFolder> UserFolders { get; private set; }
        public IGenericRepository<UserMount> UserMounts { get; private set; }

        public UnitOfWork(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWork(string connection, IContextService instance)
        {
            switch (instance.InstanceType)
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

            InstanceType = instance.InstanceType;

            Credentials = new GenericRepository<Credential>(_context);
            Networks = new GenericRepository<Network>(_context);
            PrivateKeys = new GenericRepository<PrivateKey>(_context);
            PublicKeys = new GenericRepository<PublicKey>(_context);
            Sessions = new GenericRepository<Session>(_context);
            Settings = new GenericRepository<Setting>(_context);
            Users = new GenericRepository<User>(_context);
            UserAlerts = new GenericRepository<UserAlert>(_context);
            UserFiles = new GenericRepository<UserFile>(_context);
            UserFolders = new GenericRepository<UserFolder>(_context);
            UserMounts = new GenericRepository<UserMount>(_context);
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
