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
            .UseKestrel(options =>
            {
                options.ConfigureEndpoints();
            })
            .UseUrls()
            .UseStartup<Startup>();

        public static void Main(string[] args)
        {
            var conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(conf)
                .WriteTo.Console()
                .WriteTo.RollingFile(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "appdebug.log", retainedFileCountLimit: 7)
                .Enrich.FromLogContext()
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
