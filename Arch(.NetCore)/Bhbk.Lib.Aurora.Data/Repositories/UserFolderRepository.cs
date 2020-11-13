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
    public class UserFolderRepository : GenericRepository<uvw_UserFolder>
    {
        public UserFolderRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_UserFolder Create(uvw_UserFolder entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@ParentId", SqlDbType.UniqueIdentifier) { Value = entity.ParentId.HasValue ? (object)entity.ParentId.Value : DBNull.Value },
                new SqlParameter("@VirtualName", SqlDbType.NVarChar) { Value = entity.VirtualName },
                new SqlParameter("@IsReadOnly", SqlDbType.Bit) { Value = entity.IsReadOnly }
            };

            return _context.Set<uvw_UserFolder>().FromSqlRaw("[svc].[usp_UserFolder_Insert]"
                + "@IdentityId, @ParentId, @VirtualName, @IsReadOnly", pvalues.ToArray())
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

        public override IEnumerable<uvw_UserFolder> Create(IEnumerable<uvw_UserFolder> entities)
        {
            var results = new List<uvw_UserFolder>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_UserFolder Delete(uvw_UserFolder entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_UserFolder>().FromSqlRaw("[svc].[usp_UserFolder_Delete] @IdentityId", pvalues.ToArray())
                .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_UserFolder> Delete(IEnumerable<uvw_UserFolder> entities)
        {
            var results = new List<uvw_UserFolder>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_UserFolder> Delete(LambdaExpression lambda)
        {
            throw new NotImplementedException();
        }

        public override uvw_UserFolder Update(uvw_UserFolder entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@ParentId", SqlDbType.UniqueIdentifier) { Value = entity.ParentId.HasValue ? (object)entity.ParentId.Value : DBNull.Value },
                new SqlParameter("@VirtualName", SqlDbType.NVarChar) { Value = entity.VirtualName },
                new SqlParameter("@IsReadOnly", SqlDbType.Bit) { Value = entity.IsReadOnly },
                new SqlParameter("@LastAccessedUtc", SqlDbType.DateTimeOffset) { Value = entity.LastAccessedUtc },
                new SqlParameter("@LastUpdatedUtc", SqlDbType.DateTimeOffset) { Value = entity.LastUpdatedUtc }
            };

            return _context.Set<uvw_UserFolder>().FromSqlRaw("[svc].[usp_UserFolder_Update]"
                + "@Id, @IdentityId, @ParentId, @VirtualName, @IsReadOnly, @LastAccessedUtc, @LastUpdatedUtc", pvalues.ToArray())
                    .AsEnumerable().Single();
        }

        public override IEnumerable<uvw_UserFolder> Update(IEnumerable<uvw_UserFolder> entities)
        {
            var results = new List<uvw_UserFolder>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
