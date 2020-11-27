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
                new SqlParameter("@FileSystemType", SqlDbType.NVarChar) { Value = entity.FileSystemType },
                new SqlParameter("@IsPasswordRequired", SqlDbType.Bit) { Value = entity.IsPasswordRequired },
                new SqlParameter("@IsPublicKeyRequired", SqlDbType.Bit) { Value = entity.IsPublicKeyRequired },
                new SqlParameter("@IsFileSystemReadOnly", SqlDbType.Bit) { Value = entity.IsFileSystemReadOnly },
                new SqlParameter("@Debugger", SqlDbType.NVarChar) { Value = (object)entity.Debugger ?? DBNull.Value },
                new SqlParameter("@IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("@IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_User>("EXEC @ReturnValue = [svc].[usp_User_Insert] "
                + "@IdentityAlias, @FileSystemType, @IsPasswordRequired, @IsPublicKeyRequired, @IsFileSystemReadOnly, "
                + "@Debugger, @IsEnabled, @IsDeletable", pvalues)
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
                new SqlParameter("@FileSystemType", SqlDbType.NVarChar) { Value = entity.FileSystemType },
                new SqlParameter("@IsPublicKeyRequired", SqlDbType.Bit) { Value = entity.IsPublicKeyRequired },
                new SqlParameter("@IsPasswordRequired", SqlDbType.Bit) { Value = entity.IsPasswordRequired },
                new SqlParameter("@IsFileSystemReadOnly", SqlDbType.Bit) { Value = entity.IsFileSystemReadOnly },
                new SqlParameter("@QuotaInBytes", SqlDbType.BigInt) { Value = entity.QuotaInBytes },
                new SqlParameter("@QuotaUsedInBytes", SqlDbType.BigInt) { Value = entity.QuotaUsedInBytes },
                new SqlParameter("@Debugger", SqlDbType.NVarChar) { Value = (object)entity.Debugger ?? DBNull.Value },
                new SqlParameter("@IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("@IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_User>("EXEC @ReturnValue = [svc].[usp_User_Update] "
                + "@IdentityId, @IdentityAlias, @IsPublicKeyRequired, @IsPasswordRequired, @FileSystemType, @IsFileSystemReadOnly, "
                + "@QuotaInBytes, @QuotaUsedInBytes, @Debugger, @IsEnabled, @IsDeletable", pvalues)
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
