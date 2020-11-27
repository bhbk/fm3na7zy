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
    public class SysNetDeleteCommands : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private bool _delete = false, _deleteAll = false;

        public SysNetDeleteCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-net-delete", "Delete allow/deny network for system");

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
                var networks = _uow.Networks.Get(QueryExpressionFactory.GetQueryExpression<Network>()
                    .Where(x => x.IdentityId == null && x.IsDeletable == true).ToLambda());

                OutputFactory.StdOutNetworks(networks);

                if (_delete)
                {
                    Console.Out.WriteLine();
                    Console.Out.Write("  *** Enter GUID of network for user to delete *** : ");
                    var input = Guid.Parse(StandardInput.GetInput());

                    var network = networks.Where(x => x.Id == input)
                        .SingleOrDefault();

                    if (network != null)
                    {
                        _uow.Networks.Delete(network);
                        _uow.Commit();
                    }
                }
                else if (_deleteAll)
                {
                    _uow.Networks.Delete(networks);
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
