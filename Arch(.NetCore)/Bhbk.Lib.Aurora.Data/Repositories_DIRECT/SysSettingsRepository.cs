using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class SysSettingRepository : GenericRepository<tbl_SysSettings>
    {
        public SysSettingRepository(AuroraEntities context)
            : base(context) { }
    }
}
