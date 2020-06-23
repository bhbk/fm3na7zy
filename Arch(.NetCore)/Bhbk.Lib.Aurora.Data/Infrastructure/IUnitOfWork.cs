using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;
using Bhbk.Lib.Aurora.Data.Repositories;

namespace Bhbk.Lib.Aurora.Data.Infrastructure
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        public SysCredentialRepository SysCredentials { get; }
        public SysPrivateKeyRepository SysPrivateKeys { get; }
        public SysSettingRepository SysSettings { get; }
        public UserFileRepository UserFiles { get; }
        public UserFolderRepository UserFolders { get; }
        public UserMountRepository UserMounts { get; }
        public UserPasswordRepository UserPasswords { get; }
        public UserPrivateKeyRepository UserPrivateKeys { get; }
        public UserPublicKeyRepository UserPublicKeys { get; }
        public UserRepository Users { get; }
    }
}
