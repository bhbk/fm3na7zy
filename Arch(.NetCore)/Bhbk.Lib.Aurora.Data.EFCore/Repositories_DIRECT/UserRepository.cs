using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.EFCore.Repositories_DIRECT
{
    public class UserRepository : GenericRepository<tbl_Users>
    {
        public UserRepository(AuroraEntities context)
            : base(context) { }
    }
}
