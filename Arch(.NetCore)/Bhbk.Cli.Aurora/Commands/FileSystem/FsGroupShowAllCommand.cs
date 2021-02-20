using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.QueryExpression;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands.FileSystem
{
    public class FsGroupShowAllCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private string _filter;
        private int _count;

        public FsGroupShowAllCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("fs-group-show-all", "Show all file-system(s) on system");

            HasRequiredOption("c|count=", "Enter how many results to display", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _count = int.Parse(arg);
            });

            HasOption("f|filter=", "Enter file-system (full or partial) name to look for", arg =>
            {
                CheckRequiredArguments();

                if (!string.IsNullOrEmpty(arg))
                    _filter = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var expression = QueryExpressionFactory.GetQueryExpression<FileSystem_EF>();

                if (!string.IsNullOrEmpty(_filter))
                    expression = expression.Where(x => x.Name.Contains(_filter));

                var results = _uow.FileSystems.Get(expression.ToLambda(),
                    new List<Expression<Func<FileSystem_EF, object>>>()
                        {
                            x => x.Files,
                            x => x.Folders,
                            x => x.Logins,
                            x => x.Usage,
                        })
                    .OrderBy(x => x.Name);

                var fileSystems = results.TakeLast(_count);

                if (results.Count() != fileSystems.Count())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Out.WriteLine($"  *** Showing {_count} file-system(s). Not showing {results.Count() - fileSystems.Count()} file-system(s) ***");
                    Console.Out.WriteLine();
                    Console.ResetColor();
                }

                foreach (var fileSystem in fileSystems)
                    FormatOutput.Write(fileSystem, false);

                return FormatOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return FormatOutput.AngryFarewell(ex);
            }
        }
    }
}
