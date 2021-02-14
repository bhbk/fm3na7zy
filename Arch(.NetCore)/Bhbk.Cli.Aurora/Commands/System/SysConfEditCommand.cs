using Bhbk.Cli.Aurora.IO;
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
    public class SysConfEditCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private Setting_EF _config;
        private ConfigType_E _configType;
        private readonly string _configTypeList = string.Join(", ", Enum.GetNames(typeof(ConfigType_E)));
        private string _configValue;

        public SysConfEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-conf-edit", "Edit config key/value pair for system");

            HasRequiredOption("i|id=", "Enter GUID of config key/value pair to edit", arg =>
            {
                var id = Guid.Parse(arg);

                _config = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<Setting_EF>()
                    .Where(x => x.Id == id).ToLambda())
                    .SingleOrDefault();

                if (_config == null)
                    throw new ConsoleHelpAsException($"  *** Invalid config type '{_configType}' ***");
            });

            HasOption("k|key=", "Enter config key", arg =>
            {
                CheckRequiredArguments();

                if (!Enum.TryParse(arg, out _configType))
                    throw new ConsoleHelpAsException($"  *** Invalid config key type. Options are '{_configTypeList}' ***");
            });

            HasOption("v|value=", "Enter config value", arg =>
            {
                CheckRequiredArguments();

                _configValue = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (_configType.ToString() != null)
                    _config.ConfigKey = _configType.ToString();

                if (_configValue != null)
                    _config.ConfigValue = _configValue;

                _uow.Settings.Update(_config);
                _uow.Commit();

                FormatOutput.Write(_config);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
