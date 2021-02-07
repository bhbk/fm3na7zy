using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
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
        private bool _active = false;

        public SysSessShowCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-sess-show", "Show session(s) on system");

            HasOption("a|active", "Show only active session(s)", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _active = true;
            });

            HasOption("n|network=", "Enter CIDR address to search for", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _network = arg;
            });

            HasOption("u|user=", "Enter user to search for", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _user = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                IQueryExpression<E_Session> expr;

                if (!string.IsNullOrEmpty(_network))
                {
                    if (_active)
                        expr = QueryExpressionFactory.GetQueryExpression<E_Session>()
                            .Where(x => x.RemoteEndPoint.Contains(_network) && x.IsActive == true);
                    else
                        expr = QueryExpressionFactory.GetQueryExpression<E_Session>()
                            .Where(x => x.RemoteEndPoint.Contains(_network));
                }
                else if (!string.IsNullOrEmpty(_user))
                {
                    if (_active)
                        expr = QueryExpressionFactory.GetQueryExpression<E_Session>()
                            .Where(x => x.UserName.Contains(_user) && x.IsActive == true);
                    else
                        expr = QueryExpressionFactory.GetQueryExpression<E_Session>()
                            .Where(x => x.UserName.Contains(_user));
                }
                else
                {
                    if (_active)
                        expr = QueryExpressionFactory.GetQueryExpression<E_Session>()
                            .Where(x => x.IsActive == true);
                    else
                        expr = QueryExpressionFactory.GetQueryExpression<E_Session>();
                }

                var remotes = _uow.Sessions.Get(expr.ToLambda())
                    .OrderBy(x => x.UserName).ThenBy(x => x.CreatedUtc)
                    .Select(x => x.RemoteEndPoint).Distinct().TakeLast(100).ToList();

                foreach (var remote in remotes)
                {
                    var sessions = _uow.Sessions.Get(QueryExpressionFactory.GetQueryExpression<E_Session>()
                        .Where(x => x.RemoteEndPoint == remote).ToLambda());

                    Console.Out.WriteLine();
                    FormatOutput.Sessions(sessions
                        .OrderBy(x => x.CreatedUtc), true);
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
