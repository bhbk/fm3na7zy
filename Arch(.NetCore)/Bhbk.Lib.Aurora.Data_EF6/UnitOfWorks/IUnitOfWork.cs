using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.DataAccess.EF.Repositories;
using Bhbk.Lib.DataAccess.EF.UnitOfWorks;

namespace Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        IGenericRepository<Alert_EF> Alerts { get; }
        IGenericRepository<Ambassador_EF> Ambassadors { get; }
        IGenericRepository<File_EF> Files { get; }
        IGenericRepository<FileSystem_EF> FileSystems { get; }
        IGenericRepository<FileSystemLogin_EF> FileSystemLogins { get; }
        IGenericRepository<FileSystemUsage_EF> FileSystemUsages { get; }
        IGenericRepository<Folder_EF> Folders { get; }
        IGenericRepository<Login_EF> Logins { get; }
        IGenericRepository<LoginUsage_EF> LoginUsages { get; }
        IGenericRepository<Network_EF> Networks { get; }
        IGenericRepository<PrivateKey_EF> PrivateKeys { get; }
        IGenericRepository<PublicKey_EF> PublicKeys { get; }
        IGenericRepository<Session_EF> Sessions { get; }
        IGenericRepository<Setting_EF> Settings { get; }
    }
}
