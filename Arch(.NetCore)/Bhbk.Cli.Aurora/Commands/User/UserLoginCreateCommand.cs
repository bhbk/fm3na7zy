using Bhbk.Cli.Aurora.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.CommandLine.IO;
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
    public class UserLoginCreateCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private FileSystemProviderType _fileSystem;
        private readonly string _fileSystemList = string.Join(", ", Enum.GetNames(typeof(FileSystemProviderType)));
        private string _userName;

        public UserLoginCreateCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("user-login-create", "Create login for user");

            HasRequiredOption("u|user=", "Enter user that does not exist already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user given ***");

                var user = _uow.UserLogins.Get(QueryExpressionFactory.GetQueryExpression<UserLogin>()
                    .Where(x => x.IdentityAlias == arg).ToLambda())
                    .SingleOrDefault();

                if (user != null)
                    throw new ConsoleHelpAsException($"  *** The user '{arg}' alreay exists ***");

                _userName = arg;
            });

            HasRequiredOption("f|filesystem=", "Enter type of filesystem for user", arg =>
            {
                if (!Enum.TryParse(arg, out _fileSystem))
                    throw new ConsoleHelpAsException($"  *** Invalid filesystem type. Options are '{_fileSystemList}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var admin = new AdminService(_conf)
                {
                    Grant = new ClientCredentialGrantV2(_conf)
                };

                Console.Out.Write("  *** Enter email (full or partial) of (Identity) user to look for *** : ");
                var emailSearch = StandardInput.GetInput();
                Console.Out.WriteLine();

                var identityUsers = admin.User_GetV1(
                    new DataStateV1()
                    {
                        Filter = new DataStateV1Filter()
                        {
                            Filters = new List<IDataStateFilter>()
                            {
                                new DataStateV1Filter { Field = "userName", Operator = "contains", Value = emailSearch },
                            }
                        },
                        Sort = new List<IDataStateSort>()
                        {
                            new DataStateV1Sort() { Field = "userName", Dir = "asc" }
                        },
                        Skip = 0,
                        Take = 100,
                    })
                    .AsTask().Result;

                foreach (var identityUser in identityUsers.Data)
                    Console.Out.WriteLine($"  [identity GUID] {identityUser.Id} [email] {identityUser.UserName}");

                Console.Out.WriteLine();
                Console.Out.Write("  *** Enter GUID of (Identity) user to use *** : ");
                var identityGuid = Guid.Parse(StandardInput.GetInput());

                var user = _uow.UserLogins.Create(
                    new UserLogin
                    {
                        IdentityId = identityGuid,
                        IdentityAlias = _userName,
                        FileSystemType = _fileSystem.ToString(),
                        IsPasswordRequired = true,
                        IsPublicKeyRequired = false,
                        IsFileSystemReadOnly = true,
                        IsEnabled = true,
                        IsDeletable = false,
                    });

                _uow.Commit();

                Console.Out.WriteLine();
                StandardOutputFactory.Logins(new List<UserLogin> { user }, "extras");

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
