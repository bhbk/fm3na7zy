using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Encryption;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysCredEditCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static string _credDomain;
        private static string _credLogin;
        private static string _credPass;
        
        public SysCredEditCommands()
        {
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

            var file = Search.ByAssemblyInvocation("clisettings.json");

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
                var credential = _uow.Credentials.Get(QueryExpressionFactory.GetQueryExpression<tbl_Credentials>()
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

                var secret = _conf["Databases:AuroraSecretKey"];
                var cipherText = AES.EncryptString(_credPass, secret);
                var plainText = AES.DecryptString(cipherText, secret);

                if (_credPass != plainText)
                    throw new ArithmeticException();

                _uow.Credentials.Update(credential);
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
