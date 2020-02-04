using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class PrivateKeyRepository : GenericRepository<tbl_PrivateKeys>
    {
        public PrivateKeyRepository(AuroraEntities context)
            : base(context) { }
    }
}
