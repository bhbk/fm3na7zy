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
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserDeleteCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static tbl_User _user;

        public UserDeleteCommands()
        {
            IsCommand("user-delete", "Delete user");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var instance = new ContextService(InstanceContext.DeployedOrLocal);
                _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                    .Where(x => x.IdentityAlias == arg && x.Deletable == true).ToLambda(),
                        new List<Expression<Func<tbl_User, object>>>()
                        {
                            x => x.tbl_UserFile,
                            x => x.tbl_UserFolder,
                        }).SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' or immutable ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var files = _user.tbl_UserFile.Count();
                var folders = _user.tbl_UserFolder.Count();

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
