﻿using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.DataAccess.EFCore.Repositories;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class UserFileRepository : GenericRepository<tbl_UserFile>
    {
        public UserFileRepository(AuroraEntities context)
            : base(context) { }
    }
}
