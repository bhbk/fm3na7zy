using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
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
    public class UserKeysExportCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static tbl_Users _user;

        public UserKeysExportCommands()
        {
            IsCommand("user-keys-export", "Export all public/private key pairs for user");

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
                    .Where(x => x.UserName == arg).ToLambda())
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var dir = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}.{_user.UserName}";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var pubKeys = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
                    .Where(x => x.UserId == _user.Id).ToLambda(),
                        new List<Expression<Func<tbl_PublicKeys, object>>>()
                        {
                            x => x.PrivateKey,
                        });

                ConsoleHelper.StdOutKeyPairs(pubKeys.Where(x => x.Immutable == false));

                Console.Out.Write("  *** Enter GUID of public key to export *** : ");
                var input = Guid.Parse(StandardInput.GetInput());

                var pubKey = pubKeys.Where(x => x.Id == input).SingleOrDefault();

                if (pubKey != null)
                {
                    //public opensshbase64 key format
                    var pubOpenSshBase64File = new FileInfo(dir + Path.DirectorySeparatorChar + "pub.opensshbase64.txt");
                    KeyHelper.ExportSshPubKeyBase64(_user, pubKey, pubOpenSshBase64File);
                    Console.Out.WriteLine("Created " + pubOpenSshBase64File);

                    //public opensshbase64 key format in "authorized_keys"
                    var pubOpenSshBase64sFile = new FileInfo(dir + Path.DirectorySeparatorChar + "authorized_keys.txt");
                    KeyHelper.ExportSshPubKeysBase64(_user, new List<tbl_PublicKeys>() { pubKey }, pubOpenSshBase64sFile);
                    Console.Out.WriteLine("Created " + pubOpenSshBase64sFile);

                    //public pkcs8 key format
                    var pubPkcs8 = SshPublicKeyFormat.Pkcs8;
                    var pubPkcs8File = new FileInfo(dir + Path.DirectorySeparatorChar + "pub." + pubPkcs8.ToString().ToLower() + ".txt");
                    KeyHelper.ExportSshPubKey(pubKey, pubPkcs8, pubPkcs8File);
                    Console.Out.WriteLine("Created " + pubPkcs8File);

                    //public ssh2base64 key format
                    var pubSsh2Base64 = SshPublicKeyFormat.Ssh2Base64;
                    var pubSsh2Base64File = new FileInfo(dir + Path.DirectorySeparatorChar + "pub." + pubSsh2Base64.ToString().ToLower() + ".txt");
                    KeyHelper.ExportSshPubKey(pubKey, pubSsh2Base64, pubSsh2Base64File);
                    Console.Out.WriteLine("Created " + pubSsh2Base64File);

                    //public ssh2raw key format
                    var pubSsh2Raw = SshPublicKeyFormat.Ssh2Raw;
                    var pubSsh2RawFile = new FileInfo(dir + Path.DirectorySeparatorChar + "pub." + pubSsh2Raw.ToString().ToLower());
                    KeyHelper.ExportSshPubKey(pubKey, pubSsh2Raw, pubSsh2RawFile);
                    Console.Out.WriteLine("Created " + pubSsh2Base64File);

                    if (pubKey.PrivateKey != null)
                    {
                        var privKey = pubKey.PrivateKey;

                        //private newopenssh key format
                        var privNewOpenSsh = SshPrivateKeyFormat.NewOpenSsh;
                        var privNewOpenSshFile = new FileInfo(dir + Path.DirectorySeparatorChar + "key." + privNewOpenSsh.ToString().ToLower() + ".txt");
                        KeyHelper.ExportSshPrivKey(privKey, privNewOpenSsh, privKey.KeyPass, privNewOpenSshFile);
                        Console.Out.WriteLine("Created " + privNewOpenSshFile);

                        //private openssh key format
                        var privOpenSsh = SshPrivateKeyFormat.OpenSsh;
                        var privOpenSshFile = new FileInfo(dir + Path.DirectorySeparatorChar + "key." + privOpenSsh.ToString().ToLower() + ".txt");
                        KeyHelper.ExportSshPrivKey(privKey, privOpenSsh, privKey.KeyPass, privOpenSshFile);
                        Console.Out.WriteLine("Created " + privOpenSshFile);

                        //private pkcs8 key format
                        var privPcks8 = SshPrivateKeyFormat.Pkcs8;
                        var privPcks8File = new FileInfo(dir + Path.DirectorySeparatorChar + "key." + privPcks8.ToString().ToLower() + ".txt");
                        KeyHelper.ExportSshPrivKey(privKey, privPcks8, privKey.KeyPass, privPcks8File);
                        Console.Out.WriteLine("Created " + privPcks8File);

                        //private putty key format
                        var privPutty = SshPrivateKeyFormat.Putty;
                        var privPuttyFile = new FileInfo(dir + Path.DirectorySeparatorChar + "key." + privPutty.ToString().ToLower() + ".txt");
                        KeyHelper.ExportSshPrivKey(privKey, privPutty, privKey.KeyPass, privPuttyFile);
                        Console.Out.WriteLine("Created " + privPuttyFile);
                    }
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
