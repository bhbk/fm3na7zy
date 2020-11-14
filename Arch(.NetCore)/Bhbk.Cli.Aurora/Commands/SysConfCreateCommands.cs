using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

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
                var config = _uow.Settings.Create(
                    new tbl_Setting
                    {
                        Id = Guid.NewGuid(),
                        ConfigKey = _configType.ToString(),
                        ConfigValue = _configValue,
                        IsDeletable = true,
                        CreatedUtc = DateTime.UtcNow,
                    });
                _uow.Commit();

                ConsoleHelper.StdOutSettings(new List<tbl_Setting>() { config });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
