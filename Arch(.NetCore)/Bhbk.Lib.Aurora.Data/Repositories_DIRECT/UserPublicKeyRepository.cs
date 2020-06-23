using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class UserPublicKeyRepository : GenericRepository<tbl_UserPublicKeys>
    {
        public UserPublicKeyRepository(AuroraEntities context)
            : base(context) { }
    }
}
