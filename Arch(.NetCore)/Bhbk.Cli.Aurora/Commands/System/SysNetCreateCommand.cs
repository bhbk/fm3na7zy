using Bhbk.Cli.Aurora.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Aurora.Primitives.Enums;
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
using System.Net;

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysNetCreateCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private IPNetwork _cidr;
        private Int32 _sequence;
        private NetworkActionType _actionType;
        private string _actionTypeList = string.Join(", ", Enum.GetNames(typeof(NetworkActionType)));

        public SysNetCreateCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-net-create", "Create allow/deny network for system");

            HasRequiredOption("s|sequence=", "Enter sequence value", arg =>
            {
                if (!Int32.TryParse(arg, out _sequence))
                    throw new ConsoleHelpAsException($"  *** Invalid sequence value ***");
            });

            HasRequiredOption("c|cidr=", "Enter CIDR address", arg =>
            {
                if (!IPNetwork.TryParse(arg, out _cidr))
                    throw new ConsoleHelpAsException($"  *** Invalid cidr address ***");
            });

            HasRequiredOption("a|action=", "Enter action to take", arg =>
            {
                if (!Enum.TryParse(arg, out _actionType))
                    throw new ConsoleHelpAsException($"  *** Invalid action type. Options are '{_actionTypeList}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var networks = _uow.Networks.Get(QueryExpressionFactory.GetQueryExpression<Network>()
                    .Where(x => x.IdentityId == null && x.IsEnabled == true).ToLambda());

                var exists = networks.Where(x => x.Address == _cidr.ToString())
                    .SingleOrDefault();

                if (exists != null)
                {
                    Console.Out.WriteLine("  *** The network entered already exists for user ***");
                    OutputFactory.StdOutNetworks(new List<Network> { exists });

                    return StandardOutput.FondFarewell();
                }

                var network = _uow.Networks.Create(
                    new Network
                    {
                        SequenceId = _sequence,
                        Address = _cidr.ToString(),
                        Action = _actionType.ToString(),
                        IsEnabled = true,
                        IsDeletable = true,
                    });

                _uow.Commit();

                OutputFactory.StdOutNetworks(new List<Network> { network });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
