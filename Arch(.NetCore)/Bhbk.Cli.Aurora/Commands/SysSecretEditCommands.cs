using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using Rebex.Security.Cryptography;
using System;
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysSecretEditCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private string _secretCurrent, _secretNew;

        public SysSecretEditCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-secret-edit", "Edit secret used by system");

            HasOption("c|current=", "Enter current secret to encrypt passwords", arg =>
            {
                _secretCurrent = arg;
            });

            HasOption("n|new=", "Enter new secret to encrypt passwords", arg =>
            {
                _secretNew = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var license = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                    .Where(x => x.ConfigKey == "RebexLicense").ToLambda()).OrderBy(x => x.CreatedUtc)
                    .Last();

                Rebex.Licensing.Key = license.ConfigValue;

                AsymmetricKeyAlgorithm.Register(Curve25519.Create);
                AsymmetricKeyAlgorithm.Register(Ed25519.Create);
                AsymmetricKeyAlgorithm.Register(EllipticCurveAlgorithm.Create);

                if (string.IsNullOrEmpty(_secretCurrent))
                {
                    Console.Out.Write("  *** Enter current secret to encrypt passwords *** : ");
                    _secretCurrent = StandardInput.GetHiddenInput();
                }

                if (string.IsNullOrEmpty(_secretNew))
                {
                    Console.Out.Write("  *** Enter new secret to encrypt passwords *** : ");
                    _secretNew = StandardInput.GetHiddenInput();
                }
                else
                {
                    _secretNew = AlphaNumeric.CreateString(32);
                    Console.Out.WriteLine($"  *** The new secret to encrypt passwords is *** : {_secretNew}");
                }

                var keys = _uow.PrivateKeys.Get().ToList();
                var creds = _uow.Credentials.Get().ToList();

                Console.Out.WriteLine();
                Console.Out.WriteLine("  *** Current private key pass ciphertexts *** ");
                ConsoleHelper.StdOutKeyPairSecrets(keys);

                Console.Out.WriteLine();
                Console.Out.WriteLine("  *** Current credential password ciphertexts *** ");
                ConsoleHelper.StdOutCredentialSecrets(creds);

                keys = KeyHelper.EditPrivKeySecrets(_uow, keys, _secretCurrent, _secretNew).ToList();
                creds = UserHelper.EditCredentialSecrets(_uow, creds, _secretCurrent, _secretNew).ToList();

                Console.Out.WriteLine();
                Console.Out.WriteLine("  *** New private key pass ciphertexts *** ");
                ConsoleHelper.StdOutKeyPairSecrets(keys);

                Console.Out.WriteLine();
                Console.Out.WriteLine("  *** New credential password ciphertexts *** ");
                ConsoleHelper.StdOutCredentialSecrets(creds);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
