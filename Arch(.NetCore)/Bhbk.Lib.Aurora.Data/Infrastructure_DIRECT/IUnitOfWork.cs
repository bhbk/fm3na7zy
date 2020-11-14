using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Data.Repositories_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;

namespace Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        ActivityRepository Activities { get; }
        public IGenericRepository<tbl_Credential> Credentials { get; }
        public IGenericRepository<tbl_Network> Networks { get; }
        public IGenericRepository<tbl_PrivateKey> PrivateKeys { get; }
        public IGenericRepository<tbl_PublicKey> PublicKeys { get; }
        public IGenericRepository<tbl_Setting> Settings { get; }
        public IGenericRepository<tbl_User> Users { get; }
        public IGenericRepository<tbl_UserAlert> UserAlerts { get; }
        public IGenericRepository<tbl_UserFile> UserFiles { get; }
        public IGenericRepository<tbl_UserFolder> UserFolders { get; }
        public IGenericRepository<tbl_UserMount> UserMounts { get; }
    }
}
