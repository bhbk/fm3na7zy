using Bhbk.Cli.Aurora.IO;
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
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands.User
{
    public class UserLoginShowCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private Login_EF _user;

        public UserLoginShowCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("user-login-show", "Show login for user");

            HasRequiredOption("u|user=", "Enter existing user", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user given ***");

                _user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                    .Where(x => x.UserName == arg).ToLambda(),
                        new List<Expression<Func<Login_EF, object>>>()
                        {
                            x => x.Alerts,
                            x => x.Files,
                            x => x.Folders,
                            x => x.Mount,
                            x => x.Mount.Ambassador,
                            x => x.Networks,
                            x => x.PrivateKeys,
                            x => x.PublicKeys,
                            x => x.Sessions,
                            x => x.Settings,
                            x => x.Usage,
                        })
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** No user '{arg}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                FormatOutput.Logins(new List<Login_EF> { _user }, true);

                if (_user.Mount != null)
                    FormatOutput.Mounts(new List<Mount_EF> { _user.Mount });

                FormatOutput.KeyPairs(_user.PublicKeys.OrderBy(x => x.CreatedUtc), _user.PrivateKeys);
                FormatOutput.Networks(_user.Networks.OrderBy(x => x.Action));
                FormatOutput.Alerts(_user.Alerts.OrderBy(x => x.ToDisplayName));

                var remotes = _uow.Sessions.Get(QueryExpressionFactory.GetQueryExpression<Session_EF>()
                    .Where(x => x.UserId == _user.UserId && x.IsActive == true).ToLambda())
                    .OrderBy(x => x.UserName).ThenBy(x => x.CreatedUtc)
                    .Select(x => x.RemoteEndPoint).Distinct().TakeLast(100).ToList();

                foreach (var remote in remotes)
                {
                    var sessions = _uow.Sessions.Get(QueryExpressionFactory.GetQueryExpression<Session_EF>()
                        .Where(x => x.RemoteEndPoint == remote).ToLambda());

                    Console.Out.WriteLine();
                    FormatOutput.Sessions(sessions
                        .OrderBy(x => x.CreatedUtc));
                }

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
