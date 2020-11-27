﻿using Bhbk.Cli.Aurora.Factories;
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
    public class UserLoginCreateCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private FileSystemProviderType _fileSystem;
        private readonly string _fileSystemList = string.Join(", ", Enum.GetNames(typeof(FileSystemProviderType)));
        private string _userName;

        public UserLoginCreateCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("user-login-create", "Create user login");

            HasRequiredOption("u|user=", "Enter user that does not exist already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user given ***");

                var user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                    .Where(x => x.IdentityAlias == arg).ToLambda()).SingleOrDefault();

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
                var admin = new AdminService(_conf)
                {
                    Grant = new ResourceOwnerGrantV2(_conf)
                };

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
                    Console.Out.WriteLine($"  User '{entry.UserName}' with GUID '{entry.Id}'");
                Console.Out.WriteLine();

                Console.Out.Write("  *** Enter GUID of (identity) user to use *** : ");
                var input = StandardInput.GetInput();
                Console.Out.WriteLine();

                var user = _uow.Users.Create(
                    new User
                    {
                        IdentityId = Guid.Parse(input),
                        IdentityAlias = _userName,
                        FileSystemType = _fileSystem.ToString(),
                        IsPasswordRequired = true,
                        IsPublicKeyRequired = false,
                        IsFileSystemReadOnly = true,
                        IsEnabled = true,
                        IsDeletable = false,
                    });

                _uow.Commit();

                _uow.Networks.Create(
                    new Network
                    {
                        IdentityId = user.IdentityId,
                        SequenceId = 0,
                        Address = "::1",
                        Action = NetworkActionType.Allow.ToString(),
                        IsEnabled = true,
                        IsDeletable = true,
                    });

                _uow.Networks.Create(
                    new Network
                    {
                        IdentityId = user.IdentityId,
                        SequenceId = 100,
                        Address = "0.0.0.0/0",
                        Action = NetworkActionType.Allow.ToString(),
                        IsEnabled = true,
                        IsDeletable = true,
                    });

                _uow.Commit();

                OutputFactory.StdOutUsers(new List<User> { user });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
