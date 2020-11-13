using AutoMapper;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Domain.Infrastructure;
using Bhbk.Lib.Aurora.Domain.Primitives.Enums;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.WebApi.Aurora.Tasks;
using CronExpressionDescriptor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bhbk.WebApi.Aurora
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection sc)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            var mapper = new MapperConfiguration(x => x.AddProfile<AutoMapperProfile_DIRECT>()).CreateMapper();

            sc.AddSingleton<IConfiguration>(conf);
            sc.AddSingleton<IContextService>(instance);
            sc.AddSingleton<IMapper>(mapper);
            sc.AddScoped<IUnitOfWork, UnitOfWork>(_ =>
            {
                return new UnitOfWork(conf["Databases:AuroraEntities"], instance);
            });
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

                if (bool.Parse(conf["Jobs:UnstructuredData:Enable"]))
                {
                    var jobKey = new JobKey(JobType.UnstructuredData.ToString(), GroupType.Daemons.ToString());
                    jobs.AddJob<UnstructuredDataJob>(opt => opt
                        .StoreDurably()
                        .WithIdentity(jobKey)
                    );

                    foreach (var cron in conf.GetSection("Jobs:UnstructuredData:Schedules").GetChildren()
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
            sc.AddQuartzServer(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            if (instance.InstanceType != InstanceContext.DeployedOrLocal)
                throw new NotSupportedException();

            /*
             * do not use dependency injection for unit of work below. is used 
             * only for owin authentication configuration.
             */

            var seeds = new UnitOfWork(conf["Databases:AuroraEntities"], instance);

            var key = seeds.Settings.Get(x => x.ConfigKey == "RebexLicense")
                .OrderBy(x => x.CreatedUtc).Last();

            Rebex.Licensing.Key = key.ConfigValue;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}
