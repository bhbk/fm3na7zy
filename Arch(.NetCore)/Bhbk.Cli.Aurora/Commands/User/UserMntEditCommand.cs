using Bhbk.Cli.Aurora.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Aurora.Primitives.Enums;
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

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserMntEditCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private E_Login _user;
        private Uri _uncPath;
        private MountAuthType _authType;
        private string _authTypeList = string.Join(", ", Enum.GetNames(typeof(MountAuthType)));
        private bool _alternateCredential;

        public UserMntEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], instance);

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

            HasOption("p|path=", "Enter UNC path", arg =>
            {
                if (!Uri.TryCreate(arg, UriKind.Absolute, out _uncPath)
                    || !(new Uri(arg).IsUnc))
                    throw new ConsoleHelpAsException($"  *** Invalid UNC path '{arg}' ***");

                _user.Mount.UncPath = arg;
            });

            HasOption("a|auth=", "Enter type of auth", arg =>
            {
                if (!Enum.TryParse(arg, out _authType))
                    throw new ConsoleHelpAsException($"  *** Invalid auth type. Options are '{_authTypeList}' ***");

                _user.Mount.AuthType = _authType.ToString();
            });

            HasOption("c|credential", "Use ambassador credential for mount", arg =>
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
                    Console.Out.Write("  *** Enter GUID of ambassador credential to use for mount *** : ");
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
