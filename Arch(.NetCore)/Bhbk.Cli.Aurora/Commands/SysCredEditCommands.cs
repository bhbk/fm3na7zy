using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data_EF6.Infrastructure;
using Bhbk.Lib.Aurora.Data_EF6.Models;
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
    public class SysCredEditCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private string _credDomain, _credLogin, _credPass;
        
        public SysCredEditCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-cred-edit", "Edit system credential");

            HasRequiredOption("d|domain=", "Enter credential domain", arg =>
            {
                _credDomain = arg;
            });

            HasRequiredOption("l|login=", "Enter credential login", arg =>
            {
                _credLogin = arg;
            });

            HasOption("p|pass=", "Enter credential password", arg =>
            {
                _credPass = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var credential = _uow.Credentials.Get(QueryExpressionFactory.GetQueryExpression<Credential>()
                    .Where(x => x.Domain == _credDomain && x.UserName == _credLogin).ToLambda())
                    .SingleOrDefault();

                if (credential == null)
                    throw new ConsoleHelpAsException($"  *** Invalid credential '{_credDomain}\\{_credLogin}' ***");

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

                credential.Password = cipherText;
                credential.LastUpdatedUtc = DateTime.UtcNow;

                credential = _uow.Credentials.Update(credential);
                _uow.Commit();

                ConsoleHelper.StdOutCredentials(new List<Credential>() { credential });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
