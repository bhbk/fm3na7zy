using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.EFCore.Repositories_DIRECT
{
    public class SysCredentialRepository : GenericRepository<tbl_SysCredentials>
    {
        public SysCredentialRepository(AuroraEntities context)
            : base(context) { }
    }
}
