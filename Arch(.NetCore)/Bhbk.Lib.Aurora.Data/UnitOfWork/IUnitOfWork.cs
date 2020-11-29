using Bhbk.Lib.Aurora.Data.Repositories;
using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;

namespace Bhbk.Lib.Aurora.Data.UnitOfWork
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        public CredentialRepository Credentials { get; }
        public NetworkRepository Networks { get; }
        public PrivateKeyRepository PrivateKeys { get; }
        public PublicKeyRepository PublicKeys { get; }
        public SessionRepository Sessions { get; }
        public SettingRepository Settings { get; }
        public UserRepository Users { get; }
        public UserAlertRepository UserAlerts { get; }
        public UserFileRepository UserFiles { get; }
        public UserFolderRepository UserFolders { get; }
        public UserMountRepository UserMounts { get; }
    }
}
