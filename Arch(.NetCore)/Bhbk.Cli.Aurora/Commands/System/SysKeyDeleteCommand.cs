using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands.System
{
    public class SysKeyDeleteCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private bool _deleteAll = false;

        public SysKeyDeleteCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-key-delete", "Delete public/private key for system");

            HasOption("d|delete-all", "Delete all public/private key pairs for system", arg =>
            {
                CheckRequiredArguments();

                _deleteAll = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var privKeys = _uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey_EF>()
                    .Where(x => x.UserId == null && x.IsDeletable == true).ToLambda());

                var pubKeys = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<PublicKey_EF>()
                    .Where(x => x.UserId == null && x.IsDeletable == true).ToLambda());

                if (pubKeys.Count() == 0)
                    throw new ConsoleHelpAsException($"  *** Found no public/private key pairs that are deletable ***");

                foreach (var foundPubKey in pubKeys.OrderBy(x => x.CreatedUtc))
                    FormatOutput.Write(foundPubKey, privKeys.Where(x => x.PublicKeyId == foundPubKey.Id).SingleOrDefault(), true);

                Console.Out.WriteLine();

                if (_deleteAll == true)
                {
                    Console.Out.Write("  *** Enter 'yes' to delete all public/private key pair(s) for system *** : ");
                    var input = StandardInput.GetInput();
                    Console.Out.WriteLine();

                    if (input.ToLower() == "yes")
                    {
                        _uow.PublicKeys.Delete(QueryExpressionFactory.GetQueryExpression<PublicKey_EF>()
                            .Where(x => x.UserId == null && x.IsDeletable == true).ToLambda());

                        _uow.Commit();

                        _uow.PrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<PrivateKey_EF>()
                            .Where(x => x.UserId == null && x.IsDeletable == true).ToLambda());

                        _uow.Commit();
                    }
                }
                else
                {
                    Console.Out.Write("  *** Enter GUID of public key to delete *** : ");
                    var input = Guid.Parse(StandardInput.GetInput());

                    var pubKey = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<PublicKey_EF>()
                        .Where(x => x.Id == input && x.IsDeletable == true).ToLambda())
                        .SingleOrDefault();

                    if (pubKey != null)
                    {
                        _uow.PrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<PrivateKey_EF>()
                            .Where(x => x.Id == pubKey.PrivateKeyId && x.IsDeletable == true).ToLambda());

                        _uow.Commit();

                        _uow.PublicKeys.Delete(QueryExpressionFactory.GetQueryExpression<PublicKey_EF>()
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
