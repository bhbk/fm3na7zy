using Bhbk.Cli.Aurora.Helpers;
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
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserNetDeleteCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static tbl_User _user;
        private static bool _delete = false, _deleteAll = false;

        public UserNetDeleteCommands()
        {
            IsCommand("user-net-delete", "Delete allow/deny for user network");

            HasRequiredOption("u|user=", "Enter existing user", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var instance = new ContextService(InstanceContext.DeployedOrLocal);
                _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                    .Where(x => x.IdentityAlias == arg).ToLambda(),
                        new List<Expression<Func<tbl_User, object>>>()
                        {
                            x => x.tbl_PrivateKeys,
                            x => x.tbl_PublicKeys,
                        }).SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasOption("d|delete", "Delete a network for user", arg =>
            {
                _delete = true;
            });

            HasOption("a|delete-all", "Delete all networks for user", arg =>
            {
                _deleteAll = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var nets = _uow.Networks.Get(QueryExpressionFactory.GetQueryExpression<tbl_Network>()
                    .Where(x => x.IdentityId == _user.IdentityId).ToLambda());

                ConsoleHelper.StdOutNetworks(nets);

                if (_delete)
                {
                    Console.Out.Write("  *** Enter GUID of network to delete *** : ");
                    var input = Guid.Parse(StandardInput.GetInput());

                    var key = _uow.Networks.Get(QueryExpressionFactory.GetQueryExpression<tbl_Network>()
                        .Where(x => x.IdentityId == _user.IdentityId && x.Id == input).ToLambda())
                        .SingleOrDefault();

                    if(key != null)
                    {
                        _uow.Networks.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Network>()
                            .Where(x => x.IdentityId == _user.IdentityId && x.Id == key.Id).ToLambda());

                        _uow.Commit();
                    }
                }
                else if (_deleteAll)
                {
                    _uow.Networks.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Network>()
                        .Where(x => x.IdentityId == _user.IdentityId).ToLambda());

                    _uow.Commit();
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
