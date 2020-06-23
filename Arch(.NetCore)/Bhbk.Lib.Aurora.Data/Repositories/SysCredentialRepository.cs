using Bhbk.Lib.Aurora.Data.Models;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Lib.Aurora.Data.Repositories
{
    public class SysCredentialRepository : GenericRepository<uvw_SysCredentials>
    {
        public SysCredentialRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_SysCredentials Create(uvw_SysCredentials entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Domain", SqlDbType.NVarChar) { Value = (object)entity.Domain ?? DBNull.Value },
                new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = entity.UserName },
                new SqlParameter("@Password", SqlDbType.NVarChar) { Value = entity.Password },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_SysCredentials>().FromSqlRaw("[svc].[usp_SysCredential_Insert]"
                + "@Domain, @UserName, @Password, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_SysCredential_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var reader = cmd.ExecuteReader();

                return reader.Cast<uvw_SysCredentials>().Single();
            }
            */
        }

        public override IEnumerable<uvw_SysCredentials> Create(IEnumerable<uvw_SysCredentials> entities)
        {
            var results = new List<uvw_SysCredentials>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_SysCredentials Delete(uvw_SysCredentials entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_SysCredentials>().FromSqlRaw("[svc].[usp_SysCredential_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_SysCredentials> Delete(IEnumerable<uvw_SysCredentials> entities)
        {
            var results = new List<uvw_SysCredentials>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_SysCredentials> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_SysCredentials Update(uvw_SysCredentials entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@Domain", SqlDbType.NVarChar) { Value = (object)entity.Domain ?? DBNull.Value },
                new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = entity.UserName },
                new SqlParameter("@Password", SqlDbType.NVarChar) { Value = entity.Password },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_SysCredentials>().FromSqlRaw("[svc].[usp_SysCredential_Update]"
                + "@Id, @Domain, @UserName, @Password, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_SysCredentials> Update(IEnumerable<uvw_SysCredentials> entities)
        {
            var results = new List<uvw_SysCredentials>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
