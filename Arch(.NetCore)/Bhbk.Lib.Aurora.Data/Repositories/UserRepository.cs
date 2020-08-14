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
    public class UserRepository : GenericRepository<uvw_Users>
    {
        public UserRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_Users Create(uvw_Users entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Username", SqlDbType.NVarChar) { Value = entity.UserName },
                new SqlParameter("@AllowPassword", SqlDbType.Bit) { Value = entity.AllowPassword },
                new SqlParameter("@FileSystemType", SqlDbType.NVarChar) { Value = entity.FileSystemType },
                new SqlParameter("@FileSystemReadOnly", SqlDbType.Bit) { Value = entity.FileSystemReadOnly },
                new SqlParameter("@DebugLevel", SqlDbType.NVarChar) { Value = (object)entity.DebugLevel ?? DBNull.Value },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_Users>().FromSqlRaw("[svc].[usp_User_Insert]"
                + "@Username, @AllowPassword, @FileSystemType, @FileSystemReadOnly, @DebugLevel, @Enabled, @Immutable", pvalues.ToArray())
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

        public override IEnumerable<uvw_Users> Create(IEnumerable<uvw_Users> entities)
        {
            var results = new List<uvw_Users>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Users Delete(uvw_Users entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Users>().FromSqlRaw("[svc].[usp_UserFolder_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Users> Delete(IEnumerable<uvw_Users> entities)
        {
            var results = new List<uvw_Users>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Users> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Users Update(uvw_Users entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@Username", SqlDbType.NVarChar) { Value = entity.UserName },
                new SqlParameter("@AllowPassword", SqlDbType.Bit) { Value = entity.AllowPassword },
                new SqlParameter("@FileSystemType", SqlDbType.NVarChar) { Value = entity.FileSystemType },
                new SqlParameter("@FileSystemReadOnly", SqlDbType.Bit) { Value = entity.FileSystemReadOnly },
                new SqlParameter("@DebugLevel", SqlDbType.NVarChar) { Value = (object)entity.DebugLevel ?? DBNull.Value },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_Users>().FromSqlRaw("[svc].[usp_User_Update]"
                + "@Id, @IdentityId, @Username, @AllowPassword, @FileSystemType, @FileSystemReadOnly, @DebugLevel, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Users> Update(IEnumerable<uvw_Users> entities)
        {
            var results = new List<uvw_Users>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
