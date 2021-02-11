using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
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

namespace Bhbk.Cli.Aurora.Commands.System
{
    public class SysAmbCreateCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private string _credLogin, _credPass;

        public SysAmbCreateCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-amb-create", "Create ambassador credential on system");

            HasRequiredOption("u|upn=", "Enter user principal name that does not exist already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user principal name given ***");

                var ambassador = _uow.Ambassadors.Get(QueryExpressionFactory.GetQueryExpression<Ambassador_EF>()
                    .Where(x => x.UserPrincipalName == arg).ToLambda())
                    .SingleOrDefault();

                if (ambassador != null)
                    throw new ConsoleHelpAsException($"  *** The user principal name '{arg}' already exists ***");

                _credLogin = arg.ToLower();
            });

            HasOption("p|pass=", "Enter password to use", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No password given ***");

                _credPass = arg.ToLower();
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
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
                    new Ambassador_EF
                    {
                        UserPrincipalName = _credLogin,
                        EncryptedPass = cipherText,
                        IsEnabled = true,
                        IsDeletable = true,
                    });

                _uow.Commit();

                FormatOutput.Write(ambassador, true);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
