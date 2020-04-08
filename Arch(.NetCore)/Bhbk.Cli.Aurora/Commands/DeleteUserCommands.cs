using Bhbk.Lib.Aurora.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands
{
    public class DeleteUserCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static FileInfo _file;
        private static tbl_Users _user;

        public DeleteUserCommands()
        {
            IsCommand("delete-user", "Delete a user");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _file = SearchRoots.ByAssemblyContext("clisettings.json");

                _conf = (IConfiguration)new ConfigurationBuilder()
                    .SetBasePath(_file.DirectoryName)
                    .AddJsonFile(_file.Name, optional: false, reloadOnChange: true)
                    .Build();

                var instance = new ContextService(InstanceContext.DeployedOrLocal);
                _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                    .Where(x => x.UserName == arg && x.Immutable == false).ToLambda()).SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' or immutable ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
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
