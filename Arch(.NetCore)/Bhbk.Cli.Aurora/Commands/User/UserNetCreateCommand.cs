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
using System.Linq.Expressions;
using System.Net;

namespace Bhbk.Cli.Aurora.Commands.User
{
    public class UserNetCreateCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private Login_EF _user;
        private Int32 _sequence;
        private IPNetwork _cidr;
        private NetworkActionType_E _actionType;
        private string _actionTypeList = string.Join(", ", Enum.GetNames(typeof(NetworkActionType_E)));
        private string _comment;

        public UserNetCreateCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("user-net-create", "Create allow/deny network for user");

            HasRequiredOption("u|user=", "Enter user that already exists", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                    .Where(x => x.UserName == arg).ToLambda(),
                        new List<Expression<Func<Login_EF, object>>>()
                        {
                            x => x.Networks,
                        })
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

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

            HasRequiredOption("a|action=", "Enter action taken", arg =>
            {
                if (!Enum.TryParse(arg, out _actionType))
                    throw new ConsoleHelpAsException($"  *** Invalid action type. Options are '{_actionTypeList}' ***");
            });

            HasOption("c|comment=", "Enter comment", arg =>
            {
                CheckRequiredArguments();

                if (!string.IsNullOrEmpty(arg))
                    _comment = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var exists = _user.Networks.Where(x => x.Address == _cidr.ToString())
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
                        UserId = _user.UserId,
                        SequenceId = _sequence,
                        Address = _cidr.ToString(),
                        ActionTypeId = (int)_actionType,
                        Comment = _comment,
                        IsEnabled = true,
                        IsDeletable = false,
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
