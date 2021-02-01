using Bhbk.Cli.Aurora.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Encryption;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysCredEditCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private Guid _id;
        private string _credLogin;

        public SysCredEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], instance);

            IsCommand("sys-cred-edit", "Edit credential for system");

            HasRequiredOption("i|id=", "Enter GUID of credential to edit", arg =>
            {
                _id = Guid.Parse(arg);
            });

            HasOption("l|login=", "Enter login", arg =>
            {
                _credLogin = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var ambassador = _uow.Ambassadors.Get(QueryExpressionFactory.GetQueryExpression<E_Ambassador>()
                    .Where(x => x.Id == _id).ToLambda())
                    .SingleOrDefault();

                if (ambassador == null)
                    throw new ConsoleHelpAsException($"  *** Invalid credential GUID '{_id}' ***");

                Console.Out.Write("  *** Enter credential password to use *** : ");
                var credPass = StandardInput.GetHiddenInput();

                Console.Out.WriteLine();

                var secret = _conf["Databases:AuroraSecret"];
                var encryptedPass = AES.EncryptString(credPass, secret);
                var decryptedPass = AES.DecryptString(encryptedPass, secret);

                if (credPass != decryptedPass)
                    throw new ArithmeticException();

                if (_credLogin != null)
                    ambassador.UserName = _credLogin;

                if (credPass != null)
                    ambassador.EncryptedPass = encryptedPass;

                _uow.Ambassadors.Update(ambassador);
                _uow.Commit();

                StandardOutputFactory.Ambassadors(new List<E_Ambassador> { ambassador });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
