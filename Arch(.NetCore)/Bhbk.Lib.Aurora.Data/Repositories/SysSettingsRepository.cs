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
    public class SysSettingRepository : GenericRepository<uvw_SysSettings>
    {
        public SysSettingRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_SysSettings Create(uvw_SysSettings entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@ConfigKey", SqlDbType.NVarChar) { Value = entity.ConfigKey },
                new SqlParameter("@ConfigValue", SqlDbType.NVarChar) { Value = entity.ConfigValue },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_SysSettings>().FromSqlRaw("[svc].[usp_SysSetting_Insert]"
                + "@ConfigKey, @ConfigValue, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_SysSetting_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var reader = cmd.ExecuteReader();

                return reader.Cast<uvw_SysSettings>().Single();
            }
            */
        }

        public override IEnumerable<uvw_SysSettings> Create(IEnumerable<uvw_SysSettings> entities)
        {
            var results = new List<uvw_SysSettings>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_SysSettings Delete(uvw_SysSettings entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_SysSettings>().FromSqlRaw("[svc].[usp_SysSetting_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_SysSettings> Delete(IEnumerable<uvw_SysSettings> entities)
        {
            var results = new List<uvw_SysSettings>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_SysSettings> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_SysSettings Update(uvw_SysSettings entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@ConfigKey", SqlDbType.NVarChar) { Value = entity.ConfigKey },
                new SqlParameter("@ConfigValue", SqlDbType.NVarChar) { Value = entity.ConfigValue },
                new SqlParameter("@Immutable", SqlDbType.Bit) { Value = entity.Immutable }
            };

            return _context.Set<uvw_SysSettings>().FromSqlRaw("[svc].[usp_SysSetting_Update]"
                + "@Id, @ConfigKey, @ConfigValue, @Immutable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_SysSettings> Update(IEnumerable<uvw_SysSettings> entities)
        {
            var results = new List<uvw_SysSettings>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
