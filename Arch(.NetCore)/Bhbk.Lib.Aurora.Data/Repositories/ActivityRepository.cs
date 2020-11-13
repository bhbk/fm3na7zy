using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Bhbk.Lib.Aurora.Data.Models;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Aurora.Data.Repositories
{
    public class ActivityRepository : GenericRepository<uvw_Activity>
    {
        public ActivityRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_Activity Update(uvw_Activity entity)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<uvw_Activity> Update(IEnumerable<uvw_Activity> entities)
        {
            throw new NotImplementedException();
        }
    }
}
