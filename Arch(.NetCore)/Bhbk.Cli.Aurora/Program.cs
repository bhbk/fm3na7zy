using Bhbk.Lib.Common.FileSystem;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using Serilog;
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
            var where = Search.ByAssemblyInvocation("clisettings.json");

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

            var commands = GetCommands();
            return ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
        }

        public static IEnumerable<ConsoleCommand> GetCommands()
        {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
        }
    }
}
