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
    public class PublicKeyRepository : GenericRepository<uvw_PublicKeys>
    {
        public PublicKeyRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_PublicKeys Create(uvw_PublicKeys entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@PrivateKeyId", SqlDbType.UniqueIdentifier) { Value = entity.PrivateKeyId.HasValue ? (object)entity.PrivateKeyId.Value : DBNull.Value },
                new SqlParameter("@KeyValueBase64", SqlDbType.NVarChar) { Value = entity.KeyValue },
                new SqlParameter("@KeyValueAlgo", SqlDbType.NVarChar) { Value = entity.KeyAlgo },
                new SqlParameter("@KeySig", SqlDbType.NVarChar) { Value = entity.SigValue },
                new SqlParameter("@KeySigAlgo", SqlDbType.NVarChar) { Value = entity.SigAlgo },
                new SqlParameter("@Hostname", SqlDbType.NVarChar) { Value = entity.Comment },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_PublicKeys>().FromSqlRaw("[svc].[usp_PublicKey_Insert]"
                + "@IdentityId, @PrivateKeyId, @KeyValueBase64, @KeyValueAlgo, @KeySig, @KeySigAlgo, @Hostname, @Enabled, @Immutable", pvalues.ToArray())
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

                return reader.Cast<uvw_PublicKeys>().Single();
            }
            */
        }

        public override IEnumerable<uvw_PublicKeys> Create(IEnumerable<uvw_PublicKeys> entities)
        {
            var results = new List<uvw_PublicKeys>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_PublicKeys Delete(uvw_PublicKeys entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_PublicKeys>().FromSqlRaw("[svc].[usp_PublicKey_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_PublicKeys> Delete(IEnumerable<uvw_PublicKeys> entities)
        {
            var results = new List<uvw_PublicKeys>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_PublicKeys> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_PublicKeys Update(uvw_PublicKeys entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@PrivateKeyId", SqlDbType.UniqueIdentifier) { Value = entity.PrivateKeyId.HasValue ? (object)entity.PrivateKeyId.Value : DBNull.Value },
                new SqlParameter("@KeyValueBase64", SqlDbType.NVarChar) { Value = entity.KeyValue },
                new SqlParameter("@KeyValueAlgo", SqlDbType.NVarChar) { Value = entity.KeyAlgo },
                new SqlParameter("@KeySig", SqlDbType.NVarChar) { Value = entity.SigValue },
                new SqlParameter("@KeySigAlgo", SqlDbType.NVarChar) { Value = entity.SigAlgo },
                new SqlParameter("@Hostname", SqlDbType.NVarChar) { Value = entity.Comment },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_PublicKeys>().FromSqlRaw("[svc].[usp_PublicKey_Update]"
                + "@Id, @IdentityId, @PrivateKeyId, @KeyValueBase64, @KeyValueAlgo, @KeySig, @KeySigAlgo, @Hostname, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_PublicKeys> Update(IEnumerable<uvw_PublicKeys> entities)
        {
            var results = new List<uvw_PublicKeys>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
