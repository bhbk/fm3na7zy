using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class UserMountRepository : GenericRepository<tbl_UserMount>
    {
        public UserMountRepository(AuroraEntities context)
            : base(context) { }
    }
}
