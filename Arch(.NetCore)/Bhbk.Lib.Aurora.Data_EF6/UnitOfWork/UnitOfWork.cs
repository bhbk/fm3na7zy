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
        public IGenericRepository<E_Alert> Alerts { get; private set; }
        public IGenericRepository<E_Ambassador> Ambassadors { get; private set; }
        public IGenericRepository<E_File> Files { get; private set; }
        public IGenericRepository<E_Folder> Folders { get; private set; }
        public IGenericRepository<E_Login> Logins { get; private set; }
        public IGenericRepository<E_Mount> Mounts { get; private set; }
        public IGenericRepository<E_Network> Networks { get; private set; }
        public IGenericRepository<E_PrivateKey> PrivateKeys { get; private set; }
        public IGenericRepository<E_PublicKey> PublicKeys { get; private set; }
        public IGenericRepository<E_Session> Sessions { get; private set; }
        public IGenericRepository<E_Setting> Settings { get; private set; }
        public IGenericRepository<E_LoginUsage> Usages { get; private set; }

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

            Alerts = new GenericRepository<E_Alert>(_context);
            Ambassadors = new GenericRepository<E_Ambassador>(_context);
            Files = new GenericRepository<E_File>(_context);
            Folders = new GenericRepository<E_Folder>(_context);
            Logins = new GenericRepository<E_Login>(_context);
            Mounts = new GenericRepository<E_Mount>(_context);
            Networks = new GenericRepository<E_Network>(_context);
            PrivateKeys = new GenericRepository<E_PrivateKey>(_context);
            PublicKeys = new GenericRepository<E_PublicKey>(_context);
            Sessions = new GenericRepository<E_Session>(_context);
            Settings = new GenericRepository<E_Setting>(_context);
            Usages = new GenericRepository<E_LoginUsage>(_context);
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
