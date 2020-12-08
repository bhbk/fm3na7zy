using Bhbk.Cli.Aurora.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
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
    public class UserMntEditCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private E_Login _user;
        private bool _alternateCredential;
        private AuthType _authType;
        private string _authTypeList = string.Join(", ", Enum.GetNames(typeof(AuthType)));

        public UserMntEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("user-mount-edit", "Edit mount for user");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                    .Where(x => x.UserName == arg).ToLambda(),
                        new List<Expression<Func<E_Login, object>>>()
                        {
                            x => x.Mount,
                            x => x.Mount.Ambassador,
                        })
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasOption("s|server=", "Enter server DNS/IP address", arg =>
            {
                _user.Mount.ServerAddress = arg;
            });

            HasOption("p|path=", "Enter server share path", arg =>
            {
                _user.Mount.ServerShare = arg;
            });

            HasOption("a|auth=", "Enter type of auth to use", arg =>
            {
                if (!Enum.TryParse(arg, out _authType))
                    throw new ConsoleHelpAsException($"  *** Invalid auth type. Options are '{_authTypeList}' ***");

                _user.Mount.AuthType = _authType.ToString();
            });

            HasOption("c|credential", "Is alternate credential used for mount", arg =>
            {
                _alternateCredential = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (_alternateCredential)
                {
                    var credentials = _uow.Ambassadors.Get();

                    StandardOutputFactory.Ambassadors(credentials);

                    Console.Out.WriteLine();
                    Console.Out.Write("  *** Enter GUID of credential to use for mount *** : ");
                    var input = StandardInput.GetInput();
                    Console.Out.WriteLine();

                    _user.Mount.AmbassadorId = Guid.Parse(input);
                }

                _uow.Logins.Update(_user);
                _uow.Commit();

                StandardOutputFactory.Mounts(new List<E_Mount> { _user.Mount });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
