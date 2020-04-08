using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.EFCore.Repositories_DIRECT
{
    public class UserFileRepository : GenericRepository<tbl_UserFiles>
    {
        public UserFileRepository(AuroraEntities context)
            : base(context) { }
    }
}
