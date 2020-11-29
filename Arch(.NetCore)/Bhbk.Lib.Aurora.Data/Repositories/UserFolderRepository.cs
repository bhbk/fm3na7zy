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
    public class UserFolderRepository : GenericRepository<uvw_UserFolder>
    {
        public UserFolderRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_UserFolder Create(uvw_UserFolder entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("ParentId", SqlDbType.UniqueIdentifier) { Value = entity.ParentId.HasValue ? (object)entity.ParentId.Value : DBNull.Value },
                new SqlParameter("VirtualName", SqlDbType.NVarChar) { Value = entity.VirtualName },
                new SqlParameter("IsReadOnly", SqlDbType.Bit) { Value = entity.IsReadOnly },
                rvalue,
            };

            return _context.SqlQuery<uvw_UserFolder>("EXEC @ReturnValue = [svc].[usp_UserFolder_Insert] "
                + "@IdentityId, @ParentId, @VirtualName, @IsReadOnly", pvalues)
                    .Single();
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
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue,
            };

            return _context.SqlQuery<uvw_UserFolder>("EXEC @ReturnValue = [svc].[usp_UserFolder_Delete] @IdentityId", pvalues)
                .Single();
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
            var entities = _context.Set<uvw_UserFolder>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_UserFolder Update(uvw_UserFolder entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("ParentId", SqlDbType.UniqueIdentifier) { Value = entity.ParentId.HasValue ? (object)entity.ParentId.Value : DBNull.Value },
                new SqlParameter("VirtualName", SqlDbType.NVarChar) { Value = entity.VirtualName },
                new SqlParameter("IsReadOnly", SqlDbType.Bit) { Value = entity.IsReadOnly },
                new SqlParameter("LastAccessedUtc", SqlDbType.DateTimeOffset) { Value = entity.LastAccessedUtc },
                new SqlParameter("LastUpdatedUtc", SqlDbType.DateTimeOffset) { Value = entity.LastUpdatedUtc },
                rvalue,
            };

            return _context.SqlQuery<uvw_UserFolder>("EXEC @ReturnValue = [svc].[usp_UserFolder_Update] "
                + "@Id, @IdentityId, @ParentId, @VirtualName, @IsReadOnly, @LastAccessedUtc, @LastUpdatedUtc", pvalues)
                    .Single();
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
