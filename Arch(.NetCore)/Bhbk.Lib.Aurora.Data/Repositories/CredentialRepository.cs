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
    public class CredentialRepository : GenericRepository<uvw_Credential>
    {
        public CredentialRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_Credential Create(uvw_Credential entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Domain", SqlDbType.NVarChar) { Value = (object)entity.Domain ?? DBNull.Value },
                new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = entity.UserName },
                new SqlParameter("@Password", SqlDbType.NVarChar) { Value = entity.Password },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Deletable", SqlDbType.Bit) { Value = entity.Deletable },
            };

            return _context.Set<uvw_Credential>().FromSqlRaw("[svc].[usp_Credential_Insert]"
                + "@Domain, @UserName, @Password, @Enabled, @Deletable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_Credential_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var reader = cmd.ExecuteReader();

                return reader.Cast<uvw_Credentials>().Single();
            }
            */
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
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Credential>().FromSqlRaw("[svc].[usp_Credential_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
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
            throw new NotImplementedException();
        }

        public override uvw_Credential Update(uvw_Credential entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@Domain", SqlDbType.NVarChar) { Value = (object)entity.Domain ?? DBNull.Value },
                new SqlParameter("@UserName", SqlDbType.NVarChar) { Value = entity.UserName },
                new SqlParameter("@Password", SqlDbType.NVarChar) { Value = entity.Password },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Deletable", SqlDbType.Bit) { Value = entity.Deletable }
            };

            return _context.Set<uvw_Credential>().FromSqlRaw("[svc].[usp_Credential_Update]"
                + "@Id, @Domain, @UserName, @Password, @Enabled, @Deletable", pvalues.ToArray())
                    .AsEnumerable().Single();
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
