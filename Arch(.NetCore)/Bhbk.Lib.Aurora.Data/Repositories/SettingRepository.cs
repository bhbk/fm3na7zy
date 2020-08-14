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
    public class SettingRepository : GenericRepository<uvw_Settings>
    {
        public SettingRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_Settings Create(uvw_Settings entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@ConfigKey", SqlDbType.NVarChar) { Value = entity.ConfigKey },
                new SqlParameter("@ConfigValue", SqlDbType.NVarChar) { Value = entity.ConfigValue },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_Settings>().FromSqlRaw("[svc].[usp_Setting_Insert]"
                + "@ConfigKey, @ConfigValue, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_Setting_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var reader = cmd.ExecuteReader();

                return reader.Cast<uvw_Settings>().Single();
            }
            */
        }

        public override IEnumerable<uvw_Settings> Create(IEnumerable<uvw_Settings> entities)
        {
            var results = new List<uvw_Settings>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Settings Delete(uvw_Settings entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Settings>().FromSqlRaw("[svc].[usp_Setting_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Settings> Delete(IEnumerable<uvw_Settings> entities)
        {
            var results = new List<uvw_Settings>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Settings> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Settings Update(uvw_Settings entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@ConfigKey", SqlDbType.NVarChar) { Value = entity.ConfigKey },
                new SqlParameter("@ConfigValue", SqlDbType.NVarChar) { Value = entity.ConfigValue },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_Settings>().FromSqlRaw("[svc].[usp_Setting_Update]"
                + "@Id, @ConfigKey, @ConfigValue, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Settings> Update(IEnumerable<uvw_Settings> entities)
        {
            var results = new List<uvw_Settings>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
