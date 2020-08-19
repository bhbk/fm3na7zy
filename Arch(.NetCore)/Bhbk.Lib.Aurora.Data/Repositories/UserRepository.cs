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
    public class UserRepository : GenericRepository<uvw_User>
    {
        public UserRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_User Create(uvw_User entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IdentityAlias", SqlDbType.NVarChar) { Value = entity.IdentityAlias },
                new SqlParameter("@RequirePublicKey", SqlDbType.Bit) { Value = entity.RequirePublicKey },
                new SqlParameter("@RequirePassword", SqlDbType.Bit) { Value = entity.RequirePassword },
                new SqlParameter("@FileSystemType", SqlDbType.NVarChar) { Value = entity.FileSystemType },
                new SqlParameter("@FileSystemReadOnly", SqlDbType.Bit) { Value = entity.FileSystemReadOnly },
                new SqlParameter("@DebugLevel", SqlDbType.NVarChar) { Value = (object)entity.DebugLevel ?? DBNull.Value },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Deletable", SqlDbType.Bit) { Value = entity.Deletable },
            };

            return _context.Set<uvw_User>().FromSqlRaw("[svc].[usp_User_Insert]"
                + "@IdentityAlias, @RequirePublicKey, @RequirePassword, @FileSystemType, @FileSystemReadOnly, @DebugLevel, @Enabled, @Deletable", pvalues.ToArray())
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

                return reader.Cast<uvw_Users>().Single();
            }
            */
        }

        public override IEnumerable<uvw_User> Create(IEnumerable<uvw_User> entities)
        {
            var results = new List<uvw_User>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_User Delete(uvw_User entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId }
            };

            return _context.Set<uvw_User>().FromSqlRaw("[svc].[usp_UserFolder_Delete] @IdentityId", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_User> Delete(IEnumerable<uvw_User> entities)
        {
            var results = new List<uvw_User>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_User> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_User Update(uvw_User entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@IdentityAlias", SqlDbType.NVarChar) { Value = entity.IdentityAlias },
                new SqlParameter("@RequirePublicKey", SqlDbType.Bit) { Value = entity.RequirePublicKey },
                new SqlParameter("@RequirePassword", SqlDbType.Bit) { Value = entity.RequirePassword },
                new SqlParameter("@FileSystemType", SqlDbType.NVarChar) { Value = entity.FileSystemType },
                new SqlParameter("@FileSystemReadOnly", SqlDbType.Bit) { Value = entity.FileSystemReadOnly },
                new SqlParameter("@DebugLevel", SqlDbType.NVarChar) { Value = (object)entity.DebugLevel ?? DBNull.Value },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Deletable", SqlDbType.Bit) { Value = entity.Deletable }
            };

            return _context.Set<uvw_User>().FromSqlRaw("[svc].[usp_User_Update]"
                + "@IdentityId, @IdentityId, @IdentityAlias, @RequirePublicKey, @RequirePassword, @FileSystemType, @FileSystemReadOnly, @DebugLevel, @Enabled, @Deletable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_User> Update(IEnumerable<uvw_User> entities)
        {
            var results = new List<uvw_User>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
