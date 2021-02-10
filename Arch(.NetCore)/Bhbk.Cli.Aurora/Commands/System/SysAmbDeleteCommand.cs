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
    public class SysAmbDeleteCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;

        public SysAmbDeleteCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-cred-delete", "Delete credential for system");
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var exists = _uow.Ambassadors.Get(QueryExpressionFactory.GetQueryExpression<Ambassador_EF>()
                    .Where(x => x.IsDeletable == true).ToLambda());

                FormatOutput.Ambassadors(exists);

                Console.Out.WriteLine();
                Console.Out.Write("  *** Enter GUID of credential to delete *** : ");
                var input = Guid.Parse(StandardInput.GetInput());

                var mounts = _uow.Mounts.Get(QueryExpressionFactory.GetQueryExpression<Mount_EF>()
                    .Where(x => x.AmbassadorId == input).ToLambda());

                if (mounts.Any())
                {
                    Console.Out.WriteLine();
                    Console.Out.WriteLine("  *** The credential can not be deleted while in use ***");
                    FormatOutput.Mounts(mounts);

                    return StandardOutput.FondFarewell();
                }

                _uow.Ambassadors.Delete(exists.Where(x => x.Id == input));
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
