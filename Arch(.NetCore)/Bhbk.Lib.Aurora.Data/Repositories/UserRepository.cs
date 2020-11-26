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
    public class UserRepository : GenericRepository<uvw_User>
    {
        public UserRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_User Create(uvw_User entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("@IdentityAlias", SqlDbType.NVarChar) { Value = entity.IdentityAlias },
                new SqlParameter("@RequirePublicKey", SqlDbType.Bit) { Value = entity.RequirePublicKey },
                new SqlParameter("@RequirePassword", SqlDbType.Bit) { Value = entity.RequirePassword },
                new SqlParameter("@FileSystemType", SqlDbType.NVarChar) { Value = entity.FileSystemType },
                new SqlParameter("@FileSystemReadOnly", SqlDbType.Bit) { Value = entity.FileSystemReadOnly },
                new SqlParameter("@DebugLevel", SqlDbType.NVarChar) { Value = (object)entity.DebugLevel ?? DBNull.Value },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("@Deletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_User>("EXEC @ReturnValue = [svc].[usp_User_Insert]"
                + "@IdentityAlias, @RequirePublicKey, @RequirePassword, @FileSystemType, @FileSystemReadOnly, @DebugLevel, @Enabled, @Deletable", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_User> Create(IEnumerable<uvw_User> entities)
        {
            var results = new List<uvw_User>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_User Delete(uvw_User entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                rvalue,
            };

            return _context.SqlQuery<uvw_User>("EXEC @ReturnValue = [svc].[usp_UserFolder_Delete] @IdentityId", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_User> Delete(IEnumerable<uvw_User> entities)
        {
            var results = new List<uvw_User>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_User> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_User>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_User Update(uvw_User entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@IdentityAlias", SqlDbType.NVarChar) { Value = entity.IdentityAlias },
                new SqlParameter("@RequirePublicKey", SqlDbType.Bit) { Value = entity.RequirePublicKey },
                new SqlParameter("@RequirePassword", SqlDbType.Bit) { Value = entity.RequirePassword },
                new SqlParameter("@FileSystemType", SqlDbType.NVarChar) { Value = entity.FileSystemType },
                new SqlParameter("@FileSystemReadOnly", SqlDbType.Bit) { Value = entity.FileSystemReadOnly },
                new SqlParameter("@DebugLevel", SqlDbType.NVarChar) { Value = (object)entity.DebugLevel ?? DBNull.Value },
                new SqlParameter("@Enabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("@Deletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_User>("EXEC @ReturnValue = [svc].[usp_User_Update]"
                + "@IdentityId, @IdentityId, @IdentityAlias, @RequirePublicKey, @RequirePassword, @FileSystemType, @FileSystemReadOnly, @DebugLevel, @Enabled, @Deletable", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_User> Update(IEnumerable<uvw_User> entities)
        {
            var results = new List<uvw_User>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
