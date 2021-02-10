using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.DataAccess.EF.UnitOfWorks;

namespace Bhbk.Lib.Aurora.Data_EF6.UnitOfWork
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        IGenericRepository<Ambassador_EF> Ambassadors { get; }
        IGenericRepository<Network_EF> Networks { get; }
        IGenericRepository<PrivateKey_EF> PrivateKeys { get; }
        IGenericRepository<PublicKey_EF> PublicKeys { get; }
        IGenericRepository<Session_EF> Sessions { get; }
        IGenericRepository<Setting_EF> Settings { get; }
        IGenericRepository<Login_EF> Logins { get; }
        IGenericRepository<Alert_EF> Alerts { get; }
        IGenericRepository<File_EF> Files { get; }
        IGenericRepository<Folder_EF> Folders { get; }
        IGenericRepository<Mount_EF> Mounts { get; }
        IGenericRepository<LoginUsage_EF> Usages { get; }
    }
}
