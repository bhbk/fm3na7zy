using AutoMapper;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Domain.Infrastructure;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Bhbk.DaemonFTP.Aurora
{
    public class Program
    {
        private static IMapper _mapper;
        private static IConfiguration _conf;
        private static IContextService _instance;

        public static IHostBuilder CreateLinuxHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSystemd()
            .ConfigureServices((hostContext, options) =>
            {
                options.AddSingleton<IMapper>(_mapper);
                options.AddSingleton<IConfiguration>(_conf);
                options.AddSingleton<IContextService>(_instance);
                options.AddScoped<IUnitOfWork, UnitOfWork>(_ =>
                {
                    return new UnitOfWork(_conf["Databases:AuroraEntities"], _instance);
                });
                options.AddSingleton<IHostedService, Daemon>();
            });

        public static IHostBuilder CreateWindowsHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureServices((hostContext, options) =>
            {
                options.AddSingleton<IMapper>(_mapper);
                options.AddSingleton<IConfiguration>(_conf);
                options.AddSingleton<IContextService>(_instance);
                options.AddScoped<IUnitOfWork, UnitOfWork>(_ =>
                {
                    return new UnitOfWork(_conf["Databases:AuroraEntities"], _instance);
                });
                options.AddSingleton<IHostedService, Daemon>();
            });

        public static void Main(string[] args)
        {
            _mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_DIRECT>())
                .CreateMapper();

            _conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _instance = new ContextService(InstanceContext.DeployedOrLocal);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_conf)
                .WriteTo.Console()
                .WriteTo.RollingFile(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "appdebug.log", retainedFileCountLimit: 7)
                .Enrich.FromLogContext()
                .CreateLogger();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                CreateLinuxHostBuilder(args).Build().Run();

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                CreateWindowsHostBuilder(args).Build().Run();

            else
                throw new NotSupportedException();
        }
    }
}
