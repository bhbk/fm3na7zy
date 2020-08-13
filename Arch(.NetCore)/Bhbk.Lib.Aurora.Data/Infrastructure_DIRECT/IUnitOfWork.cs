using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;
using Bhbk.Lib.Aurora.Data.Repositories_DIRECT;

namespace Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        public AmbassadorRepository Ambassadors { get; }
        public PrivateKeyRepository PrivateKeys { get; }
        public PublicKeyRepository PublicKeys { get; }
        public SettingRepository Settings { get; }
        public UserFileRepository UserFiles { get; }
        public UserFolderRepository UserFolders { get; }
        public UserMountRepository UserMounts { get; }
        public UserPasswordRepository UserPasswords { get; }
        public UserRepository Users { get; }
    }
}
