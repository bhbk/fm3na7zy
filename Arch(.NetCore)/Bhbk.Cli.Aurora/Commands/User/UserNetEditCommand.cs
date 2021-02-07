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
using System.Linq.Expressions;
using System.Net;

namespace Bhbk.Cli.Aurora.Commands.User
{
    public class UserNetEditCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private E_Login _user;
        private Guid _id;
        private bool? _isEnabled;
        private Int32 _sequence = Int32.MinValue;
        private IPNetwork _cidr = null;
        private NetworkActionType _actionType;
        private readonly string _actionTypeList = string.Join(", ", Enum.GetNames(typeof(NetworkActionType)));

        public UserNetEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("user-net-edit", "Edit allow/deny network for user");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                    .Where(x => x.UserName == arg).ToLambda(),
                        new List<Expression<Func<E_Login, object>>>()
                        {
                            x => x.Networks,
                        })
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasRequiredOption("i|id=", "Enter GUID of network to edit", arg =>
            {
                _id = Guid.Parse(arg);
            });

            HasOption("s|sequence=", "Enter sequence value", arg =>
            {
                if (!Int32.TryParse(arg, out _sequence))
                    throw new ConsoleHelpAsException($"  *** Invalid sequence value ***");
            });

            HasOption("c|cidr=", "Enter CIDR address", arg =>
            {
                if (!IPNetwork.TryParse(arg, out _cidr))
                    throw new ConsoleHelpAsException($"  *** Invalid cidr address ***");
            });

            HasOption("a|action=", "Enter type of action", arg =>
            {
                if (!Enum.TryParse(arg, out _actionType))
                    throw new ConsoleHelpAsException($"  *** Invalid auth type. Options are '{_actionTypeList}' ***");
            });

            HasOption("e|enabled=", "Is enabled", arg =>
            {
                _isEnabled = bool.Parse(arg);
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var network = _user.Networks.Where(x => x.Id == _id)
                    .SingleOrDefault();

                if (network == null)
                    throw new ConsoleHelpAsException($"  *** Invalid network GUID '{_id}' ***");

                if (_sequence != Int32.MinValue)
                    network.SequenceId = _sequence;

                if (_cidr != null)
                    network.Address = _cidr.ToString();

                if (_actionType.ToString() != null)
                    network.Action = _actionType.ToString();

                if (_isEnabled.HasValue)
                    network.IsEnabled = _isEnabled.Value;

                _uow.Networks.Update(network);
                _uow.Commit();

                FormatOutput.Networks(new List<E_Network> { network });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
