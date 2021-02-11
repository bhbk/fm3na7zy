using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Encryption;
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
using System.Linq.Expressions;
using System.Net;

namespace Bhbk.Cli.Aurora.Commands.User
{
    public class UserLoginCreateCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private AuthType_E _authType;
        private FileSystemType_E _fileSystemType;
        private readonly string _authTypeList = string.Join(", ", Enum.GetNames(typeof(AuthType_E)));
        private readonly string _fileSystemTypeList = string.Join(", ", Enum.GetNames(typeof(FileSystemType_E)));
        private string _userName, _comment;

        public UserLoginCreateCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("user-login-create", "Create login for user");

            HasRequiredOption("u|user=", "Enter user that does not exist already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user given ***");

                var user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                    .Where(x => x.UserName == arg).ToLambda())
                    .SingleOrDefault();

                if (user != null)
                    throw new ConsoleHelpAsException($"  *** The user '{arg}' alreay exists ***");

                _userName = arg;
            });

            HasRequiredOption("t|type=", "Enter type of login for user", arg =>
            {
                if (!Enum.TryParse(arg, out _authType))
                    throw new ConsoleHelpAsException($"  *** Invalid login type. Options are '{_authTypeList}' ***");
            });

            HasOption("c|comment=", "Enter comment", arg =>
            {
                CheckRequiredArguments();

                if (!string.IsNullOrEmpty(arg))
                    _comment = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var user = new Login_EF
                {
                    AuthTypeId = (int)_authType,
                    UserName = _userName,
                    IsPasswordRequired = true,
                    IsPublicKeyRequired = false,
                    IsEnabled = true,
                    IsDeletable = true,
                };

                if (_authType == AuthType_E.Identity)
                {
                    var admin = new AdminService(_conf)
                    {
                        Grant = new ClientCredentialGrantV2(_conf)
                    };

                    Console.Out.Write($"  *** Enter email (full or partial) of ({_authType}) user to look for *** : ");
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
                    Console.Out.Write($"  *** Enter GUID of ({_authType}) user *** : ");
                    var identityGuid = Guid.Parse(StandardInput.GetInput());

                    user.UserId = identityGuid;
                }

                if (_authType == AuthType_E.Local)
                {
                    var secret = _conf["Databases:AuroraSecret"];

                    Console.Out.Write($"  *** Enter password of ({_authType}) user *** : ");
                    var decryptedPass = StandardInput.GetHiddenInput();
                    Console.Out.WriteLine();

                    user.UserId = Guid.NewGuid();
                    user.EncryptedPass = AES.EncryptString(decryptedPass, secret);
                }

                user = _uow.Logins.Create(user);
                _uow.Commit();

                if (user.AuthTypeId == (int)AuthType_E.Identity)
                {
                    _uow.Networks.Create(
                        new Network_EF
                        {
                            UserId = user.UserId,
                            SequenceId = 0,
                            Address = "::/64",
                            ActionTypeId = (int)NetworkActionType_E.Allow,
                            IsEnabled = true,
                        });

                    _uow.Networks.Create(
                        new Network_EF
                        {
                            UserId = user.UserId,
                            SequenceId = 10,
                            Address = IPNetwork.IANA_BBLK_RESERVED1.ToString(),
                            ActionTypeId = (int)NetworkActionType_E.Allow,
                            IsEnabled = true,
                        });

                    _uow.Networks.Create(
                        new Network_EF
                        {
                            UserId = user.UserId,
                            SequenceId = 100,
                            Address = "0.0.0.0/0",
                            ActionTypeId = (int)NetworkActionType_E.Deny,
                            IsEnabled = true,
                        });

                    _uow.Commit();
                }

                user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                    .Where(x => x.UserId == user.UserId).ToLambda(),
                        new List<Expression<Func<Login_EF, object>>>()
                        {
                            x => x.Networks,
                            x => x.Usage,
                        })
                    .Single();
                
                Console.Out.WriteLine();
                FormatOutput.Write(user, true);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
