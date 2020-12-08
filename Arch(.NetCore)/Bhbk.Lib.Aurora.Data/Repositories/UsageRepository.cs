using Bhbk.Lib.Aurora.Data.Models;
using Bhbk.Lib.DataAccess.EFCore.Extensions;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Aurora.Data.Repositories
{
    public class UsageRepository : GenericRepository<uvw_Usage>
    {
        public UsageRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_Usage Create(uvw_Usage entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("QuotaInBytes", SqlDbType.BigInt) { Value = entity.QuotaInBytes },
                new SqlParameter("QuotaUsedInBytes", SqlDbType.BigInt) { Value = entity.QuotaUsedInBytes },
                new SqlParameter("SessionMax", SqlDbType.SmallInt) { Value = entity.SessionMax },
                new SqlParameter("SessionsInUse", SqlDbType.SmallInt) { Value = entity.SessionsInUse },
                rvalue,
            };

            return _context.SqlQuery<uvw_Usage>("EXEC @ReturnValue = [svc].[usp_Usage_Insert] "
                + "@UserId, @QuotaInBytes, @QuotaUsedInBytes, @SessionMax, @SessionsInUse", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_Usage> Create(IEnumerable<uvw_Usage> entities)
        {
            var results = new List<uvw_Usage>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Usage Delete(uvw_Usage entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                rvalue,
            };

            return _context.SqlQuery<uvw_Usage>("EXEC @ReturnValue = [svc].[usp_Usage_Delete] @UserId", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_Usage> Delete(IEnumerable<uvw_Usage> entities)
        {
            var results = new List<uvw_Usage>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Usage> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Usage>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Usage Update(uvw_Usage entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("QuotaInBytes", SqlDbType.BigInt) { Value = entity.QuotaInBytes },
                new SqlParameter("QuotaUsedInBytes", SqlDbType.BigInt) { Value = entity.QuotaUsedInBytes },
                new SqlParameter("SessionMax", SqlDbType.SmallInt) { Value = entity.SessionMax },
                new SqlParameter("SessionsInUse", SqlDbType.SmallInt) { Value = entity.SessionsInUse },
                rvalue,
            };

            return _context.SqlQuery<uvw_Usage>("EXEC @ReturnValue = [svc].[usp_Usage_Update] "
                + "@UserId, @QuotaInBytes, @QuotaUsedInBytes, @SessionMax, @SessionsInUse", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_Usage> Update(IEnumerable<uvw_Usage> entities)
        {
            var results = new List<uvw_Usage>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
