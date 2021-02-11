﻿using Bhbk.Cli.Aurora.IO;
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

namespace Bhbk.Cli.Aurora.Commands.System
{
    public class SysConfCreateCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private ConfigType_E _configType;
        private readonly string _configTypeList = string.Join(", ", Enum.GetNames(typeof(ConfigType_E)));
        private string _configValue;

        public SysConfCreateCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-conf-create", "Create config key/value pair for system");

            HasRequiredOption("k|key=", "Enter config key", arg =>
            {
                if (!Enum.TryParse(arg, out _configType))
                    throw new ConsoleHelpAsException($"  *** Invalid config key type. Options are '{_configTypeList}' ***");
            });

            HasRequiredOption("v|value=", "Enter config value", arg =>
            {
                _configValue = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var found = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<Setting_EF>()
                    .Where(x => x.ConfigKey == _configType.ToString() && x.ConfigValue == _configValue).ToLambda())
                    .LastOrDefault();

                if (found != null)
                {
                    Console.Out.WriteLine("  *** The config key/value pair entered already exists ***");
                    FormatOutput.Write(found, true);

                    return StandardOutput.FondFarewell();
                }

                var config = _uow.Settings.Create(
                    new Setting_EF
                    {
                        ConfigKey = _configType.ToString(),
                        ConfigValue = _configValue,
                        IsDeletable = true,
                    });

                _uow.Commit();

                FormatOutput.Write(config, true);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
