using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class SysPrivateKeyRepository : GenericRepository<tbl_SysPrivateKeys>
    {
        public SysPrivateKeyRepository(AuroraEntities context)
            : base(context) { }
    }
}
