using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserEditCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static tbl_Users _user;
        private static string _userPass;
        private static FileSystemTypes _fileSystem;
        private static string _fileSystemList = string.Join(", ", Enum.GetNames(typeof(FileSystemTypes)));

        public UserEditCommands()
        {
            IsCommand("user-edit", "Edit user");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
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

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                    .Where(x => x.UserName == arg && x.Immutable == false).ToLambda(),
                        new List<Expression<Func<tbl_Users, object>>>()
                        {
                            x => x.tbl_UserPasswords,
                        }).SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' or immutable ***");
            });

            HasOption("f|filesystem=", "Enter type of filesystem for user", arg =>
            {
                if (!Enum.TryParse(arg, out _fileSystem))
                    throw new ConsoleHelpAsException($"*** Invalid filesystem type. Options are '{_fileSystemList}' ***");

                if (_user != null)
                    _user.FileSystemType = _fileSystem.ToString();
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
                if (string.IsNullOrEmpty(_userPass))
                {
                    Console.Out.Write("  *** Enter password for the user *** : ");
                    _userPass = StandardInput.GetHiddenInput();
                }

                _user.tbl_UserPasswords.ConcurrencyStamp = Guid.NewGuid().ToString();
                _user.tbl_UserPasswords.HashPBKDF2 = PBKDF2.Create(_userPass);
                _user.tbl_UserPasswords.HashSHA256 = SHA256.Create(_userPass);
                _user.tbl_UserPasswords.SecurityStamp = Guid.NewGuid().ToString();

                _uow.Users.Update(_user);
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
