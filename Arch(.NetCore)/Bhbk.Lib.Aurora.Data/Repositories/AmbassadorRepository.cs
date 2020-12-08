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
    public class AmbassadorRepository : GenericRepository<uvw_Ambassador>
    {
        public AmbassadorRepository(AuroraEntities context)
            : base(context) { }

        public override uvw_Ambassador Create(uvw_Ambassador entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("UserName", SqlDbType.NVarChar) { Value = entity.UserName },
                new SqlParameter("EncryptedPass", SqlDbType.NVarChar) { Value = entity.EncryptedPass },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_Ambassador>("EXEC @ReturnValue = [svc].[usp_Ambassador_Insert] "
                + "@Domain, @UserName, @EncryptedPass, @IsEnabled, @IsDeletable", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_Ambassador> Create(IEnumerable<uvw_Ambassador> entities)
        {
            var results = new List<uvw_Ambassador>();

            foreach (var entity in entities)
            {
                var result = Create(entity);

                results.Add(result);
            }

            return results;
        }

        public override uvw_Ambassador Delete(uvw_Ambassador entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                rvalue,
            };

            return _context.SqlQuery<uvw_Ambassador>("EXEC @ReturnValue = [svc].[usp_Ambassador_Delete] @Id", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_Ambassador> Delete(IEnumerable<uvw_Ambassador> entities)
        {
            var results = new List<uvw_Ambassador>();

            foreach (var entity in entities)
            {
                var result = Delete(entity);

                results.Add(result);
            }

            return results;
        }

        public override IEnumerable<uvw_Ambassador> Delete(LambdaExpression lambda)
        {
            var entities = _context.Set<uvw_Ambassador>().AsQueryable()
                .Compile(lambda)
                .ToList();

            return Delete(entities);
        }

        public override uvw_Ambassador Update(uvw_Ambassador entity)
        {
            var rvalue = new SqlParameter("ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output };

            var pvalues = new []
            {
                new SqlParameter("Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("UserName", SqlDbType.NVarChar) { Value = entity.UserName },
                new SqlParameter("EncryptedPass", SqlDbType.NVarChar) { Value = entity.EncryptedPass },
                new SqlParameter("IsEnabled", SqlDbType.Bit) { Value = entity.IsEnabled },
                new SqlParameter("IsDeletable", SqlDbType.Bit) { Value = entity.IsDeletable },
                rvalue,
            };

            return _context.SqlQuery<uvw_Ambassador>("EXEC @ReturnValue = [svc].[usp_Ambassador_Update] "
                + "@Id, @Domain, @UserName, @Password, @IsEnabled, @IsDeletable", pvalues)
                .Single();
        }

        public override IEnumerable<uvw_Ambassador> Update(IEnumerable<uvw_Ambassador> entities)
        {
            var results = new List<uvw_Ambassador>();

            foreach (var entity in entities)
            {
                var result = Update(entity);

                results.Add(result);
            }

            return results;
        }
    }
}
