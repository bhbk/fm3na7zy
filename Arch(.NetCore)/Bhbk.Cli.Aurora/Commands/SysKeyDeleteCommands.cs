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
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysKeyDeleteCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static bool _delete = false, _deleteAll = false;

        public SysKeyDeleteCommands()
        {
            IsCommand("sys-key-delete", "Delete system public/private key pairs");

            HasOption("d|delete", "Delete a public/private key pair for system", arg =>
            {
                _delete = true;
            });

            HasOption("a|delete-all", "Delete all public/private key pairs for system", arg =>
            {
                _deleteAll = true;
            });

            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var keys = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
                    .Where(x => x.IdentityId == null && x.Immutable == true).ToLambda()).OrderBy(x => x.Created).ToList();

                if (_delete)
                {
                    ConsoleHelper.StdOutKeyPairs(keys);

                    Console.Out.Write("  *** Enter GUID of public key to delete *** : ");
                    var input = Guid.Parse(StandardInput.GetInput());

                    var pubKey = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
                        .Where(x => x.Id == input).ToLambda()).SingleOrDefault();

                    if (pubKey != null)
                    {
                        _uow.PublicKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
                            .Where(x => x.Id == pubKey.Id).ToLambda());

                        _uow.PrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKeys>()
                            .Where(x => x.Id == pubKey.PrivateKeyId).ToLambda());

                        _uow.Commit();
                    }
                }
                else if (_deleteAll)
                {
                    ConsoleHelper.StdOutKeyPairs(keys);

                    _uow.PublicKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
                        .Where(x => x.IdentityId == null && x.Immutable == true).ToLambda());

                    _uow.PrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKeys>()
                        .Where(x => x.IdentityId == null && x.Immutable == true).ToLambda());

                    _uow.Commit();
                }
                else
                    ConsoleHelper.StdOutKeyPairs(keys);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
