using Bhbk.Lib.Aurora.Data.Models;
using Bhbk.Lib.DataAccess.EFCore.Extensions;
using Bhbk.Lib.DataAccess.EFCore.Repositories;
using Microsoft.Data.SqlClient;
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
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@PrivateKeyId", SqlDbType.UniqueIdentifier) { Value = entity.PrivateKeyId.HasValue ? (object)entity.PrivateKeyId.Value : DBNull.Value },
                new SqlParameter("@KeyValueBase64", SqlDbType.NVarChar) { Value = entity.KeyValue },
                new SqlParameter("@KeyValueAlgo", SqlDbType.NVarChar) { Value = entity.KeyAlgo },
                new SqlParameter("@KeySig", SqlDbType.NVarChar) { Value = entity.SigValue },
                new SqlParameter("@KeySigAlgo", SqlDbType.NVarChar) { Value = entity.SigAlgo },
                new SqlParameter("@Comment", SqlDbType.NVarChar) { Value = entity.Comment },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("@Deletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_PublicKey>("EXEC @ReturnValue = [svc].[usp_PublicKey_Insert]"
                + "@IdentityId, @PrivateKeyId, @KeyValueBase64, @KeyValueAlgo, @KeySig, @KeySigAlgo, @Comment, @Enabled, @Deletable", pvalues)
                .Single();
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
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue,
            };

            return _context.SqlQuery<uvw_PublicKey>("EXEC @ReturnValue = [svc].[usp_PublicKey_Delete] @Id", pvalues)
                .Single();
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
            var entities = _context.Set<uvw_PublicKey>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_PublicKey Update(uvw_PublicKey entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
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
                new SqlParameter("@Deletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_PublicKey>("EXEC @ReturnValue = [svc].[usp_PublicKey_Update]"
                + "@Id, @IdentityId, @PrivateKeyId, @KeyValueBase64, @KeyValueAlgo, @KeySig, @KeySigAlgo, @Comment, @Enabled, @Deletable", pvalues)
                .Single();
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
