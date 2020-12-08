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
    public class MountRepository : GenericRepository<uvw_Mount>
    {
        public MountRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_Mount Create(uvw_Mount entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("AmbassadorId", SqlDbType.UniqueIdentifier) { Value = entity.AmbassadorId },
                new SqlParameter("AuthType", SqlDbType.NVarChar) { Value = entity.AuthType },
                new SqlParameter("ServerName", SqlDbType.NVarChar) { Value = entity.ServerAddress },
                new SqlParameter("ServerPath", SqlDbType.NVarChar) { Value = entity.ServerShare },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_Mount>("EXEC @ReturnValue = [svc].[usp_Mount_Insert] "
                + "@UserId, @AmbassadorId, @AuthType, @ServerName, @ServerPath, @IsEnabled, @IsDeletable", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_Mount> Create(IEnumerable<uvw_Mount> entities)
        {
            var results = new List<uvw_Mount>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Mount Delete(uvw_Mount entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                rvalue,
            };

            return _context.SqlQuery<uvw_Mount>("EXEC @ReturnValue = [svc].[usp_Mount_Delete] @UserId", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_Mount> Delete(IEnumerable<uvw_Mount> entities)
        {
            var results = new List<uvw_Mount>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Mount> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Mount>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Mount Update(uvw_Mount entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("AmbassadorId", SqlDbType.UniqueIdentifier) { Value = entity.AmbassadorId },
                new SqlParameter("AuthType", SqlDbType.NVarChar) { Value = entity.AuthType },
                new SqlParameter("ServerName", SqlDbType.NVarChar) { Value = entity.ServerAddress },
                new SqlParameter("ServerPath", SqlDbType.NVarChar) { Value = entity.ServerShare },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_Mount>("EXEC @ReturnValue = [svc].[usp_Mount_Update] "
                + "@UserId, @AmbassadorId, @AuthType, @ServerName, @ServerPath, @IsEnabled, @IsDeletable", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_Mount> Update(IEnumerable<uvw_Mount> entities)
        {
            var results = new List<uvw_Mount>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
