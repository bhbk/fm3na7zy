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
    public class SysCredCreateCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private string _credDomain, _credLogin, _credPass;

        public SysCredCreateCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-cred-create", "Create system credential");

            HasRequiredOption("d|domain=", "Enter credential domain to use", arg =>
            {
                _credDomain = arg;
            });

            HasRequiredOption("l|login=", "Enter credential login to use", arg =>
            {
                _credLogin = arg;
            });

            HasOption("p|pass=", "Enter credential password to use", arg =>
            {
                _credPass = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var exists = _uow.Credentials.Get(QueryExpressionFactory.GetQueryExpression<Credential>()
                    .Where(x => x.Domain == _credDomain && x.UserName == _credLogin).ToLambda())
                    .SingleOrDefault();

                if (exists != null)
                {
                    Console.Out.WriteLine("  *** The credential entered already exists ***");
                    OutputFactory.StdOutCredentials(new List<Credential> { exists });

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

                var credential = _uow.Credentials.Create(
                    new Credential
                    {
                        Domain = _credDomain,
                        UserName = _credLogin,
                        Password = cipherText,
                        IsEnabled = true,
                        IsDeletable = true,
                    });

                _uow.Commit();

                OutputFactory.StdOutCredentials(new List<Credential>() { credential });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
