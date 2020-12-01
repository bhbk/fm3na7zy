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
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserDeleteCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private User _user;

        public UserDeleteCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("user-delete", "Delete user");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                    .Where(x => x.IdentityAlias == arg && x.IsDeletable == true).ToLambda(),
                        new List<Expression<Func<User, object>>>()
                        {
                            x => x.Files,
                            x => x.Folders,
                        }).SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' or immutable ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var files = _user.Files.Count;
                var folders = _user.Folders.Count;

                if (files > 0)
                    throw new ConsoleHelpAsException($"  *** The user can not be deleted. There are {files} files owned ***");

                if (folders > 0)
                    throw new ConsoleHelpAsException($"  *** The user can not be deleted. There are {folders} folders owned ***");

                _uow.Users.Delete(_user);
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
