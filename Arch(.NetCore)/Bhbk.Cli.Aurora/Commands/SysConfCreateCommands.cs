using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysConfCreateCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static ConfigType _configType;
        private static string _configValue;
        private static string _configTypeList = string.Join(", ", Enum.GetNames(typeof(ConfigType)));

        public SysConfCreateCommands()
        {
            IsCommand("sys-conf-create", "Create system config key/value pairs");

            HasRequiredOption("k|key=", "Enter config key", arg =>
            {
                if (!Enum.TryParse(arg, out _configType))
                    throw new ConsoleHelpAsException($"*** Invalid config key type. Options are '{_configTypeList}' ***");
            });

            HasRequiredOption("v|value=", "Enter config value", arg =>
            {
                _configValue = arg;
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
                _uow.Settings.Create(
                    new tbl_Settings
                    {
                        Id = Guid.NewGuid(),
                        ConfigKey = _configType.ToString(),
                        ConfigValue = _configValue,
                        Created = DateTime.UtcNow,
                        Immutable = false,
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
