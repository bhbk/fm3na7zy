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
    public class PrivateKeyRepository : GenericRepository<uvw_PrivateKeys>
    {
        public PrivateKeyRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_PrivateKeys Create(uvw_PrivateKeys entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@PublicKeyId", SqlDbType.UniqueIdentifier) { Value = entity.PublicKeyId },
                new SqlParameter("@KeyValueBase64", SqlDbType.NVarChar) { Value = entity.KeyValue },
                new SqlParameter("@KeyValueAlgo", SqlDbType.NVarChar) { Value = entity.KeyAlgo },
                new SqlParameter("@KeyValuePass", SqlDbType.NVarChar) { Value = entity.KeyPass },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_PrivateKeys>().FromSqlRaw("[svc].[usp_PrivateKey_Insert]"
                + "@IdentityId, @PublicKeyId, @KeyValueBase64, @KeyValueAlgo, @KeyValuePass, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_PrivateKey_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var reader = cmd.ExecuteReader();

                return reader.Cast<uvw_PrivateKeys>().Single();
            }
            */
        }

        public override IEnumerable<uvw_PrivateKeys> Create(IEnumerable<uvw_PrivateKeys> entities)
        {
            var results = new List<uvw_PrivateKeys>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_PrivateKeys Delete(uvw_PrivateKeys entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_PrivateKeys>().FromSqlRaw("[svc].[usp_PrivateKey_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_PrivateKeys> Delete(IEnumerable<uvw_PrivateKeys> entities)
        {
            var results = new List<uvw_PrivateKeys>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_PrivateKeys> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_PrivateKeys Update(uvw_PrivateKeys entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@PublicKeyId", SqlDbType.UniqueIdentifier) { Value = entity.PublicKeyId },
                new SqlParameter("@KeyValueBase64", SqlDbType.NVarChar) { Value = entity.KeyValue },
                new SqlParameter("@KeyValueAlgo", SqlDbType.NVarChar) { Value = entity.KeyAlgo },
                new SqlParameter("@KeyValuePass", SqlDbType.NVarChar) { Value = entity.KeyPass },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_PrivateKeys>().FromSqlRaw("[svc].[usp_PrivateKey_Insert]"
                + "@Id, @IdentityId, @PublicKeyId, @KeyValueBase64, @KeyValueAlgo, @KeyValuePass, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_PrivateKeys> Update(IEnumerable<uvw_PrivateKeys> entities)
        {
            var results = new List<uvw_PrivateKeys>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
