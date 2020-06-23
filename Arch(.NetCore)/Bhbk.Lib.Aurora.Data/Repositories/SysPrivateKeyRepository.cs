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
    public class SysPrivateKeyRepository : GenericRepository<uvw_SysPrivateKeys>
    {
        public SysPrivateKeyRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_SysPrivateKeys Create(uvw_SysPrivateKeys entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@KeyValueBase64", SqlDbType.NVarChar) { Value = entity.KeyValueBase64 },
                new SqlParameter("@KeyValueAlgo", SqlDbType.NVarChar) { Value = entity.KeyValueAlgo },
                new SqlParameter("@KeyValuePass", SqlDbType.NVarChar) { Value = entity.KeyValuePass },
                new SqlParameter("@KeyValueFormat", SqlDbType.Bit) { Value = entity.KeyValueFormat },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_SysPrivateKeys>().FromSqlRaw("[svc].[usp_SysPrivateKey_Insert]"
                + "@KeyValueBase64, @KeyValueAlgo, @KeyValuePass, @KeyValueFormat, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_SysCredential_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var reader = cmd.ExecuteReader();

                return reader.Cast<uvw_SysPrivateKeys>().Single();
            }
            */
        }

        public override IEnumerable<uvw_SysPrivateKeys> Create(IEnumerable<uvw_SysPrivateKeys> entities)
        {
            var results = new List<uvw_SysPrivateKeys>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_SysPrivateKeys Delete(uvw_SysPrivateKeys entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_SysPrivateKeys>().FromSqlRaw("[svc].[usp_SysPrivateKey_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_SysPrivateKeys> Delete(IEnumerable<uvw_SysPrivateKeys> entities)
        {
            var results = new List<uvw_SysPrivateKeys>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_SysPrivateKeys> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_SysPrivateKeys Update(uvw_SysPrivateKeys entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@KeyValueBase64", SqlDbType.NVarChar) { Value = entity.KeyValueBase64 },
                new SqlParameter("@KeyValueAlgo", SqlDbType.NVarChar) { Value = entity.KeyValueAlgo },
                new SqlParameter("@KeyValuePass", SqlDbType.NVarChar) { Value = entity.KeyValuePass },
                new SqlParameter("@KeyValueFormat", SqlDbType.Bit) { Value = entity.KeyValueFormat },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.Enabled },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_SysPrivateKeys>().FromSqlRaw("[svc].[usp_SysPrivateKey_Update]"
                + "@Id, @KeyValueBase64, @KeyValueAlgo, @KeyValuePass, @KeyValueFormat, @Enabled, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_SysPrivateKeys> Update(IEnumerable<uvw_SysPrivateKeys> entities)
        {
            var results = new List<uvw_SysPrivateKeys>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
