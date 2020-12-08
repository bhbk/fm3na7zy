using Bhbk.Lib.Aurora.Data.Repositories;
using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;

namespace Bhbk.Lib.Aurora.Data.UnitOfWork
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        public AlertRepository Alerts { get; }
        public AmbassadorRepository Ambassadors { get; }
        public LoginRepository Logins { get; }
        public FileRepository Files { get; }
        public FolderRepository Folders { get; }
        public MountRepository Mounts { get; }
        public NetworkRepository Networks { get; }
        public PrivateKeyRepository PrivateKeys { get; }
        public PublicKeyRepository PublicKeys { get; }
        public SessionRepository Sessions { get; }
        public SettingRepository Settings { get; }
        public UsageRepository Usages { get; }
    }
}
