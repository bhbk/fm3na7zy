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
    public class UserMountRepository : GenericRepository<uvw_UserMounts>
    {
        public UserMountRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_UserMounts Create(uvw_UserMounts entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("@CredentialId", SqlDbType.UniqueIdentifier) { Value = entity.CredentialId },
                new SqlParameter("@AuthType", SqlDbType.NVarChar) { Value = entity.AuthType },
                new SqlParameter("@ServerName", SqlDbType.NVarChar) { Value = entity.ServerAddress },
                new SqlParameter("@ServerPath", SqlDbType.NVarChar) { Value = entity.ServerShare },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_UserMounts>().FromSqlRaw("[svc].[usp_UserMount_Insert]"
                + "@UserId, @CredentialId, @AuthType, @ServerName, @ServerPath, @Enabled, @Immutable", pvalues.ToArray())
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

        public override IEnumerable<uvw_UserMounts> Create(IEnumerable<uvw_UserMounts> entities)
        {
            var results = new List<uvw_UserMounts>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_UserMounts Delete(uvw_UserMounts entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId }
            };

            return _context.Set<uvw_UserMounts>().FromSqlRaw("[svc].[usp_UserMount_Delete] @UserId", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_UserMounts> Delete(IEnumerable<uvw_UserMounts> entities)
        {
            var results = new List<uvw_UserMounts>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_UserMounts> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_UserMounts Update(uvw_UserMounts entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("@CredentialId", SqlDbType.UniqueIdentifier) { Value = entity.CredentialId },
                new SqlParameter("@AuthType", SqlDbType.NVarChar) { Value = entity.AuthType },
                new SqlParameter("@ServerName", SqlDbType.NVarChar) { Value = entity.ServerAddress },
                new SqlParameter("@ServerPath", SqlDbType.NVarChar) { Value = entity.ServerShare },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_UserMounts>().FromSqlRaw("[svc].[usp_UserMount_Update]"
                + "@UserId, @CredentialId, @AuthType, @ServerName, @ServerPath, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_UserMounts> Update(IEnumerable<uvw_UserMounts> entities)
        {
            var results = new List<uvw_UserMounts>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
