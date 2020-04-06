using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;
using Bhbk.Lib.Aurora.Data.Repositories_DIRECT;

namespace Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        public SystemKeyRepository SystemKeys { get; }
        public SettingsRepository Settings { get; }
        public UserFileRepository UserFiles { get; }
        public UserFolderRepository UserFolders { get; }
        public UserPasswordRepository UserPasswords { get; }
        public UserPrivateKeyRepository UserPrivateKeys { get; }
        public UserPublicKeyRepository UserPublicKeys { get; }
        public UserRepository Users { get; }
    }
}
