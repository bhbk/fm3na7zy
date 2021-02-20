using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands.System
{
    public class SysAmbShowAllCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private string _filter;
        private int _count;

        public SysAmbShowAllCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-amb-show-list", "Show list ambassador credential(s) on system");

            HasRequiredOption("c|count=", "Enter how many results to display", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _count = int.Parse(arg);
            });

            HasOption("f|filter=", "Enter ambassador (full or partial) user principal name to look for", arg =>
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
                var expression = QueryExpressionFactory.GetQueryExpression<Ambassador_EF>();

                if (!string.IsNullOrEmpty(_filter))
                    expression = expression.Where(x => x.UserPrincipalName.Contains(_filter));

                var results = _uow.Ambassadors.Get(expression.ToLambda(),
                    new List<Expression<Func<Ambassador_EF, object>>>()
                    {
                        x => x.FileSystems,
                    })
                    .OrderBy(x => x.UserPrincipalName);

                var ambassadors = results.TakeLast(_count);

                if (results.Count() != ambassadors.Count())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Out.WriteLine($"  *** Showing {_count} ambassador(s). Not showing {results.Count() - ambassadors.Count()} ambassadors(s) ***");
                    Console.Out.WriteLine();
                    Console.ResetColor();
                }

                foreach (var ambassador in ambassadors.OrderBy(x => x.UserPrincipalName))
                    FormatOutput.Write(ambassador, false);

                return FormatOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return FormatOutput.AngryFarewell(ex);
            }
        }
    }
}
