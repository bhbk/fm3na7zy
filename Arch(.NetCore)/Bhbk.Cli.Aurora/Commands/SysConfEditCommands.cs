using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
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
    public class SysConfEditCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static string _configValue;
        private static ConfigType _configType;
        private static string _configTypeList = string.Join(", ", Enum.GetNames(typeof(ConfigType)));

        public SysConfEditCommands()
        {
            IsCommand("sys-conf-edit", "Edit system config key/value pairs");

            HasRequiredOption("k|key=", "Enter config key", arg =>
            {
                if (!Enum.TryParse(arg, out _configType))
                    throw new ConsoleHelpAsException($"*** Invalid config key type. Options are '{_configTypeList}' ***");
            });

            HasRequiredOption("v|value=", "Enter config value", arg =>
            {
                _configValue = arg;
            });

            var file = Search.ByAssemblyInvocation("clisettings.json");

            _conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(file.DirectoryName)
                .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var config = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<tbl_Settings>()
                    .Where(x => x.ConfigKey == _configType.ToString()).ToLambda())
                    .SingleOrDefault();

                if (config == null)
                    throw new ConsoleHelpAsException($"  *** Invalid config type '{_configType.ToString()}' ***");

                config.ConfigValue = _configValue;

                _uow.Settings.Update(config);
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
