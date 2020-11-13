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
    public class PublicKeyRepository : GenericRepository<uvw_PublicKey>
    {
        public PublicKeyRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_PublicKey Create(uvw_PublicKey entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@PrivateKeyId", SqlDbType.UniqueIdentifier) { Value = entity.PrivateKeyId.HasValue ? (object)entity.PrivateKeyId.Value : DBNull.Value },
                new SqlParameter("@KeyValueBase64", SqlDbType.NVarChar) { Value = entity.KeyValue },
                new SqlParameter("@KeyValueAlgo", SqlDbType.NVarChar) { Value = entity.KeyAlgo },
                new SqlParameter("@KeySig", SqlDbType.NVarChar) { Value = entity.SigValue },
                new SqlParameter("@KeySigAlgo", SqlDbType.NVarChar) { Value = entity.SigAlgo },
                new SqlParameter("@Comment", SqlDbType.NVarChar) { Value = entity.Comment },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("@Deletable", SqlDbType.Bit) { Value = entity.IsDeletable }
            };

            return _context.Set<uvw_PublicKey>().FromSqlRaw("[svc].[usp_PublicKey_Insert]"
                + "@IdentityId, @PrivateKeyId, @KeyValueBase64, @KeyValueAlgo, @KeySig, @KeySigAlgo, @Comment, @Enabled, @Deletable", pvalues.ToArray())
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

        public override IEnumerable<uvw_PublicKey> Create(IEnumerable<uvw_PublicKey> entities)
        {
            var results = new List<uvw_PublicKey>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_PublicKey Delete(uvw_PublicKey entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_PublicKey>().FromSqlRaw("[svc].[usp_PublicKey_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_PublicKey> Delete(IEnumerable<uvw_PublicKey> entities)
        {
            var results = new List<uvw_PublicKey>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_PublicKey> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_PublicKey Update(uvw_PublicKey entity)
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
                new SqlParameter("@Comment", SqlDbType.NVarChar) { Value = entity.Comment },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("@Deletable", SqlDbType.Bit) { Value = entity.IsDeletable }
            };

            return _context.Set<uvw_PublicKey>().FromSqlRaw("[svc].[usp_PublicKey_Update]"
                + "@Id, @IdentityId, @PrivateKeyId, @KeyValueBase64, @KeyValueAlgo, @KeySig, @KeySigAlgo, @Comment, @Enabled, @Deletable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_PublicKey> Update(IEnumerable<uvw_PublicKey> entities)
        {
            var results = new List<uvw_PublicKey>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
