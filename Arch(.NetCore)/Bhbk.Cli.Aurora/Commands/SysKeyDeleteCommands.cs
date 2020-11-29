using Bhbk.Cli.Aurora.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
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
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private bool _deleteAll = false;

        public SysKeyDeleteCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-key-delete", "Delete public/private key pair for system");

            HasOption("d|delete-all", "Delete all public/private key pair(s) for system", arg =>
            {
                _deleteAll = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var privKeys = _uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey>()
                    .Where(x => x.IdentityId == null && x.IsDeletable == true).ToLambda());

                var pubKeys = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<PublicKey>()
                    .Where(x => x.IdentityId == null && x.IsDeletable == true).ToLambda());

                OutputFactory.StdOutKeyPairs(pubKeys.OrderBy(x => x.CreatedUtc), privKeys);
                Console.Out.WriteLine();

                if (_deleteAll == true)
                {
                    Console.Out.Write("  *** Enter yes to delete all public/private key pair(s) for system *** : ");
                    var input = StandardInput.GetInput();

                    if (input.ToLower() == "yes")
                    {
                        _uow.PublicKeys.Delete(QueryExpressionFactory.GetQueryExpression<PublicKey>()
                            .Where(x => x.IdentityId == null && x.IsDeletable == true).ToLambda());

                        _uow.Commit();

                        _uow.PrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<PrivateKey>()
                            .Where(x => x.IdentityId == null && x.IsDeletable == true).ToLambda());

                        _uow.Commit();
                    }
                }
                else
                {
                    Console.Out.Write("  *** Enter GUID of public key to delete *** : ");
                    var input = Guid.Parse(StandardInput.GetInput());

                    var pubKey = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<PublicKey>()
                        .Where(x => x.Id == input && x.IsDeletable == true).ToLambda())
                        .SingleOrDefault();

                    if (pubKey != null)
                    {
                        _uow.PrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<PrivateKey>()
                            .Where(x => x.Id == pubKey.PrivateKeyId && x.IsDeletable == true).ToLambda());

                        _uow.Commit();

                        _uow.PublicKeys.Delete(QueryExpressionFactory.GetQueryExpression<PublicKey>()
                            .Where(x => x.Id == pubKey.Id && x.IsDeletable == true).ToLambda());

                        _uow.Commit();
                    }
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
