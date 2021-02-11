using AutoMapper;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
using Bhbk.Lib.Aurora.Domain.Profiles;
using Bhbk.Lib.Aurora.Domain.Providers;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Bhbk.Daemon.Aurora.SFTP
{
    public class Program
    {
        private static IConfiguration _conf;
        private static IContextService _env;
        private static ILogger _log;
        private static IMapper _map;

        public static IHostBuilder CreateLinuxHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSystemd()
            .ConfigureLogging((hostContext, builder) =>
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(_conf)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                    .WriteTo.File($"{hostContext.HostingEnvironment.ContentRootPath}{Path.DirectorySeparatorChar}appdebug-.log",
                        retainedFileCountLimit: int.Parse(_conf["Serilog:RollingFile:RetainedFileCountLimit"]),
                        fileSizeLimitBytes: int.Parse(_conf["Serilog:RollingFile:FileSizeLimitBytes"]),
                        rollingInterval: RollingInterval.Day)
                    .CreateLogger();

                _log = Log.Logger;
            })
            .UseSerilog()
            .ConfigureServices((hostContext, sc) =>
            {
                sc.AddSingleton<IConfiguration>(_conf);
                sc.AddSingleton<IContextService>(_env);
                sc.AddSingleton<ILogger>(_log);
                sc.AddSingleton<IMapper>(_map);
                sc.AddSingleton<StateProvider>();
                sc.AddTransient<IUnitOfWork, UnitOfWork>(_ =>
                {
                    return new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], _env);
                });
                sc.AddSingleton<IHostedService, Daemon>();
                sc.AddSingleton<IAlertService, AlertService>(_ =>
                {
                    return new AlertService(_conf)
                    {
                        Grant = new ClientCredentialGrantV2(_conf)
                    };
                });
                sc.AddSingleton<IAdminService, AdminService>(_ =>
                {
                    return new AdminService(_conf)
                    {
                        Grant = new ClientCredentialGrantV2(_conf)
                    };
                });
                sc.AddSingleton<IStsService, StsService>(_ =>
                {
                    return new StsService(_conf)
                    {
                        Grant = new ClientCredentialGrantV2(_conf)
                    };
                });
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

                _log = Log.Logger;
            })
            .ConfigureServices((hostContext, sc) =>
            {
                sc.AddSingleton<IConfiguration>(_conf);
                sc.AddSingleton<IContextService>(_env);
                sc.AddSingleton<ILogger>(_log);
                sc.AddSingleton<IMapper>(_map);
                sc.AddSingleton<StateProvider>();
                sc.AddTransient<IUnitOfWork, UnitOfWork>(_ =>
                {
                    return new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], _env);
                });
                sc.AddSingleton<IHostedService, Daemon>();
                sc.AddTransient<IAlertService, AlertService>(_ =>
                {
                    return new AlertService(_conf)
                    {
                        Grant = new ClientCredentialGrantV2(_conf)
                    };
                });
                sc.AddTransient<IAdminService, AdminService>(_ =>
                {
                    return new AdminService(_conf)
                    {
                        Grant = new ClientCredentialGrantV2(_conf)
                    };
                });
                sc.AddTransient<IStsService, StsService>(_ =>
                {
                    return new StsService(_conf)
                    {
                        Grant = new ClientCredentialGrantV2(_conf)
                    };
                });
            });

        public static void Main(string[] args = null)
        {
            _map = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_EF6>())
                .CreateMapper();

            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _env = new ContextService(InstanceContext.DeployedOrLocal);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                CreateWindowsHostBuilder(args).Build().Run();

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                CreateLinuxHostBuilder(args).Build().Run();

            else
                throw new NotSupportedException();
        }
    }
}
