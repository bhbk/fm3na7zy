using AutoMapper;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Domain.Infrastructure;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Bhbk.Lib.Aurora.Domain.Tests.RespositoryTests_DIRECT
{
    [CollectionDefinition("RepositoryTests_DIRECT")]
    public class BaseRepositoryTestsCollection : ICollectionFixture<BaseRepositoryTests> { }

    public class BaseRepositoryTests
    {
        protected IUnitOfWork UoW;
        protected IMapper Mapper;

        public BaseRepositoryTests()
        {
            var file = Search.ByAssemblyInvocation("appsettings.json");

            var conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(file.DirectoryName)
                .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.UnitTest);

            UoW = new UnitOfWork(conf["Databases:AuroraEntities"], instance);
            Mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_DIRECT>()).CreateMapper();
        }
    }
}
