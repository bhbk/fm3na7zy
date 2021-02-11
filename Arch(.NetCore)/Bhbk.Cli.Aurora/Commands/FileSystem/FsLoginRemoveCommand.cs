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
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands
{
    public class FsUserRemoveCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private FileSystem_EF _fileSystem = null;
        private FileSystemLogin_EF _fileSystemLogin = null;
        private Login_EF _user = null;

        public FsUserRemoveCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("fs-login-remove", "Remove file-system membership for user");

            HasRequiredOption("f|file-system=", "Enter existing file-system group", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No file-system group given ***");

                _fileSystem = _uow.FileSystems.Get(QueryExpressionFactory.GetQueryExpression<FileSystem_EF>()
                    .Where(x => x.Name == arg).ToLambda())
                    .SingleOrDefault();

                if (_fileSystem == null)
                    throw new ConsoleHelpAsException($"  *** Invalid file-system group '{arg}' ***");
            });

            HasRequiredOption("u|user=", "Enter existing user", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user given ***");

                _user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                    .Where(x => x.UserName == arg && x.IsDeletable == true).ToLambda())
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                _fileSystemLogin = _uow.FileSystemLogins.Get(QueryExpressionFactory.GetQueryExpression<FileSystemLogin_EF>()
                    .Where(x => x.UserId == _user.UserId && x.FileSystemId == _fileSystem.Id).ToLambda())
                    .SingleOrDefault();

                if (_fileSystemLogin == null)
                    throw new ConsoleHelpAsException($"  *** No membership for user {_user.UserName} in file-system group '{_fileSystem.Name}' exists ***");

                FormatOutput.Write(_fileSystemLogin, true);
                Console.Out.WriteLine();

                Console.Out.Write("  *** Enter 'yes' to delete file-system membership *** : ");
                var input = FormatInput.GetInput();
                Console.Out.WriteLine();

                if (input.ToLower() == "yes")
                {
                    _uow.FileSystemLogins.Delete(_fileSystemLogin);
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
