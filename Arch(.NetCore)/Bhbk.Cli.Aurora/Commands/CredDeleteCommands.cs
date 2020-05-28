using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
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
    public class CredDeleteCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static Guid _credId;

        public CredDeleteCommands()
        {
            IsCommand("delete-credential", "Delete system credential");

            HasOption("i|id=", "Enter GUID of credential to delete", arg =>
            {
                _credId = Guid.Parse(arg);
            });

            var file = SearchRoots.ByAssemblyContext("clisettings.json");

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
                var credentials = _uow.SysCredentials.Get();

                ConsoleHelper.OutSysCredentials(credentials);

                if (_credId == Guid.Empty)
                {
                    Console.Out.Write("  *** Enter GUID of credential to delete *** : ");
                    _credId = Guid.Parse(StandardInput.GetInput());

                    Console.Out.WriteLine();
                }

                var mounts = _uow.UserMounts.Get(x => x.CredentialId == _credId);

                if (mounts.Any())
                {
                    Console.Out.WriteLine("  *** The credential can not be deleted while in use ***");
                    Console.Out.WriteLine();
                    ConsoleHelper.OutUserMounts(mounts);

                    return StandardOutput.FondFarewell();
                }

                _uow.SysCredentials.Delete(QueryExpressionFactory.GetQueryExpression<tbl_SysCredentials>()
                    .Where(x => x.Id == _credId && !x.Immutable).ToLambda());

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
