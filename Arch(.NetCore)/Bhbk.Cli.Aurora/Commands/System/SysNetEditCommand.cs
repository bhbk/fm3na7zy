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
        private bool? _isEnabled;
        private IPNetwork _cidr = null;
        private NetworkActionType_E _actionType;
        private readonly string _actionTypeList = string.Join(", ", Enum.GetNames(typeof(NetworkActionType_E)));
        private string _comment;

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
                CheckRequiredArguments();

                if (!Int32.TryParse(arg, out _sequence))
                    throw new ConsoleHelpAsException($"  *** Invalid sequence value ***");
            });

            HasOption("n|network=", "Enter CIDR address to use", arg =>
            {
                CheckRequiredArguments();

                if (!IPNetwork.TryParse(arg, out _cidr))
                    throw new ConsoleHelpAsException($"  *** Invalid cidr address ***");
            });

            HasOption("a|action=", "Enter type of action to use", arg =>
            {
                CheckRequiredArguments();

                if (!Enum.TryParse(arg, out _actionType))
                    throw new ConsoleHelpAsException($"  *** Invalid action type. Options are '{_actionTypeList}' ***");
            });

            HasOption("c|comment=", "Enter comment", arg =>
            {
                CheckRequiredArguments();

                if (!string.IsNullOrEmpty(arg))
                    _comment = arg;
            });

            HasOption("e|enabled=", "Is enabled?", arg =>
            {
                CheckRequiredArguments();

                _isEnabled = bool.Parse(arg);
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
                    network.ActionTypeId = (int)_actionType;

                if (_comment != null)
                    network.Comment = _comment;

                if (_isEnabled.HasValue)
                    network.IsEnabled = _isEnabled.HasValue;

                _uow.Networks.Update(network);
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
