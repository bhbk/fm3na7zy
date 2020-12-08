using Bhbk.Lib.Aurora.Data_EF6.ModelsMem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataAccess.EF.Repositories;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bhbk.Lib.Aurora.Data_EF6.UnitOfWorkMem
{
    public class UnitOfWorkMem : IUnitOfWorkMem, IDisposable
    {
        private readonly AuroraEntitiesMem _context;
        public InstanceContext InstanceType { get; private set; }
        public IGenericRepository<E_FileMem> UserFiles { get; private set; }
        public IGenericRepository<E_FolderMem> UserFolders { get; private set; }
        public IGenericRepository<E_LoginMem> UserLogins { get; private set; }

        public UnitOfWorkMem(string connection)
            : this(connection, new ContextService(InstanceContext.DeployedOrLocal)) { }

        public UnitOfWorkMem(string connection, IContextService instance)
        {
            /*
             * dependency on localdb for sqlite backed by entity framework 6 will only run windows platforms.
             */

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotSupportedException();

            switch (instance.InstanceType)
            {
                case InstanceContext.DeployedOrLocal:
                case InstanceContext.End2EndTest:
                case InstanceContext.SystemTest:
                case InstanceContext.IntegrationTest:
                case InstanceContext.UnitTest:
                    {
                        _context = new AuroraEntitiesMem();
#if !RELEASE
                        _context.Database.Log = x => Debug.WriteLine(x);
#endif
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            /*
             * observed persistence of sqlite databases seems to vary depending on the .net architecture that is
             * consuming it as well as parameters defined in configuration files. with entity framework 6 it looks like
             * a memory based database uses localdb so this pre-flight check is needed.
             */

            if (_context.Database.Exists() 
                && !_context.Database.CompatibleWithModel(true))
                _context.Database.Delete();

            _context.Database.CreateIfNotExists();

            InstanceType = instance.InstanceType;

            UserFiles = new GenericRepository<E_FileMem>(_context);
            UserFolders = new GenericRepository<E_FolderMem>(_context);
            UserLogins = new GenericRepository<E_LoginMem>(_context);
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
