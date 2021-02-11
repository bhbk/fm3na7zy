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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands.User
{
    public class UserKeyImportCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private FileInfo _path;
        private Login_EF _user;
        private string _privKeyPass, _pubKeyComment;

        public UserKeyImportCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("user-key-import", "Import public/private key for user");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                    .Where(x => x.UserName == arg).ToLambda(),
                        new List<Expression<Func<Login_EF, object>>>()
                        {
                            x => x.PrivateKeys,
                            x => x.PublicKeys
                        })
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasRequiredOption("f|file=", "Enter file for import", arg =>
            {
                _path = new FileInfo(arg);
            });

            HasOption("p|passphrase=", "Enter private key password", arg =>
            {
                _privKeyPass = arg;
            });

            HasOption("c|comment=", "Enter public key comment", arg =>
            {
                _pubKeyComment = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
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

                if (string.IsNullOrEmpty(_privKeyPass))
                {
                    Console.Out.Write("  *** Enter private key password *** : ");
                    _privKeyPass = StandardInput.GetHiddenInput();
                    Console.Out.WriteLine();
                }

                if (string.IsNullOrEmpty(_pubKeyComment))
                {
                    Console.Out.Write("  *** Enter public key comment *** : ");
                    _pubKeyComment = StandardInput.GetInput();
                    Console.Out.WriteLine();
                }

                Console.Out.WriteLine("Opened " + _path.FullName);
                Console.Out.WriteLine();

                var stream = new MemoryStream();

                using (FileStream fileStream = new FileStream(_path.FullName, FileMode.Open, FileAccess.Read))
                    fileStream.CopyTo(stream);

                var keyPair = KeyHelper.ImportKeyPair(_conf, _uow, _user, SignatureHashAlgorithm.SHA256, stream, _privKeyPass, _pubKeyComment);

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
