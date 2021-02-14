using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using Rebex.Security.Certificates;
using Rebex.Security.Cryptography;
using System;
using System.IO;
using System.Linq;

namespace Bhbk.Cli.Aurora.Commands.System
{
    public class SysKeyImportCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private FileInfo _path;
        private string _privKeyPass;

        public SysKeyImportCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-key-import", "Import public/private key for system");

            HasRequiredOption("f|file=", "Enter file for import", arg =>
            {
                CheckRequiredArguments();

                _path = new FileInfo(arg);
            });

            HasOption("p|pass=", "Enter private key password", arg =>
            {
                CheckRequiredArguments();

                _privKeyPass = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            var key = ConfigType_E.RebexLicense.ToString();

            var license = _uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<Setting_EF>()
                .Where(x => x.ConfigKey == key).ToLambda())
                .OrderBy(x => x.CreatedUtc)
                .Last();

            Rebex.Licensing.Key = license.ConfigValue;

            AsymmetricKeyAlgorithm.Register(Curve25519.Create);
            AsymmetricKeyAlgorithm.Register(Ed25519.Create);
            AsymmetricKeyAlgorithm.Register(EllipticCurveAlgorithm.Create);

            try
            {
                if (string.IsNullOrEmpty(_privKeyPass))
                {
                    Console.Out.Write("  *** Enter private key passphrase (only needed if currently encrypted) *** : ");
                    _privKeyPass = StandardInput.GetHiddenInput();
                    Console.Out.WriteLine();
                }

                Console.Out.WriteLine("Opened " + _path.FullName);
                Console.Out.WriteLine();

                var stream = new MemoryStream();

                using (FileStream fileStream = new FileStream(_path.FullName, FileMode.Open, FileAccess.Read))
                    fileStream.CopyTo(stream);

                var keyPair = KeyHelper.ImportKeyPair(_conf, _uow, SignatureHashAlgorithm.SHA256, stream, _privKeyPass);

                var pubKey = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<PublicKey_EF>()
                    .Where(x => x.Id == keyPair.Item1.Id).ToLambda())
                    .SingleOrDefault();

                var privKey = _uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey_EF>()
                    .Where(x => x.PublicKeyId == keyPair.Item1.Id).ToLambda())
                    .SingleOrDefault();

                if (pubKey == null)
                {
                    _uow.PublicKeys.Create(keyPair.Item1);
                    _uow.Commit();
                }

                if (privKey == null)
                {
                    _uow.PrivateKeys.Create(keyPair.Item2);
                    _uow.Commit();
                }

                pubKey = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<PublicKey_EF>()
                    .Where(x => x.Id == keyPair.Item1.Id).ToLambda())
                    .Single();

                privKey = _uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey_EF>()
                    .Where(x => x.PublicKeyId == keyPair.Item1.Id).ToLambda())
                    .Single();

                FormatOutput.Write(pubKey, privKey, true);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
