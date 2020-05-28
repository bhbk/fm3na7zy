﻿using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.EFCore.Repositories_DIRECT
{
    public class SysSettingRepository : GenericRepository<tbl_SysSettings>
    {
        public SysSettingRepository(AuroraEntities context)
            : base(context) { }
    }
}