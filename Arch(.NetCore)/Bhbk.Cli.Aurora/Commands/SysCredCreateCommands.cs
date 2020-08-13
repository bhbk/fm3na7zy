using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Encryption;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysCredCreateCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static string _credDomain;
        private static string _credLogin;
        private static string _credPass;

        public SysCredCreateCommands()
        {
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
                var credentials = _uow.Ambassadors.Get();

                if (credentials.Where(x => x.Domain == _credDomain 
                    && x.UserName == _credLogin).Any())
                {
                    Console.Out.WriteLine("  *** The credential entered already exists ***");
                    Console.Out.WriteLine();
                    ConsoleHelper.StdOutAmbassadors(credentials);

                    return StandardOutput.FondFarewell();
                }

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

                var credential = _uow.Ambassadors.Create(
                    new tbl_Ambassadors
                    {
                        Id = Guid.NewGuid(),
                        Domain = _credDomain,
                        UserName = _credLogin,
                        Password = cipherText,
                        Enabled = true,
                        Created = DateTime.Now,
                        Immutable = false,
                    });

                _uow.Commit();

                Console.Out.WriteLine();
                ConsoleHelper.StdOutAmbassadors(credentials);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
