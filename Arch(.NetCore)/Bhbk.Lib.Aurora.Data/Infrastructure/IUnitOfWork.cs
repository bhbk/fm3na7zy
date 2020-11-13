using Bhbk.Lib.Aurora.Data.Models;
using Bhbk.Lib.Aurora.Data.Repositories;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;

namespace Bhbk.Lib.Aurora.Data.Infrastructure
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        ActivityRepository Activities { get; }
        public IGenericRepository<uvw_Credential> Credentials { get; }
        public IGenericRepository<uvw_Network> Networks { get; }
        public IGenericRepository<uvw_PrivateKey> PrivateKeys { get; }
        public IGenericRepository<uvw_PublicKey> PublicKeys { get; }
        public IGenericRepository<uvw_Setting> Settings { get; }
        public IGenericRepository<uvw_UserFile> UserFiles { get; }
        public IGenericRepository<uvw_UserFolder> UserFolders { get; }
        public IGenericRepository<uvw_UserMount> UserMounts { get; }
        public IGenericRepository<uvw_User> Users { get; }
    }
}
