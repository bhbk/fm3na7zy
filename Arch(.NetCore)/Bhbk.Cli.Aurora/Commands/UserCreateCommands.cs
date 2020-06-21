﻿using Bhbk.Lib.Aurora.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserCreateCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static string _userName;
        private static string _userPass;
        private static FileSystemTypes _filesystemType;
        private static string _filesystemTypeList = string.Join(", ", Enum.GetNames(typeof(FileSystemTypes)));

        public UserCreateCommands()
        {
            IsCommand("create-user", "Create user");

            HasRequiredOption("u|user=", "Enter user that does not exist already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user given ***");

                var file = SearchRoots.ByAssemblyContext("clisettings.json");

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
                if (!Enum.TryParse(arg, out _filesystemType))
                    throw new ConsoleHelpAsException($"*** Invalid filesystem type. Options are '{_filesystemTypeList}' ***");
            });

            HasOption("p|pass=", "Enter password for user", arg =>
            {
                _userPass = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var user = _uow.Users.Create(
                    new tbl_Users
                    {
                        Id = Guid.NewGuid(),
                        UserName = _userName,
                        FileSystemType = _filesystemType.ToString(),
                        Enabled = true,
                        Created = DateTime.Now,
                        Immutable = false,
                    });

                if (string.IsNullOrEmpty(_userPass))
                {
                    Console.Out.Write("  *** Enter password for the new user *** : ");
                    _userPass = StandardInput.GetHiddenInput();
                }

                _uow.UserPasswords.Create(
                    new tbl_UserPasswords
                    {
                        UserId = user.Id,
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        PasswordHashPBKDF2 = PBKDF2.Create(_userPass),
                        PasswordHashSHA256 = SHA256.Create(_userPass),
                        SecurityStamp = Guid.NewGuid().ToString(),
                        Enabled = true,
                        Created = DateTime.Now,
                        Immutable = false,
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
