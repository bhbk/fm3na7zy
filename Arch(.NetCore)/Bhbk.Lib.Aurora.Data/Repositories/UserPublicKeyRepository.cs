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
    public class UserPublicKeyRepository : GenericRepository<uvw_UserPublicKeys>
    {
        public UserPublicKeyRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_UserPublicKeys Create(uvw_UserPublicKeys entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("@PrivateKeyId", SqlDbType.UniqueIdentifier) { Value = entity.PrivateKeyId.HasValue ? (object)entity.PrivateKeyId.Value : DBNull.Value },
                new SqlParameter("@KeyValueBase64", SqlDbType.NVarChar) { Value = entity.KeyValueBase64 },
                new SqlParameter("@KeyValueAlgo", SqlDbType.NVarChar) { Value = entity.KeyValueAlgo },
                new SqlParameter("@KeySig", SqlDbType.NVarChar) { Value = entity.KeySig },
                new SqlParameter("@KeySigAlgo", SqlDbType.NVarChar) { Value = entity.KeySigAlgo },
                new SqlParameter("@Hostname", SqlDbType.NVarChar) { Value = entity.Hostname },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_UserPublicKeys>().FromSqlRaw("[svc].[usp_UserPublicKey_Insert]"
                + "@UserId, @PrivateKeyId, @KeyValueBase64, @KeyValueAlgo, @KeySig, @KeySigAlgo, @Hostname, @Enabled, @Immutable", pvalues.ToArray())
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

                return reader.Cast<uvw_UserPublicKeys>().Single();
            }
            */
        }

        public override IEnumerable<uvw_UserPublicKeys> Create(IEnumerable<uvw_UserPublicKeys> entities)
        {
            var results = new List<uvw_UserPublicKeys>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_UserPublicKeys Delete(uvw_UserPublicKeys entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_UserPublicKeys>().FromSqlRaw("[svc].[usp_UserPublicKey_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_UserPublicKeys> Delete(IEnumerable<uvw_UserPublicKeys> entities)
        {
            var results = new List<uvw_UserPublicKeys>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_UserPublicKeys> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_UserPublicKeys Update(uvw_UserPublicKeys entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("@PrivateKeyId", SqlDbType.UniqueIdentifier) { Value = entity.PrivateKeyId.HasValue ? (object)entity.PrivateKeyId.Value : DBNull.Value },
                new SqlParameter("@KeyValueBase64", SqlDbType.NVarChar) { Value = entity.KeyValueBase64 },
                new SqlParameter("@KeyValueAlgo", SqlDbType.NVarChar) { Value = entity.KeyValueAlgo },
                new SqlParameter("@KeySig", SqlDbType.NVarChar) { Value = entity.KeySig },
                new SqlParameter("@KeySigAlgo", SqlDbType.NVarChar) { Value = entity.KeySigAlgo },
                new SqlParameter("@Hostname", SqlDbType.NVarChar) { Value = entity.Hostname },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_UserPublicKeys>().FromSqlRaw("[svc].[usp_UserPublicKey_Update]"
                + "@Id, @UserId, @PrivateKeyId, @KeyValueBase64, @KeyValueAlgo, @KeySig, @KeySigAlgo, @Hostname, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_UserPublicKeys> Update(IEnumerable<uvw_UserPublicKeys> entities)
        {
            var results = new List<uvw_UserPublicKeys>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
