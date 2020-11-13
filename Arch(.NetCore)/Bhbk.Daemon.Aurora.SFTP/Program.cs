using AutoMapper;
using Bhbk.Daemon.Aurora.SFTP.Jobs;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Domain.Infrastructure;
using Bhbk.Lib.Aurora.Domain.Primitives.Enums;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Services;
using CronExpressionDescriptor;
using Microsoft.AspNetCore.Hosting;
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
        private static string callPath = "Program.Main";

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
                        var jobKey = new JobKey(JobType.MOTDDownloadJob.ToString(), GroupType.Daemons.ToString());
                        jobs.AddJob<MOTDDownloadJob>(opt => opt
                            .StoreDurably()
                            .WithIdentity(jobKey)
                        );

                        foreach (var cron in _conf.GetSection("Jobs:MOTDDownloadJob:Schedules").GetChildren()
                            .Select(x => x.Value).ToList())
                        {
                            jobs.AddTrigger(opt => opt
                                .ForJob(jobKey)
                                .StartNow()
                                .WithCronSchedule(cron)
                            );

                            Log.Information($"'{callPath}' {jobKey.Name} job has schedule '{ExpressionDescriptor.GetDescription(cron)}'");
                        }
                    }
                    if (bool.Parse(_conf["Jobs:MOTDUploadJob:Enable"]))
                    {
                        var jobKey = new JobKey(JobType.MOTDUploadJob.ToString(), GroupType.Daemons.ToString());
                        jobs.AddJob<MOTDUploadJob>(opt => opt
                            .StoreDurably()
                            .WithIdentity(jobKey)
                        );

                        foreach (var cron in _conf.GetSection("Jobs:MOTDUploadJob:Schedules").GetChildren()
                            .Select(x => x.Value).ToList())
                        {
                            jobs.AddTrigger(opt => opt
                                .ForJob(jobKey)
                                .StartNow()
                                .WithCronSchedule(cron)
                            );

                            Log.Information($"'{callPath}' {jobKey.Name} job has schedule '{ExpressionDescriptor.GetDescription(cron)}'");
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
                        var jobKey = new JobKey(JobType.MOTDDownloadJob.ToString(), GroupType.Daemons.ToString());
                        jobs.AddJob<MOTDDownloadJob>(opt => opt
                            .StoreDurably()
                            .WithIdentity(jobKey)
                        );

                        foreach (var cron in _conf.GetSection("Jobs:MOTDDownloadJob:Schedules").GetChildren()
                            .Select(x => x.Value).ToList())
                        {
                            jobs.AddTrigger(opt => opt
                                .ForJob(jobKey)
                                .StartNow()
                                .WithCronSchedule(cron)
                            );

                            Log.Information($"'{callPath}' {jobKey.Name} job has schedule '{ExpressionDescriptor.GetDescription(cron)}'");
                        }
                    }
                    if (bool.Parse(_conf["Jobs:MOTDUploadJob:Enable"]))
                    {
                        var jobKey = new JobKey(JobType.MOTDUploadJob.ToString(), GroupType.Daemons.ToString());
                        jobs.AddJob<MOTDUploadJob>(opt => opt
                            .StoreDurably()
                            .WithIdentity(jobKey)
                        );

                        foreach (var cron in _conf.GetSection("Jobs:MOTDUploadJob:Schedules").GetChildren()
                            .Select(x => x.Value).ToList())
                        {
                            jobs.AddTrigger(opt => opt
                                .ForJob(jobKey)
                                .StartNow()
                                .WithCronSchedule(cron)
                            );

                            Log.Information($"'{callPath}' {jobKey.Name} job has schedule '{ExpressionDescriptor.GetDescription(cron)}'");
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
            _mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_DIRECT>())
                .CreateMapper();

            _conf = new ConfigurationBuilder()
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
