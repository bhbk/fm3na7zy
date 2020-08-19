using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class UserFolderRepository : GenericRepository<tbl_UserFolder>
    {
        public UserFolderRepository(AuroraEntities context)
            : base(context) { }
    }
}
