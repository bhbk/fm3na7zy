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
    public class SessionRepository : GenericRepository<uvw_Session>
    {
        public SessionRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_Session Create(uvw_Session entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId.HasValue ? (object)entity.IdentityId.Value : DBNull.Value },
                new SqlParameter("CallPath", SqlDbType.NVarChar) { Value = entity.CallPath },
                new SqlParameter("Details", SqlDbType.NVarChar) { Value = (object)entity.Details ?? DBNull.Value },
                new SqlParameter("LocalEndPoint", SqlDbType.NVarChar) { Value = (object)entity.LocalEndPoint ?? DBNull.Value },
                new SqlParameter("LocalSoftwareIdentifier", SqlDbType.NVarChar) { Value = (object)entity.LocalSoftwareIdentifier ?? DBNull.Value },
                new SqlParameter("RemoteEndPoint", SqlDbType.NVarChar) { Value = (object)entity.RemoteEndPoint ?? DBNull.Value },
                new SqlParameter("RemoteSoftwareIdentifier", SqlDbType.NVarChar) { Value = (object)entity.RemoteSoftwareIdentifier ?? DBNull.Value },
                rvalue,
            };

            return _context.SqlQuery<uvw_Session>("EXEC @ReturnValue = [svc].[usp_Session_Insert] "
                + "@IdentityId, @CallPath, @Details, @LocalEndPoint, @LocalSoftwareIdentifier, @RemoteEndPoint, @RemoteSoftwareIdentifier", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_Session> Create(IEnumerable<uvw_Session> entities)
        {
            var results = new List<uvw_Session>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Session Delete(uvw_Session entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue,
            };

            return _context.SqlQuery<uvw_Session>("EXEC @ReturnValue = [svc].[usp_Session_Delete] @Id", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_Session> Delete(IEnumerable<uvw_Session> entities)
        {
            var results = new List<uvw_Session>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Session> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Session>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Session Update(uvw_Session entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId.HasValue ? (object)entity.IdentityId.Value : DBNull.Value },
                new SqlParameter("CallPath", SqlDbType.NVarChar) { Value = entity.CallPath },
                new SqlParameter("Details", SqlDbType.NVarChar) { Value = (object)entity.Details ?? DBNull.Value },
                new SqlParameter("LocalEndPoint", SqlDbType.NVarChar) { Value = (object)entity.LocalEndPoint ?? DBNull.Value },
                new SqlParameter("LocalSoftwareIdentifier", SqlDbType.NVarChar) { Value = (object)entity.LocalSoftwareIdentifier ?? DBNull.Value },
                new SqlParameter("RemoteEndPoint", SqlDbType.NVarChar) { Value = (object)entity.RemoteEndPoint ?? DBNull.Value },
                new SqlParameter("RemoteSoftwareIdentifier", SqlDbType.NVarChar) { Value = (object)entity.RemoteSoftwareIdentifier ?? DBNull.Value },
                rvalue,
            };

            return _context.SqlQuery<uvw_Session>("EXEC @ReturnValue = [svc].[usp_Session_Update] "
                + "@Id, @IdentityId, @CallPath, @Details, @LocalEndPoint, @LocalSoftwareIdentifier, @RemoteEndPoint, @RemoteSoftwareIdentifier", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_Session> Update(IEnumerable<uvw_Session> entities)
        {
            var results = new List<uvw_Session>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
