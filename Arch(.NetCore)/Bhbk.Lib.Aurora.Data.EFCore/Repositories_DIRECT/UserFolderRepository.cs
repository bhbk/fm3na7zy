using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.EFCore.Repositories_DIRECT
{
    public class UserFolderRepository : GenericRepository<tbl_UserFolders>
    {
        public UserFolderRepository(AuroraEntities context)
            : base(context) { }
    }
}
