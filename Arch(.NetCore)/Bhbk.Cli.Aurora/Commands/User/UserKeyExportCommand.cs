using Bhbk.Cli.Aurora.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Encryption;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using Rebex.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserKeyExportCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private UserLogin _user;

        public UserKeyExportCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("user-key-export", "Export public/private key for user");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.UserLogins.Get(QueryExpressionFactory.GetQueryExpression<UserLogin>()
                    .Where(x => x.IdentityAlias == arg).ToLambda(),
                        new List<Expression<Func<UserLogin, object>>>()
                        {
                            x => x.PrivateKeys,
                            x => x.PublicKeys,
                        })
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var pubKeys = _user.PublicKeys;
                var privKeys = _user.PrivateKeys;

                StandardOutputFactory.KeyPairs(pubKeys.OrderBy(x => x.CreatedUtc), privKeys);

                Console.Out.WriteLine();
                Console.Out.Write("  *** Enter GUID of public key to export *** : ");
                var input = Guid.Parse(StandardInput.GetInput());
                Console.Out.WriteLine();

                var pubKey = pubKeys.Where(x => x.Id == input)
                    .SingleOrDefault();

                if (pubKey != null)
                {
                    var dir = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}.{_user.IdentityAlias}" +
                        $"{Path.DirectorySeparatorChar}{pubKey.Id.ToString().ToLower()}{Path.DirectorySeparatorChar}";

                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    //public pkcs8 key format
                    var pubPkcs8File = new FileInfo(dir + "pub." + SshPublicKeyFormat.Pkcs8.ToString().ToLower() + ".txt");
                    var pubPkcs8Bytes = KeyHelper.ExportPubKey(pubKey, SshPublicKeyFormat.Pkcs8);
                    File.WriteAllBytes(pubPkcs8File.FullName, pubPkcs8Bytes);
                    Console.Out.WriteLine("Created " + pubPkcs8File);

                    //public ssh2base64 key format
                    var pubSsh2Base64File = new FileInfo(dir + "pub." + SshPublicKeyFormat.Ssh2Base64.ToString().ToLower() + ".txt");
                    var pubSsh2Base64Bytes = KeyHelper.ExportPubKey(pubKey, SshPublicKeyFormat.Ssh2Base64);
                    File.WriteAllBytes(pubSsh2Base64File.FullName, pubSsh2Base64Bytes);
                    Console.Out.WriteLine("Created " + pubSsh2Base64File);

                    //public ssh2raw key format
                    var pubSsh2RawFile = new FileInfo(dir + "pub." + SshPublicKeyFormat.Ssh2Raw.ToString().ToLower());
                    var pubSsh2RawBytes = KeyHelper.ExportPubKey(pubKey, SshPublicKeyFormat.Ssh2Raw);
                    File.WriteAllBytes(pubSsh2RawFile.FullName, pubSsh2RawBytes);
                    Console.Out.WriteLine("Created " + pubSsh2Base64File);

                    //public opensshbase64 key format in "authorized_keys"
                    var pubOpenSshBase64File = new FileInfo(dir + "authorized_keys.txt");
                    var pubOpenSshBase64Str = KeyHelper.ExportPubKeyBase64(_user, new List<PublicKey> { pubKey });
                    File.WriteAllText(pubOpenSshBase64File.FullName, pubOpenSshBase64Str.ToString());
                    Console.Out.WriteLine("Created " + pubOpenSshBase64File);

                    if (pubKey.PrivateKeyId != null)
                    {
                        var privKey = _user.PrivateKeys.Where(x => x.Id == pubKey.PrivateKeyId)
                            .Single();

                        var privKeyPass = AES.DecryptString(privKey.EncryptedPass, _conf["Databases:AuroraSecret"]);

                        //private newopenssh key format
                        var privNewOpenSshFile = new FileInfo(dir + "priv." + SshPrivateKeyFormat.NewOpenSsh.ToString().ToLower() + ".txt");
                        var privNewOpenSshBytes = KeyHelper.ExportPrivKey(privKey, SshPrivateKeyFormat.NewOpenSsh, privKeyPass);
                        File.WriteAllBytes(privNewOpenSshFile.FullName, privNewOpenSshBytes);
                        Console.Out.WriteLine("Created " + privNewOpenSshFile);

                        //private openssh key format
                        var privOpenSshFile = new FileInfo(dir + "priv." + SshPrivateKeyFormat.OpenSsh.ToString().ToLower() + ".txt");
                        var privOpenSshBytes = KeyHelper.ExportPrivKey(privKey, SshPrivateKeyFormat.OpenSsh, privKeyPass);
                        File.WriteAllBytes(privOpenSshFile.FullName, privOpenSshBytes);
                        Console.Out.WriteLine("Created " + privOpenSshFile);

                        //private pkcs8 key format
                        var privPcks8File = new FileInfo(dir + "priv." + SshPrivateKeyFormat.Pkcs8.ToString().ToLower() + ".txt");
                        var privPcks8Bytes = KeyHelper.ExportPrivKey(privKey, SshPrivateKeyFormat.Pkcs8, privKeyPass);
                        File.WriteAllBytes(privPcks8File.FullName, privPcks8Bytes);
                        Console.Out.WriteLine("Created " + privPcks8File);

                        //private putty key format
                        var privPuttyFile = new FileInfo(dir + "priv." + SshPrivateKeyFormat.Putty.ToString().ToLower() + ".txt");
                        var privPuttyBytes = KeyHelper.ExportPrivKey(privKey, SshPrivateKeyFormat.Putty, privKeyPass);
                        File.WriteAllBytes(privPuttyFile.FullName, privPuttyBytes);
                        Console.Out.WriteLine("Created " + privPuttyFile);

                        //private key password in cleartext
                        var privKeyPassFile = new FileInfo(dir + "priv.cleartext_passowrd.txt");
                        File.WriteAllText(privKeyPassFile.FullName, privKeyPass);
                        Console.Out.WriteLine("Created " + privKeyPassFile);
                    }
                }
                else
                    throw new ConsoleHelpAsException($"  *** Public key with GUID {input} not found ***");

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
