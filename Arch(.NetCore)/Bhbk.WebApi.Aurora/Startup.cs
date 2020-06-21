using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Aurora.Domain.Infrastructure;
using Bhbk.Lib.Aurora.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.WebApi.Aurora.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Rebex;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Bhbk.WebApi.Aurora
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection sc)
        {
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
            sc.AddSingleton<IHostedService, UnstructuredDataTask>();

            if (instance.InstanceType != InstanceContext.DeployedOrLocal)
                throw new NotSupportedException();

            /*
             * do not use dependency injection for unit of work below. is used 
             * only for owin authentication configuration.
             */

            var seeds = new UnitOfWork(conf["Databases:AuroraEntities"], instance);

            var key = seeds.SysSettings.Get(x => x.ConfigKey == "RebexLicense")
                .OrderBy(x => x.Created).Last();

            Rebex.Licensing.Key = key.ConfigValue;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}
