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
    public class UserFileRepository : GenericRepository<uvw_UserFile>
    {
        public UserFileRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_UserFile Create(uvw_UserFile entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@FolderId", SqlDbType.UniqueIdentifier) { Value = entity.FolderId.HasValue ? (object)entity.FolderId.Value : DBNull.Value },
                new SqlParameter("@VirtualName", SqlDbType.NVarChar) { Value = entity.VirtualName },
                new SqlParameter("@RealPath", SqlDbType.NVarChar) { Value = entity.RealPath },
                new SqlParameter("@RealFileName", SqlDbType.NVarChar) { Value = entity.RealFileName },
                new SqlParameter("@RealFileSize", SqlDbType.BigInt) { Value = entity.RealFileSize },
                new SqlParameter("@HashSHA256", SqlDbType.NVarChar) { Value = (object)entity.HashSHA256 ?? DBNull.Value },
                new SqlParameter("@IsReadOnly", SqlDbType.Bit) { Value = entity.IsReadOnly },
            };

            return _context.Set<uvw_UserFile>().FromSqlRaw("[svc].[usp_UserFile_Insert]"
                + "@IdentityId, @FolderId, @VirtualName, @RealPath, @RealFileName, @RealFileSize, @HashSHA256, @IsReadOnly", pvalues.ToArray())
                    .AsEnumerable().Single();

            /*
            using (var conn = _context.Database.GetDbConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[svc].[usp_UserFile_Insert]";
                cmd.Parameters.AddRange(pvalues.ToArray());
                cmd.Connection = conn;
                conn.Open();

                var reader = cmd.ExecuteReader();

                return reader.Cast<uvw_UserFiles>().Single();
            }
            */
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
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id }
            };

            return _context.Set<uvw_UserFile>().FromSqlRaw("[svc].[usp_SysSetting_Delete] @Id", pvalues.ToArray())
                .AsEnumerable().Single();
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
            throw new NotImplementedException();
        }

        public override uvw_UserFile Update(uvw_UserFile entity)
        {
            var pvalues = new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@FolderId", SqlDbType.UniqueIdentifier) { Value = entity.FolderId.HasValue ? (object)entity.FolderId.Value : DBNull.Value },
                new SqlParameter("@VirtualName", SqlDbType.NVarChar) { Value = entity.VirtualName },
                new SqlParameter("@RealPath", SqlDbType.NVarChar) { Value = entity.RealPath },
                new SqlParameter("@RealFileName", SqlDbType.NVarChar) { Value = entity.RealFileName },
                new SqlParameter("@RealFileSize", SqlDbType.BigInt) { Value = entity.RealFileSize },
                new SqlParameter("@HashSHA256", SqlDbType.NVarChar) { Value = (object)entity.HashSHA256 ?? DBNull.Value },
                new SqlParameter("@IsReadOnly", SqlDbType.Bit) { Value = entity.IsReadOnly },
                new SqlParameter("@LastAccessedUtc", SqlDbType.DateTimeOffset) { Value = entity.LastAccessedUtc },
                new SqlParameter("@LastUpdatedUtc", SqlDbType.DateTimeOffset) { Value = entity.LastUpdatedUtc },
                new SqlParameter("@LastVerifiedUtc", SqlDbType.DateTimeOffset) { Value = entity.LastVerifiedUtc },
            };

            return _context.Set<uvw_UserFile>().FromSqlRaw("[svc].[usp_UserFile_Update]"
                + "@Id, @IdentityId, @FolderId, @VirtualName, @RealPath, @RealFileName, @RealFileSize, @FileHashSHA256, @IsReadOnly, "
                + "@LastAccessedUtc, @LastUpdatedUtc, @LastVerifiedUtc", pvalues.ToArray())
                    .AsEnumerable().Single();
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
