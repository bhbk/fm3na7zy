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
    public class LoginRepository : GenericRepository<uvw_Login>
    {
        public LoginRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_Login Create(uvw_Login entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("UserName", SqlDbType.NVarChar) { Value = entity.UserName },
                new SqlParameter("FileSystemType", SqlDbType.NVarChar) { Value = entity.FileSystemType },
                new SqlParameter("IsPasswordRequired", SqlDbType.Bit) { Value = entity.IsPasswordRequired },
                new SqlParameter("IsPublicKeyRequired", SqlDbType.Bit) { Value = entity.IsPublicKeyRequired },
                new SqlParameter("IsFileSystemReadOnly", SqlDbType.Bit) { Value = entity.IsFileSystemReadOnly },
                new SqlParameter("Debugger", SqlDbType.NVarChar) { Value = (object)entity.Debugger ?? DBNull.Value },
                new SqlParameter("EncryptedPass", SqlDbType.NVarChar) { Value = (object)entity.EncryptedPass ?? DBNull.Value },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_Login>("EXEC @ReturnValue = [svc].[usp_Login_Insert] "
                + "@UserName, @FileSystemType, @IsPasswordRequired, @IsPublicKeyRequired, @IsFileSystemReadOnly, "
                + "@Debugger, @EncryptedPass, @IsEnabled, @IsDeletable", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_Login> Create(IEnumerable<uvw_Login> entities)
        {
            var results = new List<uvw_Login>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Login Delete(uvw_Login entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                rvalue,
            };

            return _context.SqlQuery<uvw_Login>("EXEC @ReturnValue = [svc].[usp_Folder_Delete] @UserId", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_Login> Delete(IEnumerable<uvw_Login> entities)
        {
            var results = new List<uvw_Login>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Login> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Login>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Login Update(uvw_Login entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("UserId", SqlDbType.UniqueIdentifier) { Value = entity.UserId },
                new SqlParameter("UserName", SqlDbType.NVarChar) { Value = entity.UserName },
                new SqlParameter("FileSystemType", SqlDbType.NVarChar) { Value = entity.FileSystemType },
                new SqlParameter("FileSystemChrootPath", SqlDbType.NVarChar) { Value = (object)entity.Debugger ?? DBNull.Value },
                new SqlParameter("IsPublicKeyRequired", SqlDbType.Bit) { Value = entity.IsPublicKeyRequired },
                new SqlParameter("IsPasswordRequired", SqlDbType.Bit) { Value = entity.IsPasswordRequired },
                new SqlParameter("IsFileSystemReadOnly", SqlDbType.Bit) { Value = entity.IsFileSystemReadOnly },
                new SqlParameter("Debugger", SqlDbType.NVarChar) { Value = (object)entity.Debugger ?? DBNull.Value },
                new SqlParameter("EncryptedPass", SqlDbType.NVarChar) { Value = (object)entity.EncryptedPass ?? DBNull.Value },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_Login>("EXEC @ReturnValue = [svc].[usp_Login_Update] "
                + "@UserId, @UserName, @FileSystemType, @FileSystemChrootPath, @IsPublicKeyRequired, @IsPasswordRequired, @IsFileSystemReadOnly, "
                + "@Debugger, @EncryptedPass, @IsEnabled, @IsDeletable", pvalues)
                    .Single();
        }

        public override IEnumerable<uvw_Login> Update(IEnumerable<uvw_Login> entities)
        {
            var results = new List<uvw_Login>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
