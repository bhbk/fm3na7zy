using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.EFCore.Repositories_DIRECT
{
    public class SystemKeyRepository : GenericRepository<tbl_SystemKeys>
    {
        public SystemKeyRepository(AuroraEntities context)
            : base(context) { }
    }
}
