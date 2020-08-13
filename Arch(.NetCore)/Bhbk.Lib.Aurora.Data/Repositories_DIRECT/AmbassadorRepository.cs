using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class AmbassadorRepository : GenericRepository<tbl_Ambassadors>
    {
        public AmbassadorRepository(AuroraEntities context)
            : base(context) { }
    }
}
