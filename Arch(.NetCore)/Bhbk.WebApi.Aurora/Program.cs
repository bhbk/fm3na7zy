using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Hosting.Options;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Diagnostics;
using System.IO;

namespace Bhbk.WebApi.Aurora
{
    public class Program
    {
        public static IWebHostBuilder CreateIISHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .CaptureStartupErrors(true)
            .UseSerilog()
            .UseIISIntegration()
            .UseStartup<Startup>();

        public static IWebHostBuilder CreateKestrelHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .CaptureStartupErrors(true)
            .UseSerilog()
            .UseKestrel(opt =>
            {
                opt.ConfigureEndpoints();
            })
            .UseUrls()
            .UseStartup<Startup>();

        public static void Main(string[] args)
        {
            var where = Search.ByAssemblyInvocation("appsettings.json");

            var conf = new ConfigurationBuilder()
                .AddJsonFile(where.Name, optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(conf)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.RollingFile(where.DirectoryName + Path.DirectorySeparatorChar + "appdebug.log",
                    retainedFileCountLimit: int.Parse(conf["Serilog:RollingFile:RetainedFileCountLimit"]),
                    fileSizeLimitBytes: int.Parse(conf["Serilog:RollingFile:FileSizeLimitBytes"]))
                .CreateLogger();

            var process = Process.GetCurrentProcess();

            if (process.ProcessName.ToLower().Contains("iis")
                || process.ProcessName.ToLower().Contains("w3wp"))
                CreateIISHostBuilder(args).Build().Run();

            else
                CreateKestrelHostBuilder(args).Build().Run();
        }
    }
}
