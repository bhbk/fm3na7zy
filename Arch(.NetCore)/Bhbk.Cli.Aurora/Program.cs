using ManyConsole;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.IO;

namespace Bhbk.Cli.Aurora
{
    public class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            var conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(conf)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.File($"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}appdebug-.log",
                    retainedFileCountLimit: int.Parse(conf["Serilog:RollingFile:RetainedFileCountLimit"]),
                    fileSizeLimitBytes: int.Parse(conf["Serilog:RollingFile:FileSizeLimitBytes"]),
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var commands = GetCommands();
            return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
        }

        public static IEnumerable<ConsoleCommand> GetCommands()
        {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
        }
    }
}
