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

namespace Bhbk.Cli.Aurora.Commands.User
{
    public class UserLoginShowAllCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private string _user;

        public UserLoginShowAllCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-login-show", "Show login(s) on system");

            HasOption("u|user=", "Enter user to search for", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _user = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                IEnumerable<Login_EF> users;

                if (!string.IsNullOrEmpty(_user))
                    users = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Session_EF>()
                        .Where(x => x.UserName.Contains(_user)).ToLambda())
                        .OrderBy(x => x.UserName).TakeLast(100);

                else
                    users = _uow.Logins.Get()
                        .OrderBy(x => x.UserName).TakeLast(100);

                FormatOutput.Logins(users);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
