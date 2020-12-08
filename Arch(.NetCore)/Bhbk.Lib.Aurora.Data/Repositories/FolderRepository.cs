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
    public class FolderRepository : GenericRepository<uvw_Folder>
    {
        public FolderRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_Folder Create(uvw_Folder entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("ParentId", SqlDbType.UniqueIdentifier) { Value = entity.ParentId.HasValue ? (object)entity.ParentId.Value : DBNull.Value },
                new SqlParameter("VirtualName", SqlDbType.NVarChar) { Value = entity.VirtualName },
                new SqlParameter("IsReadOnly", SqlDbType.Bit) { Value = entity.IsReadOnly },
                rvalue,
            };

            return _context.SqlQuery<uvw_Folder>("EXEC @ReturnValue = [svc].[usp_Folder_Insert] "
                + "@UserId, @ParentId, @VirtualName, @IsReadOnly", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_Folder> Create(IEnumerable<uvw_Folder> entities)
        {
            var results = new List<uvw_Folder>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Folder Delete(uvw_Folder entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue,
            };

            return _context.SqlQuery<uvw_Folder>("EXEC @ReturnValue = [svc].[usp_Folder_Delete] @UserId", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_Folder> Delete(IEnumerable<uvw_Folder> entities)
        {
            var results = new List<uvw_Folder>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Folder> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Folder>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Folder Update(uvw_Folder entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("ParentId", SqlDbType.UniqueIdentifier) { Value = entity.ParentId.HasValue ? (object)entity.ParentId.Value : DBNull.Value },
                new SqlParameter("VirtualName", SqlDbType.NVarChar) { Value = entity.VirtualName },
                new SqlParameter("IsReadOnly", SqlDbType.Bit) { Value = entity.IsReadOnly },
                new SqlParameter("LastAccessedUtc", SqlDbType.DateTimeOffset) { Value = entity.LastAccessedUtc },
                new SqlParameter("LastUpdatedUtc", SqlDbType.DateTimeOffset) { Value = entity.LastUpdatedUtc },
                rvalue,
            };

            return _context.SqlQuery<uvw_Folder>("EXEC @ReturnValue = [svc].[usp_Folder_Update] "
                + "@Id, @UserId, @ParentId, @VirtualName, @IsReadOnly, @LastAccessedUtc, @LastUpdatedUtc", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_Folder> Update(IEnumerable<uvw_Folder> entities)
        {
            var results = new List<uvw_Folder>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
