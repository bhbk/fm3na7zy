using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class SysCredentialRepository : GenericRepository<tbl_SysCredentials>
    {
        public SysCredentialRepository(AuroraEntities context)
            : base(context) { }
    }
}
