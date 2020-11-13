using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
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
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static tbl_User _user;
        private static AuthType _authType;
        private static bool _alternateCredential;
        private static string _serverAddress, _serverShare;
        private static string _authTypeList = string.Join(", ", Enum.GetNames(typeof(AuthType)));

        public UserMntCreateCommands()
        {
            IsCommand("user-mount-create", "Create user mount");

            HasRequiredOption("u|user=", "Enter user that already exists", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var instance = new ContextService(InstanceContext.DeployedOrLocal);
                _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                    .Where(x => x.IdentityAlias == arg).ToLambda(),
                        new List<Expression<Func<tbl_User, object>>>()
                        {
                            x => x.tbl_UserMount
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

            HasOption("c|credential=", "Enter to use alternate credential for mount", arg =>
            {
                _alternateCredential = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (_user.tbl_UserMount != null)
                {
                    Console.Out.WriteLine("  *** The user already has a mount ***");
                    ConsoleHelper.StdOutUserMounts(new List<tbl_UserMount> { _user.tbl_UserMount });

                    return StandardOutput.FondFarewell();
                }

                if (_alternateCredential)
                {
                    var credentials = _uow.Credentials.Get();

                    ConsoleHelper.StdOutCredentials(credentials);

                    Console.Out.WriteLine();
                    Console.Out.Write("  *** Enter GUID of credential to use for mount *** : ");
                    var input = StandardInput.GetInput();

                    _uow.UserMounts.Create(
                        new tbl_UserMount
                        {
                            IdentityId = _user.IdentityId,
                            CredentialId = Guid.Parse(input),
                            AuthType = _authType.ToString(),
                            ServerAddress = _serverAddress,
                            ServerShare = _serverShare,
                            IsEnabled = true,
                            IsDeletable = false,
                            CreatedUtc = DateTime.UtcNow,
                        });
                }
                else
                {
                    _uow.UserMounts.Create(
                        new tbl_UserMount
                        {
                            IdentityId = _user.IdentityId,
                            CredentialId = null,
                            AuthType = _authType.ToString(),
                            ServerAddress = _serverAddress,
                            ServerShare = _serverShare,
                            IsEnabled = true,
                            IsDeletable = false,
                            CreatedUtc = DateTime.UtcNow,
                        });
                }

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
