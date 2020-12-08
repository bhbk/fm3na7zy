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
    public class AlertRepository : GenericRepository<uvw_Alert>
    {
        public AlertRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_Alert Create(uvw_Alert entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("OnDelete", SqlDbType.Bit) { Value = entity.OnDelete },
                new SqlParameter("OnDownload", SqlDbType.Bit) { Value = entity.OnDownload },
                new SqlParameter("OnUpload", SqlDbType.Bit) { Value = entity.OnUpload },
                new SqlParameter("ToDisplayName", SqlDbType.NVarChar) { Value = entity.ToDisplayName },
                new SqlParameter("ToEmailAddress", SqlDbType.NVarChar) { Value = (object)entity.ToEmailAddress ?? DBNull.Value },
                new SqlParameter("ToPhoneNumber", SqlDbType.NVarChar) { Value = (object)entity.ToPhoneNumber ?? DBNull.Value },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                rvalue,
            };

            return _context.SqlQuery<uvw_Alert>("EXEC @ReturnValue = [svc].[usp_Alert_Insert] "
                + "@UserId, @OnDelete, @OnDownload, @OnUpload, @ToDisplayName, @ToEmailAddress, @ToPhoneNumber, @IsEnabled", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_Alert> Create(IEnumerable<uvw_Alert> entities)
        {
            var results = new List<uvw_Alert>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Alert Delete(uvw_Alert entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue,
            };

            return _context.SqlQuery<uvw_Alert>("EXEC @ReturnValue = [svc].[usp_Alert_Delete] @Id", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_Alert> Delete(IEnumerable<uvw_Alert> entities)
        {
            var results = new List<uvw_Alert>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Alert> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Alert>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Alert Update(uvw_Alert entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("OnDelete", SqlDbType.Bit) { Value = entity.OnDelete },
                new SqlParameter("OnDownload", SqlDbType.Bit) { Value = entity.OnDownload },
                new SqlParameter("OnUpload", SqlDbType.Bit) { Value = entity.OnUpload },
                new SqlParameter("ToDisplayName", SqlDbType.NVarChar) { Value = entity.ToDisplayName },
                new SqlParameter("ToEmailAddress", SqlDbType.NVarChar) { Value = (object)entity.ToEmailAddress ?? DBNull.Value },
                new SqlParameter("ToPhoneNumber", SqlDbType.NVarChar) { Value = (object)entity.ToPhoneNumber ?? DBNull.Value },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                rvalue,
            };

            return _context.SqlQuery<uvw_Alert>("EXEC @ReturnValue = [svc].[usp_Alert_Update] "
                + "@Id, @UserId, @OnDelete, @OnDownload, @OnUpload, @ToDisplayName, @ToEmailAddress, @ToPhoneNumber, @IsEnabled", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_Alert> Update(IEnumerable<uvw_Alert> entities)
        {
            var results = new List<uvw_Alert>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
