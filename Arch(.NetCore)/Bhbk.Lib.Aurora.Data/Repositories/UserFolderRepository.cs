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
    public class UserFolderRepository : GenericRepository<uvw_UserFolders>
    {
        public UserFolderRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_UserFolders Create(uvw_UserFolders entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@ParentId", SqlDbType.UniqueIdentifier) { Value = entity.ParentId.HasValue ? (object)entity.ParentId.Value : DBNull.Value },
                new SqlParameter("@VirtualName", SqlDbType.NVarChar) { Value = entity.VirtualName },
                new SqlParameter("@ReadOnly", SqlDbType.Bit) { Value = entity.ReadOnly }
            };

            return _context.Set<uvw_UserFolders>().FromSqlRaw("[svc].[usp_UserFolder_Insert]"
                + "@IdentityId, @ParentId, @VirtualName, @ReadOnly", pvalues.ToArray())
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

                return reader.Cast<uvw_UserFolders>().Single();
            }
            */
        }

        public override IEnumerable<uvw_UserFolders> Create(IEnumerable<uvw_UserFolders> entities)
        {
            var results = new List<uvw_UserFolders>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_UserFolders Delete(uvw_UserFolders entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_UserFolders>().FromSqlRaw("[svc].[usp_UserFolder_Delete] @IdentityId", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_UserFolders> Delete(IEnumerable<uvw_UserFolders> entities)
        {
            var results = new List<uvw_UserFolders>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_UserFolders> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_UserFolders Update(uvw_UserFolders entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@ParentId", SqlDbType.UniqueIdentifier) { Value = entity.ParentId.HasValue ? (object)entity.ParentId.Value : DBNull.Value },
                new SqlParameter("@VirtualName", SqlDbType.NVarChar) { Value = entity.VirtualName },
                new SqlParameter("@ReadOnly", SqlDbType.Bit) { Value = entity.ReadOnly },
                new SqlParameter("@LastAccessed", SqlDbType.DateTime2) { Value = entity.LastAccessed },
                new SqlParameter("@LastUpdated", SqlDbType.DateTime2) { Value = entity.LastUpdated }
            };

            return _context.Set<uvw_UserFolders>().FromSqlRaw("[svc].[usp_UserFolder_Update]"
                + "@Id, @IdentityId, @ParentId, @VirtualName, @ReadOnly, @LastAccessed, @LastUpdated", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_UserFolders> Update(IEnumerable<uvw_UserFolders> entities)
        {
            var results = new List<uvw_UserFolders>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
