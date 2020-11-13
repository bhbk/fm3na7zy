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
    public class NetworkRepository : GenericRepository<uvw_Network>
    {
        public NetworkRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_Network Create(uvw_Network entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@Address", SqlDbType.NVarChar) { Value = entity.Address },
                new SqlParameter("@Action", SqlDbType.NVarChar) { Value = entity.Action },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.IsEnabled },
            };

            return _context.Set<uvw_Network>().FromSqlRaw("[svc].[usp_Network_Insert]"
                + "@IdentityId, @Address, @Action, @Enabled", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_Network_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var reader = cmd.ExecuteReader();

                return reader.Cast<uvw_SysConnections>().Single();
            }
            */
        }

        public override IEnumerable<uvw_Network> Create(IEnumerable<uvw_Network> entities)
        {
            var results = new List<uvw_Network>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Network Delete(uvw_Network entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_Network>().FromSqlRaw("[svc].[usp_Network_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Network> Delete(IEnumerable<uvw_Network> entities)
        {
            var results = new List<uvw_Network>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Network> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_Network Update(uvw_Network entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@Address", SqlDbType.NVarChar) { Value = entity.Address },
                new SqlParameter("@Action", SqlDbType.NVarChar) { Value = entity.Action },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.IsEnabled },
            };

            return _context.Set<uvw_Network>().FromSqlRaw("[svc].[usp_Network_Update]"
                + "@Id, @IdentityId, @Address, @Action, @Enabled", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_Network> Update(IEnumerable<uvw_Network> entities)
        {
            var results = new List<uvw_Network>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
