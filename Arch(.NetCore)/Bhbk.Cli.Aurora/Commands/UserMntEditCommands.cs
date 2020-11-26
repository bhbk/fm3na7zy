using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data_EF6.Infrastructure;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserMntEditCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private User _user;
        private AuthType _authType;
        private readonly string _authTypeList = string.Join(", ", Enum.GetNames(typeof(AuthType)));
        
        public UserMntEditCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("user-mount-edit", "Edit user mount");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                    .Where(x => x.IdentityAlias == arg).ToLambda(),
                        new List<Expression<Func<User, object>>>()
                        {
                            x => x.Mount
                        }).SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasOption("s|server=", "Enter server DNS/IP address", arg =>
            {
                if(_user != null)
                    _user.Mount.ServerAddress = arg;
            });

            HasOption("p|path=", "Enter server share path", arg =>
            {
                if (_user != null)
                    _user.Mount.ServerShare = arg;
            });

            HasOption("a|auth=", "Enter type of auth to use", arg =>
            {
                if (!Enum.TryParse(arg, out _authType))
                    throw new ConsoleHelpAsException($"*** Invalid auth type. Options are '{_authTypeList}' ***");

                if (_user != null)
                    _user.Mount.AuthType = _authType.ToString();
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (_user.Mount != null)
                {
                    Console.Out.WriteLine("  *** The user already has a mount ***");
                    Console.Out.WriteLine();
                    ConsoleHelper.StdOutUserMounts(new List<UserMount> { _user.Mount });

                    return StandardOutput.FondFarewell();
                }

                var credentials = _uow.Credentials.Get();

                Console.Out.WriteLine();
                ConsoleHelper.StdOutCredentials(credentials);
                Console.Out.WriteLine();

                Console.Out.Write("  *** Enter GUID of credential to use for mount *** : ");
                var input = StandardInput.GetInput();
                Console.Out.WriteLine();

                _user.Mount.CredentialId = Guid.Parse(input);

                _uow.Users.Update(_user);
                _uow.Commit();

                ConsoleHelper.StdOutUserMounts(new List<UserMount>() { _user.Mount });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
