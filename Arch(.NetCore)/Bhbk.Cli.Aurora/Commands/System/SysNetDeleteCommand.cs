using Bhbk.Cli.Aurora.IO;
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

namespace Bhbk.Cli.Aurora.Commands.System
{
    public class SysNetDeleteCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private bool _deleteAll = false;

        public SysNetDeleteCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-net-delete", "Delete allow/deny network for system");

            HasOption("d|delete-all", "Delete all networks for system", arg =>
            {
                _deleteAll = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var networks = _uow.Networks.Get(QueryExpressionFactory.GetQueryExpression<E_Network>()
                    .Where(x => x.UserId == null && x.IsDeletable == true).ToLambda());

                FormatOutput.Networks(networks);
                Console.Out.WriteLine();

                if (_deleteAll == true)
                {
                    Console.Out.Write("  *** Enter yes to delete all networks for system *** : ");
                    var input = StandardInput.GetInput();

                    if (input.ToLower() == "yes")
                    {
                        _uow.Networks.Delete(networks);
                        _uow.Commit();
                    }
                }
                else
                {
                    Console.Out.Write("  *** Enter GUID of network for system to delete *** : ");
                    var input = Guid.Parse(StandardInput.GetInput());

                    var network = networks.Where(x => x.Id == input)
                        .SingleOrDefault();

                    if (network != null)
                    {
                        _uow.Networks.Delete(network);
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
