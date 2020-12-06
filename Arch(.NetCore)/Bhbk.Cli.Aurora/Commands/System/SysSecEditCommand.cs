using Bhbk.Cli.Aurora.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Aurora.Primitives.Enums;
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
    public class SysSecEditCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;

        public SysSecEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-sec-edit", "Edit encrypt/decrypt secret in use by system");
        }

        public override int Run(string[] remainingArguments)
        {
            var keyType = ConfigType.RebexLicense.ToString();

            var license = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<Setting>()
                .Where(x => x.ConfigKey == keyType).ToLambda())
                .OrderBy(x => x.CreatedUtc)
                .Last();

            Rebex.Licensing.Key = license.ConfigValue;

            AsymmetricKeyAlgorithm.Register(Curve25519.Create);
            AsymmetricKeyAlgorithm.Register(Ed25519.Create);
            AsymmetricKeyAlgorithm.Register(EllipticCurveAlgorithm.Create);

            try
            {
                Console.Out.Write("  *** Enter current secret to decrypt the encrypted passwords *** : ");
                var secretCurrent = StandardInput.GetHiddenInput();
                Console.Out.WriteLine();


                var secretNew = AlphaNumeric.CreateString(32);
                Console.Out.WriteLine($"  *** The new secret to encrypt passwords is *** : {secretNew}");
                Console.Out.WriteLine();

                var creds = _uow.Credentials.Get();
                var keys = _uow.PrivateKeys.Get();

                Console.Out.WriteLine("  *** Current credential encrypted passwords *** ");
                OutputFactory.StdOutCredentialSecrets(creds);
                Console.Out.WriteLine();

                Console.Out.WriteLine("  *** Current private key encrypted passwords *** ");
                OutputFactory.StdOutKeyPairSecrets(keys);
                Console.Out.WriteLine();

                var updatedCreds = UserHelper.ChangeCredentialSecrets(creds.ToList(), secretCurrent, secretNew);
                var updatedKeys = KeyHelper.ChangePrivKeySecrets(keys.ToList(), secretCurrent, secretNew);

                Console.Out.Write("  *** Enter yes/no to proceed *** : ");
                var decision = StandardInput.GetInput();
                Console.Out.WriteLine();

                if (decision.ToLower() == "yes")
                {
                    Console.Out.WriteLine("  *** New credential encrypted passwords *** ");
                    OutputFactory.StdOutCredentialSecrets(updatedCreds);
                    Console.Out.WriteLine();

                    Console.Out.WriteLine("  *** New private key encrypted passwords *** ");
                    OutputFactory.StdOutKeyPairSecrets(updatedKeys);
                    Console.Out.WriteLine();

                    _uow.Credentials.Update(updatedCreds);
                    _uow.PrivateKeys.Update(updatedKeys);
                    _uow.Commit();
                }

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
