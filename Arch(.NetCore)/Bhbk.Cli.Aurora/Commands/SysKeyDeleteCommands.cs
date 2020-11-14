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
    public class SysKeyDeleteCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private bool _delete = false, _deleteAll = false;

        public SysKeyDeleteCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-key-delete", "Delete private/public key for system");

            HasOption("d|delete", "Delete a public/private key pair for system", arg =>
            {
                _delete = true;
            });

            HasOption("a|delete-all", "Delete all public/private key pairs for system", arg =>
            {
                _deleteAll = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var keys = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKey>()
                    .Where(x => x.IdentityId == null && x.IsDeletable == false).ToLambda(),
                        new List<Expression<Func<tbl_PublicKey, object>>>()
                        {
                            x => x.PrivateKey,
                        });

                ConsoleHelper.StdOutKeyPairs(keys.OrderBy(x => x.CreatedUtc));

                if (_delete)
                {
                    Console.Out.WriteLine();
                    Console.Out.Write("  *** Enter GUID of public key to delete *** : ");
                    var input = Guid.Parse(StandardInput.GetInput());
                    Console.Out.WriteLine();

                    var pubKey = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKey>()
                        .Where(x => x.Id == input).ToLambda(),
                            new List<Expression<Func<tbl_PublicKey, object>>>()
                            {
                                x => x.PrivateKey,
                            }).SingleOrDefault();

                    if (pubKey != null)
                    {
                        _uow.PublicKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_PublicKey>()
                            .Where(x => x.Id == pubKey.Id).ToLambda());

                        _uow.PrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKey>()
                            .Where(x => x.Id == pubKey.PrivateKeyId).ToLambda());

                        _uow.Commit();
                    }
                }
                else if (_deleteAll)
                {
                    _uow.PublicKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_PublicKey>()
                        .Where(x => x.IdentityId == null && x.IsDeletable == false).ToLambda());

                    _uow.PrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKey>()
                        .Where(x => x.IdentityId == null && x.IsDeletable == false).ToLambda());

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
