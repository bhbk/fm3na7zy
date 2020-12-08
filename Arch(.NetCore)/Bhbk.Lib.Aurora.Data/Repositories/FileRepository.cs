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
    public class FileRepository : GenericRepository<uvw_File>
    {
        public FileRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_File Create(uvw_File entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("FolderId", SqlDbType.UniqueIdentifier) { Value = entity.FolderId },
                new SqlParameter("VirtualName", SqlDbType.NVarChar) { Value = entity.VirtualName },
                new SqlParameter("RealPath", SqlDbType.NVarChar) { Value = entity.RealPath },
                new SqlParameter("RealFileName", SqlDbType.NVarChar) { Value = entity.RealFileName },
                new SqlParameter("RealFileSize", SqlDbType.BigInt) { Value = entity.RealFileSize },
                new SqlParameter("HashSHA256", SqlDbType.NVarChar) { Value = (object)entity.HashSHA256 ?? DBNull.Value },
                new SqlParameter("IsReadOnly", SqlDbType.Bit) { Value = entity.IsReadOnly },
                rvalue,
            };

            return _context.SqlQuery<uvw_File>("EXEC @ReturnValue = [svc].[usp_File_Insert] "
                + "@UserId, @FolderId, @VirtualName, @RealPath, @RealFileName, @RealFileSize, @HashSHA256, @IsReadOnly", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_File> Create(IEnumerable<uvw_File> entities)
        {
            var results = new List<uvw_File>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_File Delete(uvw_File entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue,
            };

            return _context.SqlQuery<uvw_File>("EXEC @ReturnValue = [svc].[usp_File_Delete] @Id", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_File> Delete(IEnumerable<uvw_File> entities)
        {
            var results = new List<uvw_File>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_File> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_File>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_File Update(uvw_File entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("FolderId", SqlDbType.UniqueIdentifier) { Value = entity.FolderId },
                new SqlParameter("VirtualName", SqlDbType.NVarChar) { Value = entity.VirtualName },
                new SqlParameter("RealPath", SqlDbType.NVarChar) { Value = entity.RealPath },
                new SqlParameter("RealFileName", SqlDbType.NVarChar) { Value = entity.RealFileName },
                new SqlParameter("RealFileSize", SqlDbType.BigInt) { Value = entity.RealFileSize },
                new SqlParameter("HashSHA256", SqlDbType.NVarChar) { Value = (object)entity.HashSHA256 ?? DBNull.Value },
                new SqlParameter("IsReadOnly", SqlDbType.Bit) { Value = entity.IsReadOnly },
                new SqlParameter("LastAccessedUtc", SqlDbType.DateTimeOffset) { Value = entity.LastAccessedUtc },
                new SqlParameter("LastUpdatedUtc", SqlDbType.DateTimeOffset) { Value = entity.LastUpdatedUtc },
                new SqlParameter("LastVerifiedUtc", SqlDbType.DateTimeOffset) { Value = entity.LastVerifiedUtc },
                rvalue,
            };

            return _context.SqlQuery<uvw_File>("EXEC @ReturnValue = [svc].[usp_File_Update] "
                + "@Id, @UserId, @FolderId, @VirtualName, @RealPath, @RealFileName, @RealFileSize, @FileHashSHA256, @IsReadOnly, "
                + "@LastAccessedUtc, @LastUpdatedUtc, @LastVerifiedUtc", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_File> Update(IEnumerable<uvw_File> entities)
        {
            var results = new List<uvw_File>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
