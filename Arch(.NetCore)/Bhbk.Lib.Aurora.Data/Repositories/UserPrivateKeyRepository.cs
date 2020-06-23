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
    public class UserPrivateKeyRepository : GenericRepository<uvw_UserPrivateKeys>
    {
        public UserPrivateKeyRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_UserPrivateKeys Create(uvw_UserPrivateKeys entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("@PublicKeyId", SqlDbType.UniqueIdentifier) { Value = entity.PublicKeyId },
                new SqlParameter("@KeyValueBase64", SqlDbType.NVarChar) { Value = entity.KeyValueBase64 },
                new SqlParameter("@KeyValueAlgo", SqlDbType.NVarChar) { Value = entity.KeyValueAlgo },
                new SqlParameter("@KeyValuePass", SqlDbType.NVarChar) { Value = entity.KeyValuePass },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_UserPrivateKeys>().FromSqlRaw("[svc].[usp_UserPrivateKey_Insert]"
                + "@UserId, @PublicKeyId, @KeyValueBase64, @KeyValueAlgo, @KeyValuePass, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_UserPrivateKey_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var reader = cmd.ExecuteReader();

                return reader.Cast<uvw_UserPrivateKeys>().Single();
            }
            */
        }

        public override IEnumerable<uvw_UserPrivateKeys> Create(IEnumerable<uvw_UserPrivateKeys> entities)
        {
            var results = new List<uvw_UserPrivateKeys>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_UserPrivateKeys Delete(uvw_UserPrivateKeys entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_UserPrivateKeys>().FromSqlRaw("[svc].[usp_UserPrivateKey_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_UserPrivateKeys> Delete(IEnumerable<uvw_UserPrivateKeys> entities)
        {
            var results = new List<uvw_UserPrivateKeys>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_UserPrivateKeys> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_UserPrivateKeys Update(uvw_UserPrivateKeys entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("@PublicKeyId", SqlDbType.UniqueIdentifier) { Value = entity.PublicKeyId },
                new SqlParameter("@KeyValueBase64", SqlDbType.NVarChar) { Value = entity.KeyValueBase64 },
                new SqlParameter("@KeyValueAlgo", SqlDbType.NVarChar) { Value = entity.KeyValueAlgo },
                new SqlParameter("@KeyValuePass", SqlDbType.NVarChar) { Value = entity.KeyValuePass },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_UserPrivateKeys>().FromSqlRaw("[svc].[usp_UserPrivateKey_Insert]"
                + "@Id, @UserId, @PublicKeyId, @KeyValueBase64, @KeyValueAlgo, @KeyValuePass, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_UserPrivateKeys> Update(IEnumerable<uvw_UserPrivateKeys> entities)
        {
            var results = new List<uvw_UserPrivateKeys>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
