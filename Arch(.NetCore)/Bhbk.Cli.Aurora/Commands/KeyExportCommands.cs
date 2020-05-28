using Bhbk.Lib.Aurora.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
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
    public class KeyExportCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static FileInfo _path;
        private static tbl_Users _user;

        public KeyExportCommands()
        {
            IsCommand("export-keys", "Export all public/private key pairs");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
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

            HasRequiredOption("d|directory=", "Enter directory path for exports", arg =>
            {
                _path = new FileInfo(arg);
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (!Directory.Exists(_path.FullName))
                    Directory.CreateDirectory(_path.FullName);

                var priv = _user.tbl_UserPrivateKeys.OrderBy(x => x.Created).FirstOrDefault();
                var pub = _user.tbl_UserPublicKeys.OrderBy(x => x.Created).FirstOrDefault();

                //private newopenssh key format
                var privNewOpenSsh = SshPrivateKeyFormat.NewOpenSsh;
                var privNewOpenSshFile = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "key." + privNewOpenSsh.ToString().ToLower() + ".txt");
                KeyHelper.ExportSshPrivateKey(_user, priv, privNewOpenSsh, priv.KeyValuePass, privNewOpenSshFile);
                Console.Out.WriteLine("Created " + privNewOpenSshFile);

                //private openssh key format
                var privOpenSsh = SshPrivateKeyFormat.OpenSsh;
                var privOpenSshFile = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "key." + privOpenSsh.ToString().ToLower() + ".txt");
                KeyHelper.ExportSshPrivateKey(_user, priv, privOpenSsh, priv.KeyValuePass, privOpenSshFile);
                Console.Out.WriteLine("Created " + privOpenSshFile);

                //private pkcs8 key format
                var privPcks8 = SshPrivateKeyFormat.Pkcs8;
                var privPcks8File = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "key." + privPcks8.ToString().ToLower() + ".txt");
                KeyHelper.ExportSshPrivateKey(_user, priv, privPcks8, priv.KeyValuePass, privPcks8File);
                Console.Out.WriteLine("Created " + privPcks8File);

                //private putty key format
                var privPutty = SshPrivateKeyFormat.Putty;
                var privPuttyFile = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "key." + privPutty.ToString().ToLower() + ".txt");
                KeyHelper.ExportSshPrivateKey(_user, priv, privPutty, priv.KeyValuePass, privPuttyFile);
                Console.Out.WriteLine("Created " + privPuttyFile);

                //public opensshbase64 key format
                var pubOpenSshBase64File = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "pub.opensshbase64.txt");
                KeyHelper.ExportSshPublicKeyBase64(_user, pub, pubOpenSshBase64File);
                Console.Out.WriteLine("Created " + pubOpenSshBase64File);

                //public opensshbase64 key format in "authorized_keys"
                var pubOpenSshBase64sFile = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "authorized_keys.txt");
                KeyHelper.ExportSshPublicKeysBase64(_user, _user.tbl_UserPublicKeys, pubOpenSshBase64sFile);
                Console.Out.WriteLine("Created " + pubOpenSshBase64sFile);

                //public pkcs8 key format
                var pubPkcs8 = SshPublicKeyFormat.Pkcs8;
                var pubPkcs8File = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "pub." + pubPkcs8.ToString().ToLower() + ".txt");
                KeyHelper.ExportSshPublicKey(_user, pub, pubPkcs8, pubPkcs8File);
                Console.Out.WriteLine("Created " + pubPkcs8File);

                //public ssh2base64 key format
                var pubSsh2Base64 = SshPublicKeyFormat.Ssh2Base64;
                var pubSsh2Base64File = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "pub." + pubSsh2Base64.ToString().ToLower() + ".txt");
                KeyHelper.ExportSshPublicKey(_user, pub, pubSsh2Base64, pubSsh2Base64File);
                Console.Out.WriteLine("Created " + pubSsh2Base64File);

                //public ssh2raw key format
                var pubSsh2Raw = SshPublicKeyFormat.Ssh2Raw;
                var pubSsh2RawFile = new FileInfo(_path.FullName + Path.DirectorySeparatorChar + "pub." + pubSsh2Raw.ToString().ToLower());
                KeyHelper.ExportSshPublicKey(_user, pub, pubSsh2Raw, pubSsh2RawFile);
                Console.Out.WriteLine("Created " + pubSsh2Base64File);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
