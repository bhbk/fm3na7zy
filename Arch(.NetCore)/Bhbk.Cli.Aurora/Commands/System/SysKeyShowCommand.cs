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
    public class SysKeyShowCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;

        public SysKeyShowCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-key-show", "Show public/private key(s) for system");
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var pubKeys = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<E_PublicKey>()
                    .Where(x => x.UserId == null).ToLambda());

                var privKeys = _uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<E_PrivateKey>()
                    .Where(x => x.UserId == null).ToLambda());

                StandardOutputFactory.KeyPairs(pubKeys.OrderBy(x => x.CreatedUtc), privKeys);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
