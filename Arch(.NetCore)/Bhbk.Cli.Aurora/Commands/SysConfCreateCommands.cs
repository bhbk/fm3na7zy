using Bhbk.Cli.Aurora.Factories;
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

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysConfCreateCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private ConfigType _configType;
        private readonly string _configTypeList = string.Join(", ", Enum.GetNames(typeof(ConfigType)));
        private string _configValue;

        public SysConfCreateCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-conf-create", "Create config key/value pair");

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
                var exists = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<Setting>()
                    .Where(x => x.ConfigKey == _configType.ToString() && x.ConfigValue == _configValue).ToLambda())
                    .LastOrDefault();

                if (exists != null)
                {
                    Console.Out.WriteLine("  *** The config key/value pair entered already exists ***");
                    OutputFactory.StdOutSettings(new List<Setting> { exists });

                    return StandardOutput.FondFarewell();
                }

                var config = _uow.Settings.Create(
                    new Setting
                    {
                        ConfigKey = _configType.ToString(),
                        ConfigValue = _configValue,
                        IsDeletable = true,
                    });

                _uow.Commit();

                OutputFactory.StdOutSettings(new List<Setting> { config });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
