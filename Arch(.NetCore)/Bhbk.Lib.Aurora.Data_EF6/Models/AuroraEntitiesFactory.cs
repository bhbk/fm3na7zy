using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Bhbk.Lib.Aurora.Data_EF6.Models
{
    //https://docs.microsoft.com/en-us/aspnet/core/data/entity-framework-6?view=aspnetcore-3.1
    public class AuroraEntitiesFactory : IDbContextFactory<AuroraEntities>
    {
        private DbConnection _connectionContext = null;
        private string _connectionString = null;
        public AuroraEntitiesFactory(DbConnection connectionContext) => _connectionContext = connectionContext;
        public AuroraEntitiesFactory(string connnectionString) => _connectionString = connnectionString;

        public AuroraEntities Create()
        {
            if (_connectionContext != null)
                return new AuroraEntities(_connectionContext);

            else if (_connectionString != null)
                return new AuroraEntities(_connectionString);

            throw new NotImplementedException();
        }
    }

    public partial class AuroraEntities : DbContext
    {
        public AuroraEntities(string connectionString)
            : base(connectionString) { }

        public AuroraEntities(DbConnection connectionContext)
            : base(connectionContext, true) { }
    }

}
