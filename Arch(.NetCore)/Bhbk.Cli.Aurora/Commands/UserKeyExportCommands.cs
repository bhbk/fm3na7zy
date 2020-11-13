using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
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
    public class UserKeyExportCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static tbl_User _user;

        public UserKeyExportCommands()
        {
            IsCommand("user-key-export", "Export private/public key for user");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var instance = new ContextService(InstanceContext.DeployedOrLocal);
                _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                    .Where(x => x.IdentityAlias == arg).ToLambda())
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var dir = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}.{_user.IdentityAlias}";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var keys = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKey>()
                    .Where(x => x.IdentityId == _user.IdentityId).ToLambda(),
                        new List<Expression<Func<tbl_PublicKey, object>>>()
                        {
                            x => x.PrivateKey,
                        });

                ConsoleHelper.StdOutKeyPairs(keys.OrderBy(x => x.CreatedUtc));

                Console.Out.Write("  *** Enter GUID of public key to export *** : ");
                var input = Guid.Parse(StandardInput.GetInput());

                var pubKey = keys.Where(x => x.Id == input).SingleOrDefault();

                if (pubKey != null)
                {
                    //public opensshbase64 key format in "authorized_keys"
                    var pubOpenSshBase64File = new FileInfo(dir + Path.DirectorySeparatorChar + "authorized_keys.txt");
                    var pubOpenSshBase64Str = KeyHelper.ExportPubKeyBase64(_user, new List<tbl_PublicKey>() { pubKey });
                    File.WriteAllText(pubOpenSshBase64File.FullName, pubOpenSshBase64Str.ToString());
                    Console.Out.WriteLine("Created " + pubOpenSshBase64File);

                    //public pkcs8 key format
                    var pubPkcs8File = new FileInfo(dir + Path.DirectorySeparatorChar + "pub." + SshPublicKeyFormat.Pkcs8.ToString().ToLower() + ".txt");
                    var pubPkcs8Bytes = KeyHelper.ExportPubKey(pubKey, SshPublicKeyFormat.Pkcs8);
                    File.WriteAllBytes(pubPkcs8File.FullName, pubPkcs8Bytes);
                    Console.Out.WriteLine("Created " + pubPkcs8File);

                    //public ssh2base64 key format
                    var pubSsh2Base64File = new FileInfo(dir + Path.DirectorySeparatorChar + "pub." + SshPublicKeyFormat.Ssh2Base64.ToString().ToLower() + ".txt");
                    var pubSsh2Base64Bytes = KeyHelper.ExportPubKey(pubKey, SshPublicKeyFormat.Ssh2Base64);
                    File.WriteAllBytes(pubSsh2Base64File.FullName, pubSsh2Base64Bytes);
                    Console.Out.WriteLine("Created " + pubSsh2Base64File);

                    //public ssh2raw key format
                    var pubSsh2RawFile = new FileInfo(dir + Path.DirectorySeparatorChar + "pub." + SshPublicKeyFormat.Ssh2Raw.ToString().ToLower());
                    var pubSsh2RawBytes = KeyHelper.ExportPubKey(pubKey, SshPublicKeyFormat.Ssh2Raw);
                    File.WriteAllBytes(pubSsh2RawFile.FullName, pubSsh2RawBytes);
                    Console.Out.WriteLine("Created " + pubSsh2Base64File);

                    if (pubKey.PrivateKey != null)
                    {
                        var privKey = pubKey.PrivateKey;

                        //private key password in cleartext
                        var privKeyPassFile = new FileInfo(dir + Path.DirectorySeparatorChar + "cleartext_passowrd.txt");
                        File.WriteAllText(privKeyPassFile.FullName, AES.DecryptString(privKey.KeyPass, _conf["Databases:AuroraSecret"]));
                        Console.Out.WriteLine("Created " + privKeyPassFile);

                        //private newopenssh key format
                        var privNewOpenSshFile = new FileInfo(dir + Path.DirectorySeparatorChar + "priv." + SshPrivateKeyFormat.NewOpenSsh.ToString().ToLower() + ".txt");
                        var privNewOpenSshBytes = KeyHelper.ExportPrivKey(_conf, privKey, SshPrivateKeyFormat.NewOpenSsh, privKey.KeyPass);
                        File.WriteAllBytes(privNewOpenSshFile.FullName, privNewOpenSshBytes);
                        Console.Out.WriteLine("Created " + privNewOpenSshFile);

                        //private openssh key format
                        var privOpenSshFile = new FileInfo(dir + Path.DirectorySeparatorChar + "priv." + SshPrivateKeyFormat.OpenSsh.ToString().ToLower() + ".txt");
                        var privOpenSshBytes = KeyHelper.ExportPrivKey(_conf, privKey, SshPrivateKeyFormat.OpenSsh, privKey.KeyPass);
                        File.WriteAllBytes(privOpenSshFile.FullName, privOpenSshBytes);
                        Console.Out.WriteLine("Created " + privOpenSshFile);

                        //private pkcs8 key format
                        var privPcks8File = new FileInfo(dir + Path.DirectorySeparatorChar + "priv." + SshPrivateKeyFormat.Pkcs8.ToString().ToLower() + ".txt");
                        var privPcks8Bytes = KeyHelper.ExportPrivKey(_conf, privKey, SshPrivateKeyFormat.Pkcs8, privKey.KeyPass);
                        File.WriteAllBytes(privPcks8File.FullName, privPcks8Bytes);
                        Console.Out.WriteLine("Created " + privPcks8File);

                        //private putty key format
                        var privPuttyFile = new FileInfo(dir + Path.DirectorySeparatorChar + "priv." + SshPrivateKeyFormat.Putty.ToString().ToLower() + ".txt");
                        var privPuttyBytes = KeyHelper.ExportPrivKey(_conf, privKey, SshPrivateKeyFormat.Putty, privKey.KeyPass);
                        File.WriteAllBytes(privPuttyFile.FullName, privPuttyBytes);
                        Console.Out.WriteLine("Created " + privPuttyFile);
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
