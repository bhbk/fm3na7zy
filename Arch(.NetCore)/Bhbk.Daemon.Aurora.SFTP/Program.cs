using AutoMapper;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Domain.Infrastructure;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Bhbk.Daemon.Aurora.SFTP
{
    public class Program
    {
        private static IConfiguration _conf;
        private static IContextService _instance;
        private static ILogger _logger;
        private static IMapper _mapper;

        public static IHostBuilder CreateLinuxHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSystemd()
            .ConfigureLogging((hostContext, builder) =>
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(_conf)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File($"{hostContext.HostingEnvironment.ContentRootPath}{Path.DirectorySeparatorChar}appdebug-.log",
                        retainedFileCountLimit: int.Parse(_conf["Serilog:RollingFile:RetainedFileCountLimit"]),
                        fileSizeLimitBytes: int.Parse(_conf["Serilog:RollingFile:FileSizeLimitBytes"]),
                        rollingInterval: RollingInterval.Day)
                    .CreateLogger();

                _logger = Log.Logger;
            })
            .UseSerilog()
            .ConfigureServices((hostContext, sc) =>
            {
                sc.AddSingleton<IConfiguration>(_conf);
                sc.AddSingleton<IContextService>(_instance);
                sc.AddSingleton<ILogger>(_logger);
                sc.AddSingleton<IMapper>(_mapper);
                sc.AddTransient<IUnitOfWork, UnitOfWork>(_ =>
                {
                    return new UnitOfWork(_conf["Databases:AuroraEntities"], _instance);
                });
                sc.AddTransient<IAlertService, AlertService>(_ =>
                {
                    var alert = new AlertService(_conf)
                    {
                        Grant = new ResourceOwnerGrantV2(_conf)
                    };

                    return alert;
                });
                sc.AddTransient<IAdminService, AdminService>(_ =>
                {
                    var admin = new AdminService(_conf)
                    {
                        Grant = new ResourceOwnerGrantV2(_conf)
                    };

                    return admin;
                });
                sc.AddTransient<IStsService, StsService>(_ =>
                {
                    var sts = new StsService(_conf)
                    {
                        Grant = new ResourceOwnerGrantV2(_conf)
                    };

                    return sts;
                });
                sc.AddSingleton<IHostedService, Daemon>();
            });

        public static IHostBuilder CreateWindowsHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .UseSerilog()
            .ConfigureLogging((hostContext, builder) =>
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(_conf)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File($"{hostContext.HostingEnvironment.ContentRootPath}{Path.DirectorySeparatorChar}appdebug-.log",
                        retainedFileCountLimit: int.Parse(_conf["Serilog:RollingFile:RetainedFileCountLimit"]),
                        fileSizeLimitBytes: int.Parse(_conf["Serilog:RollingFile:FileSizeLimitBytes"]),
                        rollingInterval: RollingInterval.Day)
                    .CreateLogger();

                _logger = Log.Logger;
            })
            .ConfigureServices((hostContext, sc) =>
            {
                sc.AddSingleton<IConfiguration>(_conf);
                sc.AddSingleton<IContextService>(_instance);
                sc.AddSingleton<ILogger>(_logger);
                sc.AddSingleton<IMapper>(_mapper);
                sc.AddTransient<IAlertService, AlertService>(_ =>
                {
                    var alert = new AlertService(_conf)
                    {
                        Grant = new ResourceOwnerGrantV2(_conf)
                    };

                    return alert;
                });
                sc.AddTransient<IAdminService, AdminService>(_ =>
                {
                    var admin = new AdminService(_conf)
                    {
                        Grant = new ResourceOwnerGrantV2(_conf)
                    };

                    return admin;
                });
                sc.AddTransient<IStsService, StsService>(_ =>
                {
                    var sts = new StsService(_conf)
                    {
                        Grant = new ResourceOwnerGrantV2(_conf)
                    };

                    return sts;
                });
                sc.AddTransient<IUnitOfWork, UnitOfWork>(_ =>
                {
                    return new UnitOfWork(_conf["Databases:AuroraEntities"], _instance);
                });
                sc.AddSingleton<IHostedService, Daemon>();
            });

        public static void Main(string[] args = null)
        {
            _mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_DIRECT>())
                .CreateMapper();

            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _instance = new ContextService(InstanceContext.DeployedOrLocal);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                CreateWindowsHostBuilder(args).Build().Run();

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                CreateLinuxHostBuilder(args).Build().Run();

            else
                throw new NotSupportedException();
        }
    }
}
