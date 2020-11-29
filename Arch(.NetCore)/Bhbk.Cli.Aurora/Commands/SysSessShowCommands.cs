﻿using Bhbk.Cli.Aurora.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
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

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysSessShowCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private string _cidr, _user;

        public SysSessShowCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-sess-show", "Show sessions for system");

            HasOption("c|cidr=", "Enter CIDR address to search for", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _cidr = arg;
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
                List<string> remoteEndPoints;

                if (!string.IsNullOrEmpty(_user))
                    remoteEndPoints = _uow.Sessions.Get(QueryExpressionFactory.GetQueryExpression<Session>()
                        .Where(x => x.IdentityAlias.Contains(_user)).ToLambda())
                        .OrderBy(x => x.IdentityAlias)
                        .Select(x => x.RemoteEndPoint).Distinct().ToList();

                else if (!string.IsNullOrEmpty(_cidr))
                    remoteEndPoints = _uow.Sessions.Get(QueryExpressionFactory.GetQueryExpression<Session>()
                        .Where(x => x.RemoteEndPoint.Contains(_cidr)).ToLambda())
                        .OrderBy(x => x.IdentityAlias)
                        .Select(x => x.RemoteEndPoint).Distinct().ToList();

                else
                    remoteEndPoints = _uow.Sessions.Get()
                        .OrderBy(x => x.IdentityAlias)
                        .Select(x => x.RemoteEndPoint).Distinct().ToList();

                foreach (var remoteEndpoint in remoteEndPoints)
                {
                    var sessions = _uow.Sessions.Get(QueryExpressionFactory.GetQueryExpression<Session>()
                        .Where(x => x.RemoteEndPoint == remoteEndpoint).ToLambda());

                    var session = sessions.Where(x => x.IdentityAlias != null)
                        .FirstOrDefault();

                    Console.WriteLine();

                    if(session != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"  [User] {session.IdentityAlias} from {session.RemoteEndPoint}");
                        Console.ResetColor();
                    }

                    OutputFactory.StdOutSessions(sessions.OrderBy(x => x.CreatedUtc));
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
