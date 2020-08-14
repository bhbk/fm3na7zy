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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;
using System;
using System.Diagnostics;
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
            .UseSerilog()
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
                sc.AddTransient<IAlertService, AlertService>(_ =>
                {
                    var alert = new AlertService(_conf);
                    alert.Grant = new ResourceOwnerGrantV2(_conf);

                    return alert;
                });
                sc.AddTransient<IAdminService, AdminService>(_ =>
                {
                    var admin = new AdminService(_conf);
                    admin.Grant = new ResourceOwnerGrantV2(_conf);

                    return admin;
                });
                sc.AddTransient<IMeService, MeService>(_ =>
                {
                    var me = new MeService(_conf);
                    me.Grant = new ResourceOwnerGrantV2(_conf);

                    return me;
                });
                sc.AddTransient<IStsService, StsService>(_ =>
                {
                    var sts = new StsService(_conf);
                    sts.Grant = new ResourceOwnerGrantV2(_conf);

                    return sts;
                });
                sc.AddSingleton<IHostedService, Daemon>();
                sc.AddQuartz(jobs =>
                {
                    jobs.SchedulerId = Guid.NewGuid().ToString();

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
                sc.AddQuartzServer(opt =>
                {
                    opt.WaitForJobsToComplete = true;
                });
            });

        public static IHostBuilder CreateWindowsHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .UseWindowsService()
            .ConfigureServices((hostContext, sc) =>
            {
                sc.AddSingleton<IConfiguration>(_conf);
                sc.AddSingleton<IContextService>(_instance);
                sc.AddSingleton<ILogger>(_logger);
                sc.AddSingleton<IMapper>(_mapper);
                sc.AddTransient<IAlertService, AlertService>(_ =>
                {
                    var alert = new AlertService(_conf);
                    alert.Grant = new ResourceOwnerGrantV2(_conf);

                    return alert;
                });
                sc.AddTransient<IAdminService, AdminService>(_ =>
                {
                    var admin = new AdminService(_conf);
                    admin.Grant = new ResourceOwnerGrantV2(_conf);

                    return admin;
                });
                sc.AddTransient<IMeService, MeService>(_ =>
                {
                    var me = new MeService(_conf);
                    me.Grant = new ResourceOwnerGrantV2(_conf);

                    return me;
                });
                sc.AddTransient<IStsService, StsService>(_ =>
                {
                    var sts = new StsService(_conf);
                    sts.Grant = new ResourceOwnerGrantV2(_conf);

                    return sts;
                });
                sc.AddTransient<IUnitOfWork, UnitOfWork>(_ =>
                {
                    return new UnitOfWork(_conf["Databases:AuroraEntities"], _instance);
                });
                sc.AddSingleton<IHostedService, Daemon>();
                sc.AddQuartz(jobs =>
                {
                    jobs.SchedulerId = Guid.NewGuid().ToString();

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
                sc.AddQuartzServer(opt =>
                {
                    opt.WaitForJobsToComplete = true;
                });
            });

        public static void Main(string[] args = null)
        {
            var where = Search.ByAssemblyInvocation("appsettings.json");

            _conf = new ConfigurationBuilder()
                .AddJsonFile(where.Name, optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_conf)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.RollingFile(where.DirectoryName + Path.DirectorySeparatorChar + "appdebug.log",
                    retainedFileCountLimit: int.Parse(_conf["Serilog:RollingFile:RetainedFileCountLimit"]),
                    fileSizeLimitBytes: int.Parse(_conf["Serilog:RollingFile:FileSizeLimitBytes"]))
                .CreateLogger();

            _logger = Log.Logger;

            _mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_DIRECT>())
                .CreateMapper();

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
