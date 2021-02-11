using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
using Bhbk.Lib.CommandLine.IO;
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
    public class SysAmbShowCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private Ambassador_EF _ambassador;

        public SysAmbShowCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-amb-show", "Show ambassador credential on system");

            HasRequiredOption("a|ambassador=", "Enter existing ambassador credential", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No ambassador credential given ***");

                _ambassador = _uow.Ambassadors.Get(QueryExpressionFactory.GetQueryExpression<Ambassador_EF>()
                    .Where(x => x.UserPrincipalName == arg).ToLambda(),
                        new List<Expression<Func<Ambassador_EF, object>>>()
                        {
                            x => x.FileSystems,
                        })
                    .SingleOrDefault();

                if (_ambassador == null)
                    throw new ConsoleHelpAsException($"  *** Invalid ambassador credential '{arg}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                FormatOutput.Write(_ambassador, true);

                if (_ambassador.FileSystems.Count() > 0)
                {
                    Console.Out.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Out.WriteLine($"  [file system(s)]");
                    Console.ResetColor();

                    foreach (var fileSystem in _ambassador.FileSystems.OrderBy(x => x.CreatedUtc))
                    {
                        var fileSystemLogin = _uow.FileSystemLogins.Get(QueryExpressionFactory.GetQueryExpression<FileSystemLogin_EF>()
                            .Where(x => x.AmbassadorId == _ambassador.Id).ToLambda(),
                                new List<Expression<Func<FileSystemLogin_EF, object>>>()
                                {
                                    x => x.FileSystem,
                                })
                            .Single();

                        FormatOutput.Write(fileSystemLogin, false);
                    }
                }

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
