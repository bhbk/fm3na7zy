using Bhbk.Cli.Aurora.IO;
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
using Rebex;
using Rebex.Security.Cryptography;
using System;
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands.System
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

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-sec-edit", "Edit encrypt/decrypt secret in use by system");
        }

        public override int Run(string[] remainingArguments)
        {
            var keyType = ConfigType.RebexLicense.ToString();

            var license = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<E_Setting>()
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

                var loginType = UserAuthType.Local.ToString().ToLower();

                var ambassadors = _uow.Ambassadors.Get();
                var privateKeys = _uow.PrivateKeys.Get();
                var logins = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                    .Where(x => x.UserAuthType.ToLower() == loginType).ToLambda());

                Console.Out.WriteLine("  *** Current ambassador encrypted passwords *** ");
                FormatOutput.AmbassadorSecrets(ambassadors);
                Console.Out.WriteLine();

                Console.Out.WriteLine("  *** Current private key encrypted passwords *** ");
                FormatOutput.KeyPairSecrets(privateKeys);
                Console.Out.WriteLine();

                Console.Out.WriteLine("  *** Current login encrypted passwords *** ");
                FormatOutput.LoginSecrets(logins);
                Console.Out.WriteLine();

                var updatedAmbassadors = UserHelper.ChangeAmbassadorSecrets(ambassadors.ToList(), secretCurrent, secretNew);
                var updatedKeys = KeyHelper.ChangePrivKeySecrets(privateKeys.ToList(), secretCurrent, secretNew);
                var updatedLogins = UserHelper.ChangeLoginSecrets(logins.ToList(), secretCurrent, secretNew);

                Console.Out.Write("  *** Enter yes/no to proceed *** : ");
                var decision = StandardInput.GetInput();
                Console.Out.WriteLine();

                if (decision.ToLower() == "yes")
                {
                    Console.Out.WriteLine("  *** New ambassador encrypted passwords *** ");
                    FormatOutput.AmbassadorSecrets(updatedAmbassadors);
                    Console.Out.WriteLine();

                    Console.Out.WriteLine("  *** New private key encrypted passwords *** ");
                    FormatOutput.KeyPairSecrets(updatedKeys);
                    Console.Out.WriteLine();

                    Console.Out.WriteLine("  *** New login encrypted passwords *** ");
                    FormatOutput.LoginSecrets(updatedLogins);
                    Console.Out.WriteLine();

                    _uow.Ambassadors.Update(updatedAmbassadors);
                    _uow.PrivateKeys.Update(updatedKeys);
                    _uow.Logins.Update(updatedLogins);
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
