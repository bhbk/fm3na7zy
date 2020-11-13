using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
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
    public class SysCredDeleteCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static Guid _credID;

        public SysCredDeleteCommands()
        {
            IsCommand("sys-cred-delete", "Delete system credential");

            HasOption("i|id=", "Enter GUID of credential to delete", arg =>
            {
                _credID = Guid.Parse(arg);
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
                var credentials = _uow.Credentials.Get(QueryExpressionFactory.GetQueryExpression<tbl_Credential>()
                    .Where(x => x.IsDeletable == true).ToLambda());

                ConsoleHelper.StdOutCredentials(credentials);

                if (_credID == Guid.Empty)
                {
                    Console.Out.WriteLine();
                    Console.Out.Write("  *** Enter GUID of credential to delete *** : ");
                    _credID = Guid.Parse(StandardInput.GetInput());
                }

                var mounts = _uow.UserMounts.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserMount>()
                    .Where(x => x.CredentialId == _credID).ToLambda());

                if (mounts.Any())
                {
                    Console.Out.WriteLine();
                    Console.Out.WriteLine("  *** The credential can not be deleted while in use ***");
                    Console.Out.WriteLine();
                    ConsoleHelper.StdOutUserMounts(mounts);

                    return StandardOutput.FondFarewell();
                }

                _uow.Credentials.Delete(QueryExpressionFactory.GetQueryExpression<tbl_Credential>()
                    .Where(x => x.Id == _credID && x.IsDeletable == true).ToLambda());

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
