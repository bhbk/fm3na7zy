using AutoMapper;
using Bhbk.Daemon.Aurora.Simulator.Jobs;
using Bhbk.Lib.Aurora.Domain.Infrastructure;
using Bhbk.Lib.Aurora.Domain.Primitives.Enums;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Bhbk.Daemon.Aurora.Simulator
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
            .ConfigureServices((hostContext, options) =>
            {
                options.AddSingleton<IConfiguration>(_conf);
                options.AddSingleton<IContextService>(_instance);
                options.AddSingleton<ILogger>(_logger);
                options.AddSingleton<IMapper>(_mapper);
                options.AddTransient<IAdminService, AdminService>(_ =>
                {
                    var admin = new AdminService(_conf);
                    admin.Grant = new ResourceOwnerGrantV2(_conf);

                    return admin;
                });
                options.AddTransient<IAlertService, AlertService>(_ =>
                {
                    var alert = new AlertService(_conf);
                    alert.Grant = new ResourceOwnerGrantV2(_conf);

                    return alert;
                });
            });

        public static IHostBuilder CreateWindowsHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureServices((hostContext, sc) =>
            {
                sc.AddSingleton<IConfiguration>(_conf);
                sc.AddSingleton<IContextService>(_instance);
                sc.AddSingleton<ILogger>(_logger);
                sc.AddSingleton<IMapper>(_mapper);
                sc.AddTransient<IAdminService, AdminService>(_ =>
                {
                    var admin = new AdminService(_conf);
                    admin.Grant = new ResourceOwnerGrantV2(_conf);

                    return admin;
                });
                sc.AddTransient<IAlertService, AlertService>(_ =>
                {
                    var alert = new AlertService(_conf);
                    alert.Grant = new ResourceOwnerGrantV2(_conf);

                    return alert;
                });
                sc.AddQuartz(jobs =>
                {
                    jobs.SchedulerId = Guid.NewGuid().ToString();

                    //jobs.UseMicrosoftDependencyInjectionScopedJobFactory();
                    jobs.UseMicrosoftDependencyInjectionJobFactory(options =>
                    {
                        options.AllowDefaultConstructor = false;
                    });

                    jobs.UseSimpleTypeLoader();
                    jobs.UseInMemoryStore();
                    jobs.UseDefaultThreadPool(threads =>
                    {
                        threads.MaxConcurrency = 3;
                    });

                    var downloadJobKey = new JobKey(JobType.DownloadJob.ToString(), GroupType.Sftp.ToString());
                    jobs.AddJob<UploadJob>(opt => opt
                        .StoreDurably()
                        .WithIdentity(downloadJobKey)
                    );

                    foreach (var cron in _conf.GetSection("Jobs:SimDownload:Schedules").GetChildren()
                        .Select(x => x.Value).ToList())
                    {
                        jobs.AddTrigger(opt => opt
                            .ForJob(downloadJobKey)
                            .StartNow()
                            .WithCronSchedule(cron)
                        );
                    }

                    var uploadJobKey = new JobKey(JobType.UploadJob.ToString(), GroupType.Sftp.ToString());
                    jobs.AddJob<UploadJob>(opt => opt
                        .StoreDurably()
                        .WithIdentity(uploadJobKey)
                    );

                    foreach (var cron in _conf.GetSection("Jobs:SimUpload:Schedules").GetChildren()
                        .Select(x => x.Value).ToList())
                    {
                        jobs.AddTrigger(opt => opt
                            .ForJob(uploadJobKey)
                            .StartNow()
                            .WithCronSchedule(cron)
                        );
                    }
                });
                sc.AddQuartzServer(options =>
                {
                    options.WaitForJobsToComplete = true;
                });
            });

        public static void Main(string[] args = null)
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _instance = new ContextService(InstanceContext.DeployedOrLocal);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_conf)
                .WriteTo.Console()
                .WriteTo.RollingFile(Directory.GetCurrentDirectory()
                    + Path.DirectorySeparatorChar + "appdebug.log", retainedFileCountLimit: 7, fileSizeLimitBytes: 10485760)
                .Enrich.FromLogContext()
                .CreateLogger();

            _logger = Log.Logger;

            _mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_DIRECT>())
                .CreateMapper();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                CreateLinuxHostBuilder(args).Build().Run();

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                CreateWindowsHostBuilder(args).Build().Run();

            else
                throw new NotSupportedException();
        }
    }
}
