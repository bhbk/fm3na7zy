using Bhbk.Cli.Aurora.Helpers;
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
    public class KeyDeleteCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static tbl_Users _user;
        private static bool _delete = false, _deleteAll = false;

        public KeyDeleteCommands()
        {
            IsCommand("delete-key", "Delete user public/private key pairs");

            HasRequiredOption("u|user=", "Enter existing user", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                var file = SearchRoots.ByAssemblyContext("clisettings.json");

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
                            x => x.tbl_UserPrivateKeys,
                            x => x.tbl_UserPublicKeys
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
                    ConsoleHelper.OutUserPublicKeyPairs(_user.tbl_UserPublicKeys.Where(x => !x.Immutable));

                    Console.Out.Write("  *** Enter GUID of public key to delete *** : ");
                    var input = StandardInput.GetInput();

                    var key = _uow.UserPublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserPublicKeys>()
                        .Where(x => x.UserId == _user.Id && x.Id.ToString() == input).ToLambda()).SingleOrDefault();

                    if(key != null)
                    {
                        _uow.UserPublicKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_UserPublicKeys>()
                            .Where(x => x.UserId == _user.Id && !x.Immutable && x.Id == key.Id).ToLambda());

                        _uow.UserPrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_UserPrivateKeys>()
                            .Where(x => x.UserId == _user.Id && !x.Immutable && x.Id == key.PrivateKeyId).ToLambda());

                        _uow.Commit();
                    }
                }
                else if (_deleteAll)
                {
                    ConsoleHelper.OutUserPublicKeyPairs(_user.tbl_UserPublicKeys.Where(x => !x.Immutable));

                    _uow.UserPublicKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_UserPublicKeys>()
                        .Where(x => x.UserId == _user.Id && !x.Immutable).ToLambda());

                    _uow.UserPrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_UserPrivateKeys>()
                        .Where(x => x.UserId == _user.Id && !x.Immutable).ToLambda());

                    _uow.Commit();
                }
                else
                {
                    ConsoleHelper.OutUserPublicKeyPairs(_user.tbl_UserPublicKeys.Where(x => !x.Immutable));
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
