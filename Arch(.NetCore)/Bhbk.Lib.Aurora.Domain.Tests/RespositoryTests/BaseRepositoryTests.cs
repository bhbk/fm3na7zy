using AutoMapper;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Domain.Infrastructure;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Bhbk.Lib.Aurora.Domain.Tests.RespositoryTests
{
    [CollectionDefinition("RepositoryTests")]
    public class BaseRepositoryTestsCollection : ICollectionFixture<BaseRepositoryTests> { }

    class BaseRepositoryTests
    {
    }
}
