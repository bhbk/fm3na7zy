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
    public class SysCredCreateCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private string _credLogin, _credPass;

        public SysCredCreateCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-cred-create", "Create credential for system");

            HasRequiredOption("l|login=", "Enter login", arg =>
            {
                _credLogin = arg;
            });

            HasOption("p|pass=", "Enter password", arg =>
            {
                _credPass = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var exists = _uow.Ambassadors.Get(QueryExpressionFactory.GetQueryExpression<E_Ambassador>()
                    .Where(x => x.UserName == _credLogin).ToLambda())
                    .SingleOrDefault();

                if (exists != null)
                {
                    Console.Out.WriteLine("  *** The credential entered already exists ***");
                    StandardOutputFactory.Credentials(new List<E_Ambassador> { exists });

                    return StandardOutput.FondFarewell();
                }

                if (string.IsNullOrEmpty(_credPass))
                {
                    Console.Out.Write("  *** Enter credential password to use *** : ");
                    _credPass = StandardInput.GetHiddenInput();
                    Console.Out.WriteLine();
                }

                var secret = _conf["Databases:AuroraSecret"];
                var cipherText = AES.EncryptString(_credPass, secret);
                var plainText = AES.DecryptString(cipherText, secret);

                if (_credPass != plainText)
                    throw new ArithmeticException();

                var ambassador = _uow.Ambassadors.Create(
                    new E_Ambassador
                    {
                        UserName = _credLogin,
                        EncryptedPass = cipherText,
                        IsEnabled = true,
                        IsDeletable = true,
                    });

                _uow.Commit();

                StandardOutputFactory.Credentials(new List<E_Ambassador> { ambassador });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
