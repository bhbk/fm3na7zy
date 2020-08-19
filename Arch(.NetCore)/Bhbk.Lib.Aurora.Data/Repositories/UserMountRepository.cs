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
    public class UserMountRepository : GenericRepository<uvw_UserMount>
    {
        public UserMountRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_UserMount Create(uvw_UserMount entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@CredentialId", SqlDbType.UniqueIdentifier) { Value = entity.CredentialId },
                new SqlParameter("@AuthType", SqlDbType.NVarChar) { Value = entity.AuthType },
                new SqlParameter("@ServerName", SqlDbType.NVarChar) { Value = entity.ServerAddress },
                new SqlParameter("@ServerPath", SqlDbType.NVarChar) { Value = entity.ServerShare },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Deletable", SqlDbType.Bit) { Value = entity.Deletable }
            };

            return _context.Set<uvw_UserMount>().FromSqlRaw("[svc].[usp_UserMount_Insert]"
                + "@IdentityId, @CredentialId, @AuthType, @ServerName, @ServerPath, @Enabled, @Deletable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_UserFolder_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var reader = cmd.ExecuteReader();

                return reader.Cast<uvw_UserMounts>().Single();
            }
            */
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
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId }
            };

            return _context.Set<uvw_UserMount>().FromSqlRaw("[svc].[usp_UserMount_Delete] @IdentityId", pvalues.ToArray())
                .AsEnumerable().Single();
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
            throw new NotImplementedException();
        }

        public override uvw_UserMount Update(uvw_UserMount entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@CredentialId", SqlDbType.UniqueIdentifier) { Value = entity.CredentialId },
                new SqlParameter("@AuthType", SqlDbType.NVarChar) { Value = entity.AuthType },
                new SqlParameter("@ServerName", SqlDbType.NVarChar) { Value = entity.ServerAddress },
                new SqlParameter("@ServerPath", SqlDbType.NVarChar) { Value = entity.ServerShare },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Deletable", SqlDbType.Bit) { Value = entity.Deletable }
            };

            return _context.Set<uvw_UserMount>().FromSqlRaw("[svc].[usp_UserMount_Update]"
                + "@IdentityId, @CredentialId, @AuthType, @ServerName, @ServerPath, @Enabled, @Deletable", pvalues.ToArray())
                    .AsEnumerable().Single();
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
