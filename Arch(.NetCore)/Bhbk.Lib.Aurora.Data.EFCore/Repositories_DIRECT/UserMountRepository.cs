using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.EFCore.Repositories_DIRECT
{
    public class UserMountRepository : GenericRepository<tbl_UserMounts>
    {
        public UserMountRepository(AuroraEntities context)
            : base(context) { }
    }
}
