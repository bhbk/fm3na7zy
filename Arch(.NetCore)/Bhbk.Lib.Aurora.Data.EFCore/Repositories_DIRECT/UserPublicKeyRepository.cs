using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.EFCore.Repositories_DIRECT
{
    public class UserPublicKeyRepository : GenericRepository<tbl_UserPublicKeys>
    {
        public UserPublicKeyRepository(AuroraEntities context)
            : base(context) { }
    }
}
