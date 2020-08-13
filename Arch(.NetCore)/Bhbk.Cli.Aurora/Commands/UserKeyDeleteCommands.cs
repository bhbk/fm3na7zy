﻿using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
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
    public class UserKeyDeleteCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static tbl_Users _user;
        private static bool _delete = false, _deleteAll = false;

        public UserKeyDeleteCommands()
        {
            IsCommand("user-key-delete", "Delete user public/private key pairs");

            HasRequiredOption("u|user=", "Enter existing user", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                var file = Search.ByAssemblyInvocation("clisettings.json");

                _conf = (IConfiguration)new ConfigurationBuilder()
                    .SetBasePath(file.DirectoryName)
                    .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                    .Build();

                var instance = new ContextService(InstanceContext.DeployedOrLocal);
                _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                    .Where(x => x.UserName == arg).ToLambda(),
                        new List<Expression<Func<tbl_Users, object>>>()
                        {
                            x => x.tbl_UserPasswords,
                            x => x.tbl_PrivateKeys,
                            x => x.tbl_PublicKeys
                        }).SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasOption("d|delete", "Delete a public/private key pair for user", arg =>
            {
                _delete = true;
            });

            HasOption("a|delete-all", "Delete all public/private key pairs for user", arg =>
            {
                _deleteAll = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (_delete)
                {
                    ConsoleHelper.StdOutKeyPairs(_user.tbl_PublicKeys.Where(x => x.Immutable == false));

                    Console.Out.Write("  *** Enter GUID of public key to delete *** : ");
                    var input = Guid.Parse(StandardInput.GetInput());

                    var key = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
                        .Where(x => x.UserId == _user.Id && x.Id == input).ToLambda())
                        .SingleOrDefault();

                    if(key != null)
                    {
                        _uow.PublicKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
                            .Where(x => x.UserId == _user.Id && !x.Immutable && x.Id == key.Id).ToLambda());

                        _uow.PrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKeys>()
                            .Where(x => x.UserId == _user.Id && !x.Immutable && x.Id == key.PrivateKeyId).ToLambda());

                        _uow.Commit();
                    }
                }
                else if (_deleteAll)
                {
                    ConsoleHelper.StdOutKeyPairs(_user.tbl_PublicKeys.Where(x => x.Immutable == false));

                    _uow.PublicKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
                        .Where(x => x.UserId == _user.Id && !x.Immutable).ToLambda());

                    _uow.PrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKeys>()
                        .Where(x => x.UserId == _user.Id && !x.Immutable).ToLambda());

                    _uow.Commit();
                }
                else
                {
                    ConsoleHelper.StdOutKeyPairs(_user.tbl_PublicKeys.Where(x => x.Immutable == false));
                }

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}