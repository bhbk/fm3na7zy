using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class NetworkRepository : GenericRepository<tbl_Network>
    {
        public NetworkRepository(AuroraEntities context)
            : base(context) { }
    }
}
