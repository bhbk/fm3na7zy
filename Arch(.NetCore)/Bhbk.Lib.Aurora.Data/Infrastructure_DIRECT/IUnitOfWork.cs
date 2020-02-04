using Bhbk.Lib.DataAccess.EFCore.UnitOfWorks;
using Bhbk.Lib.Aurora.Data.Repositories_DIRECT;

namespace Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT
{
    public interface IUnitOfWork : IGenericUnitOfWork
    {
        public PrivateKeyRepository PrivateKeys { get; }
        public SettingsRepository Settings { get; }
        public UserPrivateKeyRepository UserPrivateKeys { get; }
        public UserPublicKeyRepository UserPublicKeys { get; }
        public UserRepository Users { get; }
    }
}
