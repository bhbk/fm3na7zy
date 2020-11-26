using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data_EF6.Infrastructure;
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

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysConfEditCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private ConfigType _configType;
        private readonly string _configTypeList = string.Join(", ", Enum.GetNames(typeof(ConfigType)));
        private string _configValue;

        public SysConfEditCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-conf-edit", "Edit config key/value pair");

            HasRequiredOption("k|key=", "Enter config key", arg =>
            {
                if (!Enum.TryParse(arg, out _configType))
                    throw new ConsoleHelpAsException($"*** Invalid config key type. Options are '{_configTypeList}' ***");
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
                var config = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<Setting>()
                    .Where(x => x.ConfigKey == _configType.ToString()).ToLambda())
                    .SingleOrDefault();

                if (config == null)
                    throw new ConsoleHelpAsException($"  *** Invalid config type '{_configType}' ***");

                config.ConfigValue = _configValue;

                config = _uow.Settings.Update(config);
                _uow.Commit();

                ConsoleHelper.StdOutSettings(new List<Setting>() { config });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
