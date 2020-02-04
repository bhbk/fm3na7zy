using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class UserRepository : GenericRepository<tbl_Users>
    {
        public UserRepository(AuroraEntities context)
            : base(context) { }
    }
}
