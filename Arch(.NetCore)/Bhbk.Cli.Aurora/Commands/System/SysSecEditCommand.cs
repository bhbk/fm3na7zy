using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
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

            IsCommand("sys-sec-edit", "Edit encrypt/decrypt secret used on system");
        }

        public override int Run(string[] remainingArguments)
        {
            var keyType = ConfigType_E.RebexLicense.ToString();

            var license = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<Setting_EF>()
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

                var loginType = (int)AuthType_E.Local;

                var ambassadors = _uow.Ambassadors.Get();
                var privKeys = _uow.PrivateKeys.Get();
                var logins = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                    .Where(x => x.AuthTypeId == (int)loginType).ToLambda());

                Console.Out.WriteLine("  *** Current credential encrypted passwords *** ");
                foreach (var ambassador in ambassadors)
                    FormatOutput.Write(ambassador, true);
                Console.Out.WriteLine();

                Console.Out.WriteLine("  *** Current private key encrypted passwords *** ");
                foreach (var privKey in privKeys)
                    FormatOutput.Write(privKey, true);
                Console.Out.WriteLine();

                Console.Out.WriteLine("  *** Current login encrypted passwords *** ");
                foreach (var login in logins)
                    FormatOutput.Write(login, true);
                Console.Out.WriteLine();

                var updatedAmbassadors = UserHelper.ChangeAmbassadorSecrets(ambassadors.ToList(), secretCurrent, secretNew);
                var updatedPrivKeys = KeyHelper.ChangePrivKeySecrets(privKeys.ToList(), secretCurrent, secretNew);
                var updatedLogins = UserHelper.ChangeLoginSecrets(logins.ToList(), secretCurrent, secretNew);

                Console.Out.Write("  *** Enter yes/no to proceed *** : ");
                var decision = StandardInput.GetInput();
                Console.Out.WriteLine();

                if (decision.ToLower() == "yes")
                {
                    Console.Out.WriteLine("  *** New credential encrypted passwords *** ");
                    foreach (var updatedAmbassador in updatedAmbassadors)
                        FormatOutput.Write(updatedAmbassador, true);
                    Console.Out.WriteLine();

                    Console.Out.WriteLine("  *** New private key encrypted passwords *** ");
                    foreach (var updatedPrivKey in updatedPrivKeys)
                        FormatOutput.Write(updatedPrivKey, true);
                    Console.Out.WriteLine();

                    Console.Out.WriteLine("  *** New login encrypted passwords *** ");
                    foreach (var updatedLogin in updatedLogins)
                        FormatOutput.Write(updatedLogin, true);
                    Console.Out.WriteLine();

                    _uow.Ambassadors.Update(updatedAmbassadors);
                    _uow.PrivateKeys.Update(updatedPrivKeys);
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
