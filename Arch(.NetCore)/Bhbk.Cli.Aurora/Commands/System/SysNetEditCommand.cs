using Bhbk.Cli.Aurora.IO;
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

namespace Bhbk.Cli.Aurora.Commands.System
{
    public class SysNetEditCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private Guid _id;
        private Int32 _sequence = Int32.MinValue;
        private IPNetwork _cidr = null;
        private NetworkActionType_E _actionType;
        private readonly string _actionTypeList = string.Join(", ", Enum.GetNames(typeof(NetworkActionType_E)));

        public SysNetEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-net-edit", "Edit allow/deny network for system");

            HasRequiredOption("i|id=", "Enter GUID of network to edit", arg =>
            {
                _id = Guid.Parse(arg);
            });

            HasOption("s|sequence=", "Enter sequence value", arg =>
            {
                if (!Int32.TryParse(arg, out _sequence))
                    throw new ConsoleHelpAsException($"  *** Invalid sequence value ***");
            });

            HasOption("c|cidr=", "Enter CIDR address to use", arg =>
            {
                if (!IPNetwork.TryParse(arg, out _cidr))
                    throw new ConsoleHelpAsException($"  *** Invalid cidr address ***");
            });

            HasOption("a|action=", "Enter type of action to use", arg =>
            {
                if (!Enum.TryParse(arg, out _actionType))
                    throw new ConsoleHelpAsException($"  *** Invalid action type. Options are '{_actionTypeList}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var networks = _uow.Networks.Get(QueryExpressionFactory.GetQueryExpression<Network_EF>()
                    .Where(x => x.UserId == null).ToLambda());

                var network = networks.Where(x => x.Id == _id)
                    .SingleOrDefault();

                if (network == null)
                    throw new ConsoleHelpAsException($"*** Invalid network GUID '{_id}' ***");

                if (_sequence != Int32.MinValue)
                    network.SequenceId = _sequence;

                if (_cidr != null)
                    network.Address = _cidr.ToString();

                if (_actionType.ToString() != null)
                    network.Action = _actionType.ToString();

                _uow.Networks.Update(network);
                _uow.Commit();

                FormatOutput.Networks(new List<Network_EF> { network });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
