using Bhbk.Cli.Aurora.IO;
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

namespace Bhbk.Cli.Aurora.Commands.System
{
    public class SysConfDeleteCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;

        public SysConfDeleteCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-conf-delete", "Delete config key/value pair for system");
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var configs = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<E_Setting>()
                    .Where(x => x.UserId == null && x.IsDeletable == true).ToLambda());

                FormatOutput.Settings(configs);

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

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
