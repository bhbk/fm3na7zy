using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Aurora.Data_EF6.Models;
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

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserNetCreateCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private User _user;
        private IPNetwork _cidr;
        private NetworkAction _actionType;
        private readonly string _actionTypeList = string.Join(", ", Enum.GetNames(typeof(NetworkAction)));

        public UserNetCreateCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("user-net-create", "Create allow/deny for user network");

            HasRequiredOption("u|user=", "Enter user that already exists", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                    .Where(x => x.IdentityAlias == arg).ToLambda(),
                        new List<Expression<Func<User, object>>>()
                        {
                            x => x.Mount,
                        }).SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasRequiredOption("c|cidr=", "Enter CIDR address to use", arg =>
            {
                if(!IPNetwork.TryParse(arg, out _cidr))
                    throw new ConsoleHelpAsException($"*** Invalid cidr address ***");
            });

            HasRequiredOption("a|action=", "Enter type of action to use", arg =>
            {
                if (!Enum.TryParse(arg, out _actionType))
                    throw new ConsoleHelpAsException($"*** Invalid auth type. Options are '{_actionTypeList}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var network = _uow.Networks.Create(
                    new Network
                    {
                        Id = Guid.NewGuid(),
                        IdentityId = _user.IdentityId,
                        Address = _cidr.ToString(),
                        Action = _actionType.ToString(),
                        IsEnabled = true,
                        CreatedUtc = DateTime.UtcNow,
                    });
                _uow.Commit();

                ConsoleHelper.StdOutNetworks(new List<Network>() { network });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
