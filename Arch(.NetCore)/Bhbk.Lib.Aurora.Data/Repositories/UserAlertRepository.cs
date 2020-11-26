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
    public class UserAlertRepository : GenericRepository<uvw_UserAlert>
    {
        public UserAlertRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_UserAlert Create(uvw_UserAlert entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@ToFirstName", SqlDbType.NVarChar) { Value = entity.ToFirstName },
                new SqlParameter("@ToLastName", SqlDbType.NVarChar) { Value = entity.ToLastName },
                new SqlParameter("@ToEmailAddress", SqlDbType.NVarChar) { Value = (object)entity.ToEmailAddress ?? DBNull.Value },
                new SqlParameter("@ToPhoneNumber", SqlDbType.NVarChar) { Value = (object)entity.ToPhoneNumber ?? DBNull.Value },
                new SqlParameter("@OnDelete", SqlDbType.Bit) { Value = entity.OnDelete },
                new SqlParameter("@OnDownload", SqlDbType.Bit) { Value = entity.OnDownload },
                new SqlParameter("@OnUpload", SqlDbType.Bit) { Value = entity.OnUpload },
                rvalue,
            };

            return _context.SqlQuery<uvw_UserAlert>("EXEC @ReturnValue = [svc].[usp_UserAlert_Insert]"
                + "@IdentityId, @FolderId, @VirtualName, @RealPath, @RealFileName, @RealFileSize, @HashSHA256, @IsReadOnly", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_UserAlert> Create(IEnumerable<uvw_UserAlert> entities)
        {
            var results = new List<uvw_UserAlert>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_UserAlert Delete(uvw_UserAlert entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue,
            };

            return _context.SqlQuery<uvw_UserAlert>("EXEC @ReturnValue = [svc].[usp_UserAlert_Delete] @Id", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_UserAlert> Delete(IEnumerable<uvw_UserAlert> entities)
        {
            var results = new List<uvw_UserAlert>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_UserAlert> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_UserAlert>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_UserAlert Update(uvw_UserAlert entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@IdentityId", SqlDbType.UniqueIdentifier) { Value = entity.IdentityId },
                new SqlParameter("@ToFirstName", SqlDbType.NVarChar) { Value = entity.ToFirstName },
                new SqlParameter("@ToLastName", SqlDbType.NVarChar) { Value = entity.ToLastName },
                new SqlParameter("@ToEmailAddress", SqlDbType.NVarChar) { Value = (object)entity.ToEmailAddress ?? DBNull.Value },
                new SqlParameter("@ToPhoneNumber", SqlDbType.NVarChar) { Value = (object)entity.ToPhoneNumber ?? DBNull.Value },
                new SqlParameter("@OnDelete", SqlDbType.Bit) { Value = entity.OnDelete },
                new SqlParameter("@OnDownload", SqlDbType.Bit) { Value = entity.OnDownload },
                new SqlParameter("@OnUpload", SqlDbType.Bit) { Value = entity.OnUpload },
                rvalue,
            };

            return _context.SqlQuery<uvw_UserAlert>("EXEC @ReturnValue = [svc].[usp_UserAlert_Update]"
                + "@Id, @IdentityId, @FolderId, @VirtualName, @RealPath, @RealFileName, @RealFileSize, @FileHashSHA256, @IsReadOnly, "
                + "@LastAccessedUtc, @LastUpdatedUtc, @LastVerifiedUtc", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_UserAlert> Update(IEnumerable<uvw_UserAlert> entities)
        {
            var results = new List<uvw_UserAlert>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
