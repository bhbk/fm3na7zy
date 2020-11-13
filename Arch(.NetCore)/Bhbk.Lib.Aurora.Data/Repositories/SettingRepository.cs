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
    public class SettingRepository : GenericRepository<uvw_Setting>
    {
        public SettingRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_Setting Create(uvw_Setting entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@ConfigKey", SqlDbType.NVarChar) { Value = entity.ConfigKey },
                new SqlParameter("@ConfigValue", SqlDbType.NVarChar) { Value = entity.ConfigValue },
                new SqlParameter("@IdDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
            };

            return _context.Set<uvw_Setting>().FromSqlRaw("[svc].[usp_Setting_Insert]"
                + "@IdentityId, @ConfigKey, @ConfigValue, @IdDeletable", pvalues.ToArray())
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

        public override IEnumerable<uvw_Setting> Create(IEnumerable<uvw_Setting> entities)
        {
            var results = new List<uvw_Setting>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Setting Delete(uvw_Setting entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Setting>().FromSqlRaw("[svc].[usp_Setting_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Setting> Delete(IEnumerable<uvw_Setting> entities)
        {
            var results = new List<uvw_Setting>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Setting> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Setting Update(uvw_Setting entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@ConfigKey", SqlDbType.NVarChar) { Value = entity.ConfigKey },
                new SqlParameter("@ConfigValue", SqlDbType.NVarChar) { Value = entity.ConfigValue },
                new SqlParameter("@IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable }
            };

            return _context.Set<uvw_Setting>().FromSqlRaw("[svc].[usp_Setting_Update]"
                + "@Id, @IdentityId, @ConfigKey, @ConfigValue, @IsDeletable", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Setting> Update(IEnumerable<uvw_Setting> entities)
        {
            var results = new List<uvw_Setting>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
