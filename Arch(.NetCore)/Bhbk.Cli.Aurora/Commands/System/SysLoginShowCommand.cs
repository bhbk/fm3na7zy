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
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysLoginShowCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private string _user;

        public SysLoginShowCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

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
                IEnumerable<UserLogin> users;

                if (!string.IsNullOrEmpty(_user))
                    users = _uow.UserLogins.Get(QueryExpressionFactory.GetQueryExpression<Session>()
                        .Where(x => x.IdentityAlias.Contains(_user)).ToLambda())
                        .OrderBy(x => x.IdentityAlias).TakeLast(100);

                else
                    users = _uow.UserLogins.Get()
                        .OrderBy(x => x.IdentityAlias).TakeLast(100);

                StandardOutputFactory.Logins(users);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
