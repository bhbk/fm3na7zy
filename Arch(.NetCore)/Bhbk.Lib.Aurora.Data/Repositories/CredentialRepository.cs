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
    public class CredentialRepository : GenericRepository<uvw_Credential>
    {
        public CredentialRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_Credential Create(uvw_Credential entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("@Domain", SqlDbType.NVarChar) { Value = (object)entity.Domain ?? DBNull.Value },
                new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = entity.UserName },
                new SqlParameter("@Password", SqlDbType.NVarChar) { Value = entity.Password },
                new SqlParameter("@IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("@IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_Credential>("EXEC @ReturnValue = [svc].[usp_Credential_Insert] "
                + "@Domain, @UserName, @Password, @IsEnabled, @IsDeletable", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_Credential> Create(IEnumerable<uvw_Credential> entities)
        {
            var results = new List<uvw_Credential>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Credential Delete(uvw_Credential entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue,
            };

            return _context.SqlQuery<uvw_Credential>("EXEC @ReturnValue = [svc].[usp_Credential_Delete] @Id", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_Credential> Delete(IEnumerable<uvw_Credential> entities)
        {
            var results = new List<uvw_Credential>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Credential> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Credential>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Credential Update(uvw_Credential entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@Domain", SqlDbType.NVarChar) { Value = (object)entity.Domain ?? DBNull.Value },
                new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = entity.UserName },
                new SqlParameter("@Password", SqlDbType.NVarChar) { Value = entity.Password },
                new SqlParameter("@IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("@IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_Credential>("EXEC @ReturnValue = [svc].[usp_Credential_Update] "
                + "@Id, @Domain, @UserName, @Password, @IsEnabled, @IsDeletable", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_Credential> Update(IEnumerable<uvw_Credential> entities)
        {
            var results = new List<uvw_Credential>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
