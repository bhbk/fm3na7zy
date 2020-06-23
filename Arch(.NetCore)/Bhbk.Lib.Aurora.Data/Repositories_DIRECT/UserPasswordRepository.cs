using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class UserPasswordRepository : GenericRepository<tbl_UserPasswords>
    {
        public UserPasswordRepository(AuroraEntities context)
            : base(context) { }
    }
}
