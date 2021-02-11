using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
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

namespace Bhbk.Cli.Aurora.Commands.System
{
    public class SysNetCreateCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private IPNetwork _cidr;
        private Int32 _sequence;
        private NetworkActionType_E _actionType;
        private string _actionTypeList = string.Join(", ", Enum.GetNames(typeof(NetworkActionType_E)));
        private string _comment;

        public SysNetCreateCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-net-create", "Create allow/deny network for system");

            HasRequiredOption("s|sequence=", "Enter sequence value", arg =>
            {
                if (!Int32.TryParse(arg, out _sequence))
                    throw new ConsoleHelpAsException($"  *** Invalid sequence value ***");
            });

            HasRequiredOption("n|network=", "Enter CIDR address", arg =>
            {
                if (!IPNetwork.TryParse(arg, out _cidr))
                    throw new ConsoleHelpAsException($"  *** Invalid cidr address ***");
            });

            HasRequiredOption("a|action=", "Enter action to take", arg =>
            {
                if (!Enum.TryParse(arg, out _actionType))
                    throw new ConsoleHelpAsException($"  *** Invalid action type. Options are '{_actionTypeList}' ***");
            });

            HasOption("c|comment=", "Enter comment", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _comment = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var networks = _uow.Networks.Get(QueryExpressionFactory.GetQueryExpression<Network_EF>()
                    .Where(x => x.UserId == null && x.IsEnabled == true).ToLambda());

                var exists = networks.Where(x => x.Address == _cidr.ToString())
                    .SingleOrDefault();

                if (exists != null)
                {
                    Console.Out.WriteLine("  *** The network entered already exists for user ***");
                    FormatOutput.Write(exists, true);

                    return StandardOutput.FondFarewell();
                }

                var network = _uow.Networks.Create(
                    new Network_EF
                    {
                        SequenceId = _sequence,
                        Address = _cidr.ToString(),
                        ActionTypeId = (int)_actionType,
                        Comment = _comment,
                        IsEnabled = true,
                        IsDeletable = true,
                    });

                _uow.Commit();

                FormatOutput.Write(network, true);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
