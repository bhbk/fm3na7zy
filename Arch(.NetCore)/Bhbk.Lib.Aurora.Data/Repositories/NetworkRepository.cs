using Bhbk.Lib.Aurora.Data.Models;
using Bhbk.Lib.DataAccess.EFCore.Extensions;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Aurora.Data.Repositories
{
    public class NetworkRepository : GenericRepository<uvw_Network>
    {
        public NetworkRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_Network Create(uvw_Network entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("Address", SqlDbType.NVarChar) { Value = entity.Address },
                new SqlParameter("Action", SqlDbType.NVarChar) { Value = entity.Action },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                rvalue,
            };

            return _context.SqlQuery<uvw_Network>("EXEC @ReturnValue = [svc].[usp_Network_Insert] "
                + "@UserId, @Address, @Action, @IsEnabled", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_Network> Create(IEnumerable<uvw_Network> entities)
        {
            var results = new List<uvw_Network>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Network Delete(uvw_Network entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue,
            };

            return _context.SqlQuery<uvw_Network>("EXEC @ReturnValue = [svc].[usp_Network_Delete] @Id", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_Network> Delete(IEnumerable<uvw_Network> entities)
        {
            var results = new List<uvw_Network>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Network> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Network>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Network Update(uvw_Network entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("Address", SqlDbType.NVarChar) { Value = entity.Address },
                new SqlParameter("Action", SqlDbType.NVarChar) { Value = entity.Action },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                rvalue,
            };

            return _context.SqlQuery<uvw_Network>("EXEC @ReturnValue = [svc].[usp_Network_Update] "
                + "@Id, @UserId, @Address, @Action, @IsEnabled", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_Network> Update(IEnumerable<uvw_Network> entities)
        {
            var results = new List<uvw_Network>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
