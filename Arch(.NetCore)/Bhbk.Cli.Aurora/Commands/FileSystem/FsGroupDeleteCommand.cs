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
    public class FsGroupDeleteCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private FileSystem_EF _fileSystem = null;

        public FsGroupDeleteCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("fs-group-delete", "Delete file-system group on system");

            HasRequiredOption("f|file-system=", "Enter existing file-system group", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No file-system group given ***");

                _fileSystem = _uow.FileSystems.Get(QueryExpressionFactory.GetQueryExpression<FileSystem_EF>()
                    .Where(x => x.Name == arg).ToLambda(),
                        new List<Expression<Func<FileSystem_EF, object>>>()
                        {
                            x => x.Files,
                            x => x.Folders,
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
                Console.Out.WriteLine();

                Console.Out.Write("  *** Enter 'yes' to delete file-system *** : ");
                var input = FormatInput.GetInput();
                Console.Out.WriteLine();

                if (input.ToLower() == "yes")
                {
                    if (!_fileSystem.IsDeletable)
                        throw new ConsoleHelpAsException($"  *** The file-system can not be deleted. ***");

                    var files = _fileSystem.Files.Count;

                    if (files > 0)
                        throw new ConsoleHelpAsException($"  *** The file-system can not be deleted. There are {files} files in it ***");

                    var users = _fileSystem.Logins.Count;

                    if (users > 0)
                        throw new ConsoleHelpAsException($"  *** The file-system can not be deleted. There are {users} users using it ***");

                    _uow.FileSystems.Delete(_fileSystem);
                    _uow.Commit();
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
