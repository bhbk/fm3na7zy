using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.DataAccess.EF.UnitOfWorks;

namespace Bhbk.Lib.Aurora.Data_EF6.UnitOfWork
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        IGenericRepository<E_Ambassador> Ambassadors { get; }
        IGenericRepository<E_Network> Networks { get; }
        IGenericRepository<E_PrivateKey> PrivateKeys { get; }
        IGenericRepository<E_PublicKey> PublicKeys { get; }
        IGenericRepository<E_Session> Sessions { get; }
        IGenericRepository<E_Setting> Settings { get; }
        IGenericRepository<E_Login> Logins { get; }
        IGenericRepository<E_Alert> Alerts { get; }
        IGenericRepository<E_File> Files { get; }
        IGenericRepository<E_Folder> Folders { get; }
        IGenericRepository<E_Mount> Mounts { get; }
        IGenericRepository<E_Usage> Usages { get; }
    }
}
