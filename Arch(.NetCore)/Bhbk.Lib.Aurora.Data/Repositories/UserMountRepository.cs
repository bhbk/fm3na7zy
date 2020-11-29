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
    public class UserMountRepository : GenericRepository<uvw_UserMount>
    {
        public UserMountRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_UserMount Create(uvw_UserMount entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("CredentialId", SqlDbType.UniqueIdentifier) { Value = entity.CredentialId },
                new SqlParameter("AuthType", SqlDbType.NVarChar) { Value = entity.AuthType },
                new SqlParameter("ServerName", SqlDbType.NVarChar) { Value = entity.ServerAddress },
                new SqlParameter("ServerPath", SqlDbType.NVarChar) { Value = entity.ServerShare },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_UserMount>("EXEC @ReturnValue = [svc].[usp_UserMount_Insert] "
                + "@IdentityId, @CredentialId, @AuthType, @ServerName, @ServerPath, @IsEnabled, @IsDeletable", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_UserMount> Create(IEnumerable<uvw_UserMount> entities)
        {
            var results = new List<uvw_UserMount>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_UserMount Delete(uvw_UserMount entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                rvalue,
            };

            return _context.SqlQuery<uvw_UserMount>("EXEC @ReturnValue = [svc].[usp_UserMount_Delete] @IdentityId", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_UserMount> Delete(IEnumerable<uvw_UserMount> entities)
        {
            var results = new List<uvw_UserMount>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_UserMount> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_UserMount>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_UserMount Update(uvw_UserMount entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("CredentialId", SqlDbType.UniqueIdentifier) { Value = entity.CredentialId },
                new SqlParameter("AuthType", SqlDbType.NVarChar) { Value = entity.AuthType },
                new SqlParameter("ServerName", SqlDbType.NVarChar) { Value = entity.ServerAddress },
                new SqlParameter("ServerPath", SqlDbType.NVarChar) { Value = entity.ServerShare },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_UserMount>("EXEC @ReturnValue = [svc].[usp_UserMount_Update] "
                + "@IdentityId, @CredentialId, @AuthType, @ServerName, @ServerPath, @IsEnabled, @IsDeletable", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_UserMount> Update(IEnumerable<uvw_UserMount> entities)
        {
            var results = new List<uvw_UserMount>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
