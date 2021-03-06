﻿using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysConfDeleteCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static Guid _configID;

        public SysConfDeleteCommands()
        {
            IsCommand("sys-conf-delete", "Delete config key/value pair");

            HasOption("i|id=", "Enter GUID of config to delete", arg =>
            {
                _configID = Guid.Parse(arg);
            });

            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var configs = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                    .Where(x => x.Deletable == true).ToLambda());

                ConsoleHelper.StdOutSettings(configs);

                if (_configID == Guid.Empty)
                {
                    Console.Out.Write("  *** Enter GUID of config to delete *** : ");
                    _configID = Guid.Parse(StandardInput.GetInput());
                }

                _uow.Settings.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                    .Where(x => x.Id == _configID && x.Deletable == true).ToLambda());

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
