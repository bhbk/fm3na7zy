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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserKeysImportCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static FileInfo _path;
        private static tbl_Users _user;
        private static SshHostKeyAlgorithm _keyAlgo;
        private static SignatureHashAlgorithm _sigAlgo;
        private static string _privKeyPass;
        private static string _keyAlgoList = string.Join(", ", Enum.GetNames(typeof(SshHostKeyAlgorithm)));
        private static string _sigAlgoList = string.Join(", ", Enum.GetNames(typeof(SignatureHashAlgorithm)));

        public UserKeysImportCommands()
        {
            IsCommand("user-keys-import", "Import all public/private key pairs for user");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var instance = new ContextService(InstanceContext.DeployedOrLocal);
                _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                    .Where(x => x.IdentityAlias == arg).ToLambda(),
                        new List<Expression<Func<tbl_Users, object>>>()
                        {
                            x => x.tbl_PrivateKeys,
                            x => x.tbl_PublicKeys
                        }).SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasRequiredOption("a|alg=", "Enter key algorithm", arg =>
            {
                if (!Enum.TryParse(arg, out _keyAlgo))
                    throw new ConsoleHelpAsException($"*** Invalid key algorithm. Options are '{_keyAlgoList}' ***");
            });

            HasRequiredOption("s|sig=", "Enter signature algorithm", arg =>
            {
                if (!Enum.TryParse(arg, out _sigAlgo))
                    throw new ConsoleHelpAsException($"  *** Invalid signature algorithm. Options are '{_sigAlgoList}' ***");
            });

            HasRequiredOption("f|file=", "Enter file for import", arg =>
            {
                _path = new FileInfo(arg);
            });

            HasOption("p|pass=", "Enter private key password", arg =>
            {
                _privKeyPass = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (!Directory.Exists(_path.FullName))
                    Directory.CreateDirectory(_path.FullName);

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

                var privKey = _user.tbl_PrivateKeys.OrderBy(x => x.Created).FirstOrDefault();
                var pubKey = _user.tbl_PublicKeys.OrderBy(x => x.Created).FirstOrDefault();

                //private newopenssh key format
                var privNewOpenSsh = SshPrivateKeyFormat.NewOpenSsh;
                var privNewOpenSshFile = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "key." + privNewOpenSsh.ToString().ToLower() + ".txt");
                KeyHelper.ImportSshPrivKey(_uow, _user, 
                    SshPrivateKeyFormat.Pkcs8, SshHostKeyAlgorithm.RSA, privKey.KeyPass,
                    SshPublicKeyFormat.Pkcs8, SignatureHashAlgorithm.SHA256, Dns.GetHostName(), privNewOpenSshFile);
                Console.Out.WriteLine("Opened " + privNewOpenSshFile);

                //private openssh key format
                var privOpenSsh = SshPrivateKeyFormat.OpenSsh;
                var privOpenSshFile = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "key." + privOpenSsh.ToString().ToLower() + ".txt");
                KeyHelper.ImportSshPrivKey(_uow, _user,
                    SshPrivateKeyFormat.Pkcs8, SshHostKeyAlgorithm.RSA, privKey.KeyPass,
                    SshPublicKeyFormat.Pkcs8, SignatureHashAlgorithm.SHA256, Dns.GetHostName(), privOpenSshFile);
                Console.Out.WriteLine("Opened " + privOpenSshFile);

                //private pkcs8 key format
                var privPcks8 = SshPrivateKeyFormat.Pkcs8;
                var privPcks8File = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "key." + privPcks8.ToString().ToLower() + ".txt");
                KeyHelper.ImportSshPrivKey(_uow, _user,
                    SshPrivateKeyFormat.Pkcs8, SshHostKeyAlgorithm.RSA, privKey.KeyPass,
                    SshPublicKeyFormat.Pkcs8, SignatureHashAlgorithm.SHA256, Dns.GetHostName(), privPcks8File);
                Console.Out.WriteLine("Opened " + privPcks8File);

                //private putty key format
                var privPutty = SshPrivateKeyFormat.Putty;
                var privPuttyFile = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "key." + privPutty.ToString().ToLower() + ".txt");
                KeyHelper.ImportSshPrivKey(_uow, _user,
                    SshPrivateKeyFormat.Pkcs8, SshHostKeyAlgorithm.RSA, privKey.KeyPass,
                    SshPublicKeyFormat.Pkcs8, SignatureHashAlgorithm.SHA256, Dns.GetHostName(), privPuttyFile);
                Console.Out.WriteLine("Opened " + privPuttyFile);

                //public opensshbase64 key format
                var pubOpenSshBase64File = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "pub.opensshbase64.txt");
                KeyHelper.ImportSshPubKeyBase64(_uow, _user, 
                    SignatureHashAlgorithm.SHA256, pubOpenSshBase64File);
                Console.Out.WriteLine("Opened " + pubOpenSshBase64File);

                //public opensshbase64 key format in "authorized_keys"
                var pubOpenSshBase64sFile = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "authorized_keys.txt");
                KeyHelper.ImportSshPubKeysBase64(_uow, _user, 
                    SignatureHashAlgorithm.SHA256, pubOpenSshBase64sFile);
                Console.Out.WriteLine("Opened " + pubOpenSshBase64sFile);

                //public pkcs8 key format
                var pubPkcs8 = SshPublicKeyFormat.Pkcs8;
                var pubPkcs8File = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "pub." + pubPkcs8.ToString().ToLower() + ".txt");
                KeyHelper.ImportSshPubKey(_uow, _user, 
                    SshHostKeyAlgorithm.RSA, SshPublicKeyFormat.Pkcs8,
                    SignatureHashAlgorithm.SHA256, Dns.GetHostName(), pubPkcs8File);
                Console.Out.WriteLine("Opened " + pubPkcs8File);

                //public ssh2base64 key format
                var pubSsh2Base64 = SshPublicKeyFormat.Ssh2Base64;
                var pubSsh2Base64File = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "pub." + pubSsh2Base64.ToString().ToLower() + ".txt");
                KeyHelper.ImportSshPubKey(_uow, _user,
                    SshHostKeyAlgorithm.RSA, SshPublicKeyFormat.Pkcs8,
                    SignatureHashAlgorithm.SHA256, Dns.GetHostName(), pubSsh2Base64File);
                Console.Out.WriteLine("Opened " + pubSsh2Base64File);

                //public ssh2raw key format
                var pubSsh2Raw = SshPublicKeyFormat.Ssh2Raw;
                var pubSsh2RawFile = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "pub." + pubSsh2Raw.ToString().ToLower());
                KeyHelper.ImportSshPubKey(_uow, _user,
                    SshHostKeyAlgorithm.RSA, SshPublicKeyFormat.Pkcs8,
                    SignatureHashAlgorithm.SHA256, Dns.GetHostName(), pubSsh2RawFile);
                Console.Out.WriteLine("Opened " + pubSsh2Base64File);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
