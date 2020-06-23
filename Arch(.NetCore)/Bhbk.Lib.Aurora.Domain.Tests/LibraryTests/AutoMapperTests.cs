using AutoMapper;
using Bhbk.Lib.Aurora.Domain.Infrastructure;
using Xunit;

namespace Bhbk.Lib.Aurora.Domain.Tests.LibraryTests
{
    [Collection("LibraryTests")]
    public class AutoMapperTests : BaseLibraryTests
    {
        [Fact]
        public void Lib_AutoMapper_Profile_Success()
        {
            var Mapper = new MapperConfiguration(
                    x => x.AddProfile<AutoMapperProfile>()).CreateMapper();

            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void Lib_AutoMapper_Profile_Success_Direct()
        {
            var Mapper = new MapperConfiguration(
                    x => x.AddProfile<AutoMapperProfile_DIRECT>()).CreateMapper();

            Mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
