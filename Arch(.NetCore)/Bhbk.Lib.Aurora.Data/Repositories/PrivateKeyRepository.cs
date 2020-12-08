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
    public class PrivateKeyRepository : GenericRepository<uvw_PrivateKey>
    {
        public PrivateKeyRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_PrivateKey Create(uvw_PrivateKey entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("PublicKeyId", SqlDbType.UniqueIdentifier) { Value = entity.PublicKeyId },
                new SqlParameter("KeyValueBase64", SqlDbType.NVarChar) { Value = entity.KeyValue },
                new SqlParameter("KeyValueAlgo", SqlDbType.NVarChar) { Value = entity.KeyAlgo },
                new SqlParameter("KeyValuePass", SqlDbType.NVarChar) { Value = entity.EncryptedPass },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_PrivateKey>("EXEC @ReturnValue = [svc].[usp_PrivateKey_Insert] "
                + "@UserId, @PublicKeyId, @KeyValueBase64, @KeyValueAlgo, @KeyValuePass, @IsEnabled, @IsDeletable", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_PrivateKey> Create(IEnumerable<uvw_PrivateKey> entities)
        {
            var results = new List<uvw_PrivateKey>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_PrivateKey Delete(uvw_PrivateKey entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue,
            };

            return _context.SqlQuery<uvw_PrivateKey>("EXEC @ReturnValue = [svc].[usp_PrivateKey_Delete] @Id", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_PrivateKey> Delete(IEnumerable<uvw_PrivateKey> entities)
        {
            var results = new List<uvw_PrivateKey>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_PrivateKey> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_PrivateKey>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_PrivateKey Update(uvw_PrivateKey entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId.HasValue ? (object)entity.UserId.Value : DBNull.Value },
                new SqlParameter("PublicKeyId", SqlDbType.UniqueIdentifier) { Value = entity.PublicKeyId },
                new SqlParameter("KeyValueBase64", SqlDbType.NVarChar) { Value = entity.KeyValue },
                new SqlParameter("KeyValueAlgo", SqlDbType.NVarChar) { Value = entity.KeyAlgo },
                new SqlParameter("KeyValuePass", SqlDbType.NVarChar) { Value = entity.EncryptedPass },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_PrivateKey>("EXEC @ReturnValue = [svc].[usp_PrivateKey_Update] "
                + "@Id, @UserId, @PublicKeyId, @KeyValueBase64, @KeyValueAlgo, @KeyValuePass, @IsEnabled, @IsDeletable", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_PrivateKey> Update(IEnumerable<uvw_PrivateKey> entities)
        {
            var results = new List<uvw_PrivateKey>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
