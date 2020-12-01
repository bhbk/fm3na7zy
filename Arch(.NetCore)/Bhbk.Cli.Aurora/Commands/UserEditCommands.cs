using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.CommandLine.IO;
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
    public class UserEditCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private User _user;
        private FileSystemProviderType _fileSystem;
        private readonly string _fileSystemList = string.Join(", ", Enum.GetNames(typeof(FileSystemProviderType)));

        public UserEditCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("user-edit", "Edit user");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user given ***");

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                    .Where(x => x.IdentityAlias == arg && x.IsDeletable == true).ToLambda())
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' or immutable ***");
            });

            HasOption("f|filesystem=", "Enter type of filesystem for user", arg =>
            {
                if (!Enum.TryParse(arg, out _fileSystem))
                    throw new ConsoleHelpAsException($"*** Invalid filesystem type. Options are '{_fileSystemList}' ***");

                if (_user != null)
                    _user.FileSystemType = _fileSystem.ToString();
            });

            HasOption("k|public-key=", "Require public key for user", arg =>
            {
                _user.RequirePublicKey = bool.Parse(arg);
            });

            HasOption("p|pass=", "Require password for user", arg =>
            {
                _user.RequirePassword = bool.Parse(arg);
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                _uow.Users.Update(_user);
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
