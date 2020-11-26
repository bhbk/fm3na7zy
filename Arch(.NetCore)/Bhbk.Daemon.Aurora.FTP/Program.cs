using AutoMapper;
using Bhbk.Lib.Aurora.Data_EF6.Infrastructure;
using Bhbk.Lib.Aurora.Domain.Profiles;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Authorization;
using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.CommandHandlers;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.ConnectionHandlers;
using FubarDev.FtpServer.DataConnection;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters;
using FubarDev.FtpServer.Localization;
using FubarDev.FtpServer.ServerCommandHandlers;
using FubarDev.FtpServer.ServerCommands;
using FubarDev.FtpServer.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Bhbk.Daemon.Aurora.FTP
{
    public class Program
    {
        private static IConfiguration _conf;
        private static IContextService _instance;
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
            })
            .UseSerilog()
            .ConfigureServices((hostContext, options) =>
            {
                options.AddSingleton<IMapper>(_mapper);
                options.AddSingleton<IConfiguration>(_conf);
                options.AddSingleton<IContextService>(_instance);
                options.AddTransient<IUnitOfWork, UnitOfWork>(_ =>
                {
                    return new UnitOfWork(_conf["Databases:AuroraEntities"], _instance);
                });
                options.AddSingleton<IHostedService, Daemon>();
            });

        public static IHostBuilder CreateWindowsHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseWindowsService()
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
            })
            .UseSerilog()
            .ConfigureServices((hostContext, options) =>
            {
                options.AddSingleton<IMapper>(_mapper);
                options.AddSingleton<IConfiguration>(_conf);
                options.AddSingleton<IContextService>(_instance);
                options.AddTransient<IUnitOfWork, UnitOfWork>(_ =>
                {
                    return new UnitOfWork(_conf["Databases:AuroraEntities"], _instance);
                });
                options.AddSingleton<IHostedService, Daemon>();
            });

        public static void Main(string[] args = null)
        {
            _mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile>())
                .CreateMapper();

            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _instance = new ContextService(InstanceContext.DeployedOrLocal);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                CreateLinuxHostBuilder(args).Build().Run();

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                CreateWindowsHostBuilder(args).Build().Run();

            else
                throw new NotSupportedException();
        }
    }
}
