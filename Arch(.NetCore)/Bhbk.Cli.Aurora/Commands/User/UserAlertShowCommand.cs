using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
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
    public class UserAlertShowCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private Login_EF _user;

        public UserAlertShowCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("user-alert-show", "Show alert(s) for user");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                    .Where(x => x.UserName == arg).ToLambda(),
                        new List<Expression<Func<Login_EF, object>>>()
                        {
                            x => x.Alerts,
                        })
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                foreach (var alert in _user.Alerts)
                    FormatOutput.Write(alert, true);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
