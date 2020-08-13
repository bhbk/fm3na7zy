using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class SettingRepository : GenericRepository<tbl_Settings>
    {
        public SettingRepository(AuroraEntities context)
            : base(context) { }
    }
}
