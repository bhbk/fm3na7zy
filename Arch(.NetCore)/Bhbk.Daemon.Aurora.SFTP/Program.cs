using AutoMapper;
using Bhbk.Daemon.Aurora.SFTP.Jobs;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Domain.Infrastructure;
using Bhbk.Lib.Aurora.Domain.Primitives.Enums;
using Bhbk.Lib.Common.FileSystem;
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
                sc.AddTransient<IMeService, MeService>(_ =>
                {
                    var me = new MeService(_conf);
                    me.Grant = new ResourceOwnerGrantV2(_conf);

                    return me;
                });
                sc.AddSingleton<IHostedService, Daemon>();
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
                    jobs.UseDefaultThreadPool();

                    if (bool.Parse(_conf["Jobs:MOTDJob:Enable"]))
                    {
                        var motdJobKey = new JobKey(JobType.MOTDDownloadJob.ToString(), GroupType.Daemons.ToString());
                        jobs.AddJob<MOTDDownloadJob>(opt => opt
                            .StoreDurably()
                            .WithIdentity(motdJobKey)
                        );

                        foreach (var cron in _conf.GetSection("Jobs:MOTDJob:Schedules").GetChildren()
                            .Select(x => x.Value).ToList())
                        {
                            jobs.AddTrigger(opt => opt
                                .ForJob(motdJobKey)
                                .StartNow()
                                .WithCronSchedule(cron)
                            );
                        }
                    }
                });
                sc.AddQuartzServer(options =>
                {
                    options.WaitForJobsToComplete = true;
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
                sc.AddTransient<IMeService, MeService>(_ =>
                {
                    var me = new MeService(_conf);
                    me.Grant = new ResourceOwnerGrantV2(_conf);

                    return me;
                });
                sc.AddTransient<IUnitOfWork, UnitOfWork>(_ =>
                {
                    return new UnitOfWork(_conf["Databases:AuroraEntities"], _instance);
                });
                sc.AddSingleton<IHostedService, Daemon>();
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
                    jobs.UseDefaultThreadPool();

                    if (bool.Parse(_conf["Jobs:MOTDDownloadJob:Enable"]))
                    {
                        var motdPullJobKey = new JobKey(JobType.MOTDDownloadJob.ToString(), GroupType.Daemons.ToString());
                        jobs.AddJob<MOTDDownloadJob>(opt => opt
                            .StoreDurably()
                            .WithIdentity(motdPullJobKey)
                        );

                        foreach (var cron in _conf.GetSection("Jobs:MOTDDownloadJob:Schedules").GetChildren()
                            .Select(x => x.Value).ToList())
                        {
                            jobs.AddTrigger(opt => opt
                                .ForJob(motdPullJobKey)
                                .StartNow()
                                .WithCronSchedule(cron)
                            );
                        }
                    }
                    if (bool.Parse(_conf["Jobs:MOTDUploadJob:Enable"]))
                    {
                        var motdPushJobKey = new JobKey(JobType.MOTDUploadJob.ToString(), GroupType.Daemons.ToString());
                        jobs.AddJob<MOTDUploadJob>(opt => opt
                            .StoreDurably()
                            .WithIdentity(motdPushJobKey)
                        );

                        foreach (var cron in _conf.GetSection("Jobs:MOTDUploadJob:Schedules").GetChildren()
                            .Select(x => x.Value).ToList())
                        {
                            jobs.AddTrigger(opt => opt
                                .ForJob(motdPushJobKey)
                                .StartNow()
                                .WithCronSchedule(cron)
                            );
                        }
                    }
                });
                sc.AddQuartzServer(options =>
                {
                    options.WaitForJobsToComplete = true;
                });
            });

        public static void Main(string[] args = null)
        {
            /*
             * https://ss64.com/nt/sc.html
             * 
             * when running as background service on windows, the current working directory resolves 
             * to c:\windows\system32\. does not matter where the assemblies are running from.
             * 
             * use reflection to look at other working directories based on entry points in our quest
             * to locate configuration data needed to start backgroun service.
             */
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var conf = Search.ByAssemblyInvocation("appsettings.json");

                _conf = (IConfiguration)new ConfigurationBuilder()
                    .SetBasePath(conf.DirectoryName)
                    .AddJsonFile(conf.Name, optional: false, reloadOnChange: true)
                    .Build();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _conf = (IConfiguration)new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
            }
            else
                throw new NotSupportedException();

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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                CreateWindowsHostBuilder(args).Build().Run();

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                CreateLinuxHostBuilder(args).Build().Run();

            else
                throw new NotSupportedException();
        }
    }
}
