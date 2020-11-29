using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.DataAccess.EF.UnitOfWorks;

namespace Bhbk.Lib.Aurora.Data_EF6.UnitOfWork
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        IGenericRepository<Credential> Credentials { get; }
        IGenericRepository<Network> Networks { get; }
        IGenericRepository<PrivateKey> PrivateKeys { get; }
        IGenericRepository<PublicKey> PublicKeys { get; }
        IGenericRepository<Session> Sessions { get; }
        IGenericRepository<Setting> Settings { get; }
        IGenericRepository<User> Users { get; }
        IGenericRepository<UserAlert> UserAlerts { get; }
        IGenericRepository<UserFile> UserFiles { get; }
        IGenericRepository<UserFolder> UserFolders { get; }
        IGenericRepository<UserMount> UserMounts { get; }
    }
}
