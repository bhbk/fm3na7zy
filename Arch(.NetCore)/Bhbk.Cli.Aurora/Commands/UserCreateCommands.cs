using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.DataState.Interfaces;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserCreateCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static FileSystemTypes _fileSystem;
        private static string _userName;
        private static string _fileSystemList = string.Join(", ", Enum.GetNames(typeof(FileSystemTypes)));

        public UserCreateCommands()
        {
            IsCommand("user-create", "Create user");

            HasRequiredOption("u|user=", "Enter user that does not exist already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user given ***");

                var file = Search.ByAssemblyInvocation("clisettings.json");

                _conf = (IConfiguration)new ConfigurationBuilder()
                    .SetBasePath(file.DirectoryName)
                    .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                    .Build();

                var instance = new ContextService(InstanceContext.DeployedOrLocal);
                _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

                var user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                    .Where(x => x.UserName == arg).ToLambda()).SingleOrDefault();

                if (user != null)
                    throw new ConsoleHelpAsException($"  *** The user '{arg}' alreay exists ***");

                _userName = arg;
            });

            HasRequiredOption("f|filesystem=", "Enter type of filesystem for user", arg =>
            {
                if (!Enum.TryParse(arg, out _fileSystem))
                    throw new ConsoleHelpAsException($"*** Invalid filesystem type. Options are '{_fileSystemList}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var admin = new AdminService(_conf);
                admin.Grant = new ResourceOwnerGrantV2(_conf);

                var users = admin.User_GetV1(new DataStateV1()
                {
                    Sort = new List<IDataStateSort>()
                        {
                            new DataStateV1Sort() { Field = "userName", Dir = "asc" }
                        },
                    Skip = 0,
                    Take = 100,
                }).Result;

                foreach(var entry in users.Data)
                    Console.Out.WriteLine($"  Username '{entry.UserName}' with GUID '{entry.Id}'");

                Console.Out.WriteLine();
                Console.Out.Write("  *** Enter GUID of (identity) user to use *** : ");
                var input = StandardInput.GetInput();

                var user = _uow.Users.Create(
                    new tbl_Users
                    {
                        Id = Guid.Parse(input),
                        UserName = _userName,
                        AllowPassword = true,
                        FileSystemType = _fileSystem.ToString(),
                        Enabled = true,
                        Created = DateTime.Now,
                        Immutable = false,
                    });

                _uow.Networks.Create(
                    new tbl_Networks
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Address = "0.0.0.0/0",
                        Action = "Allow",
                        Enabled = true,
                        Created = DateTime.UtcNow,
                    });

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
