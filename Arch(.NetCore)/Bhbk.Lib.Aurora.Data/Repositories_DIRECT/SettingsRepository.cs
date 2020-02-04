using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class SettingsRepository : GenericRepository<tbl_Settings>
    {
        public SettingsRepository(AuroraEntities context)
            : base(context) { }
    }
}
