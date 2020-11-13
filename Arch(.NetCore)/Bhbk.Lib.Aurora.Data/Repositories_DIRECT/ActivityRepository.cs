using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Repositories_DIRECT
{
    public class ActivityRepository : GenericRepository<tbl_Activity>
    {
        public ActivityRepository(AuroraEntities context)
            : base(context) { }

        public override tbl_Activity Update(tbl_Activity entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<tbl_Activity> Update(IEnumerable<tbl_Activity> entities)
        {
            throw new NotImplementedException();
        }
    }
}
