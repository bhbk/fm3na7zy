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
    public class UserLoginRepository : GenericRepository<uvw_UserLogin>
    {
        public UserLoginRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_UserLogin Create(uvw_UserLogin entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("IdentityAlias", SqlDbType.NVarChar) { Value = entity.IdentityAlias },
                new SqlParameter("FileSystemType", SqlDbType.NVarChar) { Value = entity.FileSystemType },
                new SqlParameter("IsPasswordRequired", SqlDbType.Bit) { Value = entity.IsPasswordRequired },
                new SqlParameter("IsPublicKeyRequired", SqlDbType.Bit) { Value = entity.IsPublicKeyRequired },
                new SqlParameter("IsFileSystemReadOnly", SqlDbType.Bit) { Value = entity.IsFileSystemReadOnly },
                new SqlParameter("Debugger", SqlDbType.NVarChar) { Value = (object)entity.Debugger ?? DBNull.Value },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_UserLogin>("EXEC @ReturnValue = [svc].[usp_UserLogin_Insert] "
                + "@IdentityAlias, @FileSystemType, @IsPasswordRequired, @IsPublicKeyRequired, @IsFileSystemReadOnly, "
                + "@Debugger, @IsEnabled, @IsDeletable", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_UserLogin> Create(IEnumerable<uvw_UserLogin> entities)
        {
            var results = new List<uvw_UserLogin>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_UserLogin Delete(uvw_UserLogin entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                rvalue,
            };

            return _context.SqlQuery<uvw_UserLogin>("EXEC @ReturnValue = [svc].[usp_UserFolder_Delete] @IdentityId", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_UserLogin> Delete(IEnumerable<uvw_UserLogin> entities)
        {
            var results = new List<uvw_UserLogin>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_UserLogin> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_UserLogin>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_UserLogin Update(uvw_UserLogin entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("IdentityAlias", SqlDbType.NVarChar) { Value = entity.IdentityAlias },
                new SqlParameter("FileSystemType", SqlDbType.NVarChar) { Value = entity.FileSystemType },
                new SqlParameter("FileSystemChrootPath", SqlDbType.NVarChar) { Value = (object)entity.Debugger ?? DBNull.Value },
                new SqlParameter("IsPublicKeyRequired", SqlDbType.Bit) { Value = entity.IsPublicKeyRequired },
                new SqlParameter("IsPasswordRequired", SqlDbType.Bit) { Value = entity.IsPasswordRequired },
                new SqlParameter("IsFileSystemReadOnly", SqlDbType.Bit) { Value = entity.IsFileSystemReadOnly },
                new SqlParameter("QuotaInBytes", SqlDbType.BigInt) { Value = entity.QuotaInBytes },
                new SqlParameter("QuotaUsedInBytes", SqlDbType.BigInt) { Value = entity.QuotaUsedInBytes },
                new SqlParameter("SessionMax", SqlDbType.SmallInt) { Value = entity.SessionMax },
                new SqlParameter("SessionsInUse", SqlDbType.SmallInt) { Value = entity.SessionsInUse },
                new SqlParameter("Debugger", SqlDbType.NVarChar) { Value = (object)entity.Debugger ?? DBNull.Value },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_UserLogin>("EXEC @ReturnValue = [svc].[usp_UserLogin_Update] "
                + "@IdentityId, @IdentityAlias, @FileSystemType, @FileSystemChrootPath, @IsPublicKeyRequired, @IsPasswordRequired, @IsFileSystemReadOnly, "
                + "@QuotaInBytes, @QuotaUsedInBytes, @SessionMax, @SessionsInUse, @Debugger, @IsEnabled, @IsDeletable", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_UserLogin> Update(IEnumerable<uvw_UserLogin> entities)
        {
            var results = new List<uvw_UserLogin>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
