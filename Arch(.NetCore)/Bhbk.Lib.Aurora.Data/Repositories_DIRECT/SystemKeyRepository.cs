using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class SystemKeyRepository : GenericRepository<tbl_SystemKeys>
    {
        public SystemKeyRepository(AuroraEntities context)
            : base(context) { }
    }
}
