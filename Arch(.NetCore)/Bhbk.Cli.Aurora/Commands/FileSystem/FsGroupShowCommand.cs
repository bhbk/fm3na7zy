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

namespace Bhbk.Cli.Aurora.Commands.FileSystem
{
    public class FsGroupShowCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private FileSystem_EF _fileSystem = null;

        public FsGroupShowCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("fs-group-show", "Show file-system group on system");

            HasRequiredOption("f|file-system=", "Enter existing file-system group", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No file-system group given ***");

                _fileSystem = _uow.FileSystems.Get(QueryExpressionFactory.GetQueryExpression<FileSystem_EF>()
                    .Where(x => x.Name == arg).ToLambda(),
                        new List<Expression<Func<FileSystem_EF, object>>>()
                        {
                            x => x.Folders,
                            x => x.Files,
                            x => x.Logins,
                            x => x.Usage,
                        })
                    .SingleOrDefault();

                if (_fileSystem == null)
                    throw new ConsoleHelpAsException($"  *** Invalid file-system group '{arg}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                FormatOutput.Write(_fileSystem, true);

                if (_fileSystem.Logins.Count() > 0)
                {
                    Console.Out.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Out.WriteLine($"  [user(s)]");
                    Console.ResetColor();

                    /*
                     * don't know how to do an orderby username down below... grrrr...
                     */

                    foreach (var fileSystemUser in _fileSystem.Logins)
                    {
                        var user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                            .Where(x => x.UserId == fileSystemUser.UserId).ToLambda(),
                                new List<Expression<Func<Login_EF, object>>>()
                                {
                                    x => x.Alerts,
                                    x => x.Networks,
                                    x => x.PrivateKeys,
                                    x => x.PublicKeys,
                                    x => x.Sessions,
                                    x => x.Settings,
                                    x => x.Usage,
                                })
                            .SingleOrDefault();

                        FormatOutput.Write(user, false);
                    }
                }

                return FormatOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return FormatOutput.AngryFarewell(ex);
            }
        }
    }
}
