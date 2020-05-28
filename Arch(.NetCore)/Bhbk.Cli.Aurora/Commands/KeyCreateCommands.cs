using Bhbk.Lib.Aurora.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
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
    public class KeyCreateCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static tbl_Users _user;
        private static int _keySize;
        private static string _keyPass;
        private static string _keyDns;
        private static SshHostKeyAlgorithm _keyType;
        private static SignatureHashAlgorithm _sigType;
        private static string _keyTypeList = string.Join(", ", Enum.GetNames(typeof(SshHostKeyAlgorithm)));
        private static string _sigTypeList = string.Join(", ", Enum.GetNames(typeof(SignatureHashAlgorithm)));

        public KeyCreateCommands()
        {
            IsCommand("create-key", "Create user public/private key pairs");

            HasRequiredOption("u|user=", "Enter user that already exists", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                var file = SearchRoots.ByAssemblyContext("clisettings.json");

                _conf = (IConfiguration)new ConfigurationBuilder()
                    .SetBasePath(file.DirectoryName)
                    .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                    .Build();

                var instance = new ContextService(InstanceContext.DeployedOrLocal);
                _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                    .Where(x => x.UserName == arg).ToLambda(),
                        new List<Expression<Func<tbl_Users, object>>>()
                        {
                            x => x.tbl_UserPasswords,
                            x => x.tbl_UserPrivateKeys,
                            x => x.tbl_UserPublicKeys
                        }).SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasRequiredOption("t|type=", "Enter type of key algorithm", arg =>
            {
                if (!Enum.TryParse(arg, out _keyType))
                    throw new ConsoleHelpAsException($"*** Invalid key algorithm. Options are '{_keyTypeList}' ***");
            });

            HasRequiredOption("s|size=", "Enter key size", arg =>
            {
                if (!int.TryParse(arg, out _keySize))
                    throw new ConsoleHelpAsException($"  *** Invalid key size '{_keySize}' ***");
            });

            HasRequiredOption("h|hash=", "Enter type of hash algorithm", arg =>
            {
                if (!Enum.TryParse(arg, out _sigType))
                    throw new ConsoleHelpAsException($"  *** Invalid hash algorithm. Options are '{_sigTypeList}' ***");
            });

            HasOption("p|pass=", "Enter password for public key", arg =>
            {
                _keyPass = arg;
            });

            HasOption("d|dns=", "Enter hostname for public key", arg =>
            {
                _keyDns = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (!string.IsNullOrEmpty(_keyPass))
                {
                    Console.Out.Write("  *** Enter password for the new private key *** : ");
                    _keyPass = StandardInput.GetHiddenInput();
                }
                else
                {
                    Console.Out.WriteLine("  *** The password is being generated... ***");
                    _keyPass = AlphaNumeric.CreateString(32);
                }

                if (string.IsNullOrEmpty(_keyDns))
                    _keyDns = Dns.GetHostName();

                var key = KeyHelper.GenerateSshPrivateKey(_uow, _user, _keyType, _keySize, _keyPass, _sigType, _keyDns);

                Console.Out.WriteLine();
                Console.Out.WriteLine("  *** Private key (base64 format) ***"
                    + Environment.NewLine + $"{Convert.ToBase64String(key.GetPrivateKey())}");

                Console.Out.WriteLine();
                Console.Out.WriteLine("  *** Public key (base64 format) ***"
                    + Environment.NewLine + $"{Convert.ToBase64String(key.GetPublicKey())}");

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
