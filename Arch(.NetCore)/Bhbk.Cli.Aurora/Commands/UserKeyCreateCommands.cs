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
using Rebex.Net;
using Rebex.Security.Certificates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserKeyCreateCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static tbl_Users _user;
        private static int _privKeySize;
        private static string _privKeyPass;
        private static string _pubKeyDns;
        private static SshHostKeyAlgorithm _keyAlgo;
        private static SignatureHashAlgorithm _sigAlgo;
        private static string _keyAlgoList = string.Join(", ", Enum.GetNames(typeof(SshHostKeyAlgorithm)));
        private static string _sigAlgoList = string.Join(", ", Enum.GetNames(typeof(SignatureHashAlgorithm)));

        public UserKeyCreateCommands()
        {
            IsCommand("user-key-create", "Create user public/private key pairs");

            HasRequiredOption("u|user=", "Enter user that already exists", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var instance = new ContextService(InstanceContext.DeployedOrLocal);
                _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                    .Where(x => x.UserName == arg).ToLambda(),
                        new List<Expression<Func<tbl_Users, object>>>()
                        {
                            x => x.tbl_PrivateKeys,
                            x => x.tbl_PublicKeys,
                        }).SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasRequiredOption("a|alg=", "Enter key algorithm", arg =>
            {
                if (!Enum.TryParse(arg, out _keyAlgo))
                    throw new ConsoleHelpAsException($"*** Invalid key algorithm. Options are '{_keyAlgoList}' ***");
            });

            HasRequiredOption("s|size=", "Enter key size", arg =>
            {
                if (!int.TryParse(arg, out _privKeySize))
                    throw new ConsoleHelpAsException($"  *** Invalid key size '{_privKeySize}' ***");
            });

            HasRequiredOption("h|hash=", "Enter signature algorithm", arg =>
            {
                if (!Enum.TryParse(arg, out _sigAlgo))
                    throw new ConsoleHelpAsException($"  *** Invalid signature algorithm. Options are '{_sigAlgoList}' ***");
            });

            HasOption("p|pass=", "Enter private key password", arg =>
            {
                _privKeyPass = arg;
            });

            HasOption("d|dns=", "Enter public key hostname", arg =>
            {
                _pubKeyDns = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                Console.Out.WriteLine();

                if (!string.IsNullOrEmpty(_privKeyPass))
                {
                    Console.Out.Write("  *** Enter password for the private key *** : ");
                    _privKeyPass = StandardInput.GetHiddenInput();
                }
                else
                {
                    _privKeyPass = AlphaNumeric.CreateString(32);
                    Console.Out.WriteLine($"  *** The password for the private key *** : {_privKeyPass}");
                }

                if (string.IsNullOrEmpty(_pubKeyDns))
                    _pubKeyDns = Dns.GetHostName();

                var privKey = KeyHelper.CreateUserSshPrivKey(_uow, _user,
                    SshPrivateKeyFormat.Pkcs8, _keyAlgo, _privKeySize, _privKeyPass,
                    SshPublicKeyFormat.Pkcs8, _sigAlgo, _pubKeyDns);

                var pubKey = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
                    .Where(x => x.PrivateKeyId == privKey.Id).ToLambda())
                    .Single();

                Console.Out.WriteLine($"{privKey.KeyValue}");
                Console.Out.WriteLine($"{pubKey.KeyValue}");

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
