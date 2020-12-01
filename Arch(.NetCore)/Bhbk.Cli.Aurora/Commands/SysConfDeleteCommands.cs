using Bhbk.Cli.Aurora.Factories;
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
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysConfDeleteCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private bool _delete = false, _deleteAll = false;

        public SysConfDeleteCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-conf-delete", "Delete config key/value pair");

            HasOption("d|delete", "Delete a config key/value pair", arg =>
            {
                _delete = true;
            });

            HasOption("a|delete-all", "Delete all config key/value pairs", arg =>
            {
                _deleteAll = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var configs = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<Setting>()
                    .Where(x => x.IdentityId == null && x.IsDeletable == true).ToLambda());

                OutputFactory.StdOutSettings(configs);

                if (_delete)
                {
                    Console.Out.WriteLine();
                    Console.Out.Write("  *** Enter GUID of config key/value pair to delete *** : ");
                    var input = Guid.Parse(StandardInput.GetInput());

                    var config = configs.Where(x => x.Id == input)
                        .SingleOrDefault();

                    if (config != null)
                    {
                        _uow.Settings.Delete(config);
                        _uow.Commit();
                    }
                }
                else if (_deleteAll)
                {
                    _uow.Settings.Delete(configs);
                    _uow.Commit();
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
