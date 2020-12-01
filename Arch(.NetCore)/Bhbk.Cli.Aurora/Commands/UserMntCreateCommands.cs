using Bhbk.Cli.Aurora.Factories;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
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
    public class UserMntCreateCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private User _user;
        private AuthType _authType;
        private readonly string _authTypeList = string.Join(", ", Enum.GetNames(typeof(AuthType)));
        private string _serverAddress, _serverShare;
        private bool _alternateCredential;

        public UserMntCreateCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("user-mount-create", "Create user mount");

            HasRequiredOption("u|user=", "Enter user that already exists", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                    .Where(x => x.IdentityAlias == arg).ToLambda(),
                        new List<Expression<Func<User, object>>>()
                        {
                            x => x.Mount,
                            x => x.Mount.Credential,
                        }).SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasRequiredOption("s|server=", "Enter server DNS/IP address", arg =>
            {
                _serverAddress = arg;
            });

            HasRequiredOption("p|path=", "Enter server share path", arg =>
            {
                _serverShare = arg;
            });

            HasRequiredOption("a|auth=", "Enter type of auth to use", arg =>
            {
                if (!Enum.TryParse(arg, out _authType))
                    throw new ConsoleHelpAsException($"*** Invalid auth type. Options are '{_authTypeList}' ***");
            });

            HasOption("c|credential", "Enter to use alternate credential for mount", arg =>
            {
                _alternateCredential = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var exists = _user.Mount;

                if (exists != null)
                {
                    Console.Out.WriteLine("  *** The user already has a mount ***");
                    OutputFactory.StdOutUserMounts(new List<UserMount> { exists });

                    return StandardOutput.FondFarewell();
                }

                UserMount mount;

                if (_alternateCredential)
                {
                    var credentials = _uow.Credentials.Get();

                    OutputFactory.StdOutCredentials(credentials);

                    Console.Out.Write("  *** Enter GUID of credential to use for mount *** : ");
                    var input = StandardInput.GetInput();
                    Console.Out.WriteLine();

                    mount = _uow.UserMounts.Create(
                        new UserMount
                        {
                            IdentityId = _user.IdentityId,
                            CredentialId = Guid.Parse(input),
                            AuthType = _authType.ToString(),
                            ServerAddress = _serverAddress,
                            ServerShare = _serverShare,
                            IsEnabled = true,
                            IsDeletable = true,
                        });

                    _uow.Commit();
                }
                else
                {
                    mount = _uow.UserMounts.Create(
                        new UserMount
                        {
                            IdentityId = _user.IdentityId,
                            AuthType = _authType.ToString(),
                            ServerAddress = _serverAddress,
                            ServerShare = _serverShare,
                            IsEnabled = true,
                            IsDeletable = true,
                        });

                    _uow.Commit();
                }

                OutputFactory.StdOutUserMounts(new List<UserMount>() { mount });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
