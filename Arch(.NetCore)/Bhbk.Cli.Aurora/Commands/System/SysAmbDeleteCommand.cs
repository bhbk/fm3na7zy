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
    public class SysAmbDeleteCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private Ambassador_EF _ambassador;

        public SysAmbDeleteCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-amb-delete", "Delete ambassador credential on system");

            HasRequiredOption("a|ambassador=", "Enter existing ambassador credential", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No ambassador credential given ***");

                _ambassador = _uow.Ambassadors.Get(QueryExpressionFactory.GetQueryExpression<Ambassador_EF>()
                    .Where(x => x.UserPrincipalName == arg && x.IsDeletable == true).ToLambda())
                    .SingleOrDefault();

                if (_ambassador == null)
                    throw new ConsoleHelpAsException($"  *** Invalid ambassador credential '{arg}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var fileSystemLogins = _uow.FileSystemLogins.Get(QueryExpressionFactory.GetQueryExpression<FileSystemLogin_EF>()
                    .Where(x => x.AmbassadorId == _ambassador.Id).ToLambda(),
                        new List<Expression<Func<FileSystemLogin_EF, object>>>()
                        {
                            x => x.Ambassador,
                            x => x.FileSystem,
                            x => x.Login,
                            x => x.SmbAuthType,
                        });

                if (fileSystemLogins.Any())
                {
                    Console.Out.WriteLine();
                    Console.Out.WriteLine("  *** The credential can not be deleted while in use ***");

                    foreach(var fileSystemLogin in fileSystemLogins)
                        FormatOutput.Write(fileSystemLogin);

                    return StandardOutput.FondFarewell();
                }

                _uow.Ambassadors.Delete(_ambassador);
                _uow.Commit();

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
