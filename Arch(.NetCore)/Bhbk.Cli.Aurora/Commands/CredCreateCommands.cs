using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
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
    public class CredCreateCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static string _credDomain;
        private static string _credUser;
        private static string _credPass;

        public CredCreateCommands()
        {
            IsCommand("create-credential", "Create system credential");

            HasRequiredOption("d|domain=", "Enter credential domain to use", arg =>
            {
                _credDomain = arg;
            });

            HasRequiredOption("l|login=", "Enter credential user to use", arg =>
            {
                _credUser = arg;
            });

            HasOption("p|pass=", "Enter credential password to use", arg =>
            {
                _credPass = arg;
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

                if (credentials.Where(x => x.Domain == _credDomain 
                    && x.UserName == _credUser).Any())
                {
                    Console.Out.WriteLine("  *** The credential entered already exists ***");
                    Console.Out.WriteLine();
                    ConsoleHelper.OutSysCredentials(credentials);

                    return StandardOutput.FondFarewell();
                }

                if (string.IsNullOrEmpty(_credPass))
                {
                    Console.Out.Write("  *** Enter credential password to use *** : ");
                    _credPass = StandardInput.GetHiddenInput();

                    Console.Out.WriteLine();
                }

                var secret = _conf["Databases:AuroraSecretKey"];
                var cipherText = AES.Encrypt(_credPass, secret);
                var plainText = AES.Decrypt(cipherText, secret);

                if (_credPass != plainText)
                    throw new ArithmeticException();

                var credential = _uow.SysCredentials.Create(
                    new tbl_SysCredentials
                    {
                        Id = Guid.NewGuid(),
                        Domain = _credDomain,
                        UserName = _credUser,
                        Password = cipherText,
                        Enabled = true,
                        Created = DateTime.Now,
                        Immutable = false,
                    });

                _uow.Commit();

                Console.Out.WriteLine();
                ConsoleHelper.OutSysCredentials(credentials);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
