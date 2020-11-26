using Bhbk.Lib.Aurora.Data.Models;
using Bhbk.Lib.DataAccess.EFCore.Extensions;
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
    public class UserFileRepository : GenericRepository<uvw_UserFile>
    {
        public UserFileRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_UserFile Create(uvw_UserFile entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@FolderId", SqlDbType.UniqueIdentifier) { Value = entity.FolderId },
                new SqlParameter("@VirtualName", SqlDbType.NVarChar) { Value = entity.VirtualName },
                new SqlParameter("@RealPath", SqlDbType.NVarChar) { Value = entity.RealPath },
                new SqlParameter("@RealFileName", SqlDbType.NVarChar) { Value = entity.RealFileName },
                new SqlParameter("@RealFileSize", SqlDbType.BigInt) { Value = entity.RealFileSize },
                new SqlParameter("@HashSHA256", SqlDbType.NVarChar) { Value = (object)entity.HashSHA256 ?? DBNull.Value },
                new SqlParameter("@IsReadOnly", SqlDbType.Bit) { Value = entity.IsReadOnly },
                rvalue,
            };

            return _context.SqlQuery<uvw_UserFile>("EXEC @ReturnValue = [svc].[usp_UserFile_Insert]"
                + "@IdentityId, @FolderId, @VirtualName, @RealPath, @RealFileName, @RealFileSize, @HashSHA256, @IsReadOnly", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_UserFile> Create(IEnumerable<uvw_UserFile> entities)
        {
            var results = new List<uvw_UserFile>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_UserFile Delete(uvw_UserFile entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue,
            };

            return _context.SqlQuery<uvw_UserFile>("EXEC @ReturnValue = [svc].[usp_UserFile_Delete] @Id", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_UserFile> Delete(IEnumerable<uvw_UserFile> entities)
        {
            var results = new List<uvw_UserFile>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_UserFile> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_UserFile>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_UserFile Update(uvw_UserFile entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@FolderId", SqlDbType.UniqueIdentifier) { Value = entity.FolderId },
                new SqlParameter("@VirtualName", SqlDbType.NVarChar) { Value = entity.VirtualName },
                new SqlParameter("@RealPath", SqlDbType.NVarChar) { Value = entity.RealPath },
                new SqlParameter("@RealFileName", SqlDbType.NVarChar) { Value = entity.RealFileName },
                new SqlParameter("@RealFileSize", SqlDbType.BigInt) { Value = entity.RealFileSize },
                new SqlParameter("@HashSHA256", SqlDbType.NVarChar) { Value = (object)entity.HashSHA256 ?? DBNull.Value },
                new SqlParameter("@IsReadOnly", SqlDbType.Bit) { Value = entity.IsReadOnly },
                new SqlParameter("@LastAccessedUtc", SqlDbType.DateTimeOffset) { Value = entity.LastAccessedUtc },
                new SqlParameter("@LastUpdatedUtc", SqlDbType.DateTimeOffset) { Value = entity.LastUpdatedUtc },
                new SqlParameter("@LastVerifiedUtc", SqlDbType.DateTimeOffset) { Value = entity.LastVerifiedUtc },
                rvalue,
            };

            return _context.SqlQuery<uvw_UserFile>("EXEC @ReturnValue = [svc].[usp_UserFile_Update]"
                + "@Id, @IdentityId, @FolderId, @VirtualName, @RealPath, @RealFileName, @RealFileSize, @FileHashSHA256, @IsReadOnly, "
                + "@LastAccessedUtc, @LastUpdatedUtc, @LastVerifiedUtc", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_UserFile> Update(IEnumerable<uvw_UserFile> entities)
        {
            var results = new List<uvw_UserFile>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
