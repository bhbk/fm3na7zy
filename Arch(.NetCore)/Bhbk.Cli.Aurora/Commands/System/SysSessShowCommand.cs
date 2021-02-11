using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.QueryExpression;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands.System
{
    public class SysSessShowCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private string _network, _user;
        private int _count;
        private bool _active = false;

        public SysSessShowCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-sess-show-all", "Show session details for user(s)");

            HasRequiredOption("c|count=", "Enter how many session groups to display", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _count = int.Parse(arg);
            });

            HasOption("u|user=", "Enter user to search for", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _user = arg;
            });

            HasOption("n|network=", "Enter CIDR address to search for", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _network = arg;
            });

            HasOption("a|active", "Show only active session(s)", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _active = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                IQueryExpression<Session_EF> expression =
                    QueryExpressionFactory.GetQueryExpression<Session_EF>();

                if (!string.IsNullOrEmpty(_network))
                    expression = expression.Where(x => x.RemoteEndPoint.Contains(_network));

                else if (!string.IsNullOrEmpty(_user))
                    expression = expression.Where(x => x.UserName.Contains(_user));

                if (_active)
                    expression = expression.Where(x => x.IsActive == true);

                var remotes = _uow.Sessions.Get(expression.ToLambda())
                    .OrderBy(x => x.UserName).ThenBy(x => x.CreatedUtc)
                    .Select(x => x.RemoteEndPoint).Distinct().TakeLast(_count).ToList();

                foreach (var remote in remotes)
                {
                    var sessions = _uow.Sessions.Get(QueryExpressionFactory.GetQueryExpression<Session_EF>()
                        .Where(x => x.RemoteEndPoint == remote).ToLambda());

                    Console.Out.WriteLine();
                    foreach (var session in sessions.OrderBy(x => x.CreatedUtc))
                        FormatOutput.Write(session, false);
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
