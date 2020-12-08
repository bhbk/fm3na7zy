using Bhbk.Cli.Aurora.Factories;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
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
    public class SysConfEditCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private Guid _id;
        private ConfigType _configType;
        private readonly string _configTypeList = string.Join(", ", Enum.GetNames(typeof(ConfigType)));
        private string _configValue;

        public SysConfEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-conf-edit", "Edit config key/value pair for system");

            HasRequiredOption("i|id=", "Enter GUID of config key/value pair to edit", arg =>
            {
                _id = Guid.Parse(arg);
            });

            HasOption("k|key=", "Enter config key", arg =>
            {
                if (!Enum.TryParse(arg, out _configType))
                    throw new ConsoleHelpAsException($"  *** Invalid config key type. Options are '{_configTypeList}' ***");
            });

            HasOption("v|value=", "Enter config value", arg =>
            {
                _configValue = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var config = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<E_Setting>()
                    .Where(x => x.Id == _id).ToLambda())
                    .SingleOrDefault();

                if (config == null)
                    throw new ConsoleHelpAsException($"  *** Invalid config type '{_configType}' ***");

                if (_configType.ToString() != null)
                    config.ConfigKey = _configType.ToString();

                if (_configValue != null)
                    config.ConfigValue = _configValue;

                _uow.Settings.Update(config);
                _uow.Commit();

                StandardOutputFactory.Settings(new List<E_Setting> { config });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
