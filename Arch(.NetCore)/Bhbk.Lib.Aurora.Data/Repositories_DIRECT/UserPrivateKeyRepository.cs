using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class UserPrivateKeyRepository : GenericRepository<tbl_UserPrivateKeys>
    {
        public UserPrivateKeyRepository(AuroraEntities context)
            : base(context) { }
    }
}
