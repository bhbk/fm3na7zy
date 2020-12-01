using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Bhbk.Daemon.Aurora.SFTP.Factories;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Hosting;
using Rebex;
using Rebex.IO.FileSystem.Notifications;
using Rebex.Net;
using Rebex.Net.Servers;
using Rebex.Security.Cryptography;
using Serilog;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Bhbk.Daemon.Aurora.SFTP.Tests.DaemonTests
{
    public class UserDaemonTests : IClassFixture<BaseDaemonTests>
    {
        private readonly BaseDaemonTests _factory;

        public UserDaemonTests(BaseDaemonTests factory) => _factory = factory;

        [Fact]
        public void Sftp_User_Login_Success()
        {

        }
    }
}
