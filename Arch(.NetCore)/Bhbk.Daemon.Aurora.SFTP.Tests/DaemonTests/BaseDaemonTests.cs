using Bhbk.Daemon.Aurora.SFTP;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Linq.Dynamic.Core;
using Xunit;
using AutoMapper;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Domain.Infrastructure;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Serilog;
using System.IO;
using System.Runtime.InteropServices;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Bhbk.Daemon.Aurora.SFTP.Tests.DaemonTests
{
    public class BaseDaemonTests
    {
        public BaseDaemonTests()
        {

        }
    }
}
