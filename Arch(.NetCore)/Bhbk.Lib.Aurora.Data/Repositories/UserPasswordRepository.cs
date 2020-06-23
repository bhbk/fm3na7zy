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
    public class UserPasswordRepository : GenericRepository<uvw_UserPasswords>
    {
        public UserPasswordRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_UserPasswords Create(uvw_UserPasswords entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("@ConcurrencyStamp", SqlDbType.NVarChar) { Value = entity.ConcurrencyStamp },
                new SqlParameter("@PasswordHashPBKDF2", SqlDbType.NVarChar) { Value = (object)entity.PasswordHashPBKDF2 ?? DBNull.Value },
                new SqlParameter("@PasswordHashSHA256", SqlDbType.NVarChar) { Value = (object)entity.PasswordHashSHA256 ?? DBNull.Value },
                new SqlParameter("@SecurityStamp", SqlDbType.NVarChar) { Value = entity.SecurityStamp },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_UserPasswords>().FromSqlRaw("[svc].[usp_UserPassword_Insert]"
                + "@UserId, @ConcurrencyStamp, @PasswordHashPBKDF2, @PasswordHashSHA256, @SecurityStamp, @Enabled, @Immutable", pvalues.ToArray())
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

                return reader.Cast<uvw_UserPasswords>().Single();
            }
            */
        }

        public override IEnumerable<uvw_UserPasswords> Create(IEnumerable<uvw_UserPasswords> entities)
        {
            var results = new List<uvw_UserPasswords>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_UserPasswords Delete(uvw_UserPasswords entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId }
            };

            return _context.Set<uvw_UserPasswords>().FromSqlRaw("[svc].[usp_UserPassword_Delete] @UserId", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_UserPasswords> Delete(IEnumerable<uvw_UserPasswords> entities)
        {
            var results = new List<uvw_UserPasswords>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_UserPasswords> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_UserPasswords Update(uvw_UserPasswords entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("@ConcurrencyStamp", SqlDbType.NVarChar) { Value = entity.ConcurrencyStamp },
                new SqlParameter("@PasswordHashPBKDF2", SqlDbType.NVarChar) { Value = (object)entity.PasswordHashPBKDF2 ?? DBNull.Value },
                new SqlParameter("@PasswordHashSHA256", SqlDbType.NVarChar) { Value = (object)entity.PasswordHashSHA256 ?? DBNull.Value },
                new SqlParameter("@SecurityStamp", SqlDbType.NVarChar) { Value = entity.SecurityStamp },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_UserPasswords>().FromSqlRaw("[svc].[usp_UserPassword_Update]"
                + "@UserId, @ConcurrencyStamp, @PasswordHashPBKDF2, @PasswordHashSHA256, @SecurityStamp, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_UserPasswords> Update(IEnumerable<uvw_UserPasswords> entities)
        {
            var results = new List<uvw_UserPasswords>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
