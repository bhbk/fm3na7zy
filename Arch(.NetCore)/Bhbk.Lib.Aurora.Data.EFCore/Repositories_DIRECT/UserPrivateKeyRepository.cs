using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.EFCore.Repositories_DIRECT
{
    public class UserPrivateKeyRepository : GenericRepository<tbl_UserPrivateKeys>
    {
        public UserPrivateKeyRepository(AuroraEntities context)
            : base(context) { }
    }
}
