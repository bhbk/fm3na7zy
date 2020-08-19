using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Primitives.Enums;
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
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static tbl_User _user;
        private static IPNetwork _cidr;
        private static NetworkAction _actionType;
        private static string _actionTypeList = string.Join(", ", Enum.GetNames(typeof(NetworkAction)));

        public UserNetCreateCommands()
        {
            IsCommand("user-net-create", "Create allow/deny for user network");

            HasRequiredOption("u|user=", "Enter user that already exists", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var instance = new ContextService(InstanceContext.DeployedOrLocal);
                _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                    .Where(x => x.IdentityAlias == arg).ToLambda(),
                        new List<Expression<Func<tbl_User, object>>>()
                        {
                            x => x.tbl_UserMount,
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
                _uow.Networks.Create(
                    new tbl_Network
                    {
                        Id = Guid.NewGuid(),
                        IdentityId = _user.IdentityId,
                        Address = _cidr.ToString(),
                        Action = _actionType.ToString(),
                        Enabled = true,
                        Created = DateTime.UtcNow,
                    });

                _uow.Commit();

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
