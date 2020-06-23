using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Bhbk.Lib.Aurora.Domain.Tests.LibraryTests
{
    [CollectionDefinition("LibraryTests")]
    public class BaseLibraryTestsCollection : ICollectionFixture<BaseLibraryTests> { }

    public class BaseLibraryTests { }
}
