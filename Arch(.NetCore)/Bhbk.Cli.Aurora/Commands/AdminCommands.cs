using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
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
    public class AdminCommands : ConsoleCommand
    {
        private static CommandTypes _cmdType;
        private static string _cmdTypeList = string.Join(", ", Enum.GetNames(typeof(CommandTypes)));

        public AdminCommands()
        {
            IsCommand("admin", "Do things with aurora entities...");

            HasOption("c=|create", "Create an entity", arg =>
            {
                if (!Enum.TryParse<CommandTypes>(arg, out _cmdType))
                    throw new ConsoleHelpAsException("Invalid entity type. Possible are " + _cmdTypeList);
            });

            HasOption("d=|delete", "Delete an entity", arg =>
            {
                if (!Enum.TryParse<CommandTypes>(arg, out _cmdType))
                    throw new ConsoleHelpAsException("Invalid entity type. Possible are " + _cmdTypeList);
            });
        }

        public override int Run(string[] remainingArguments)
        {
            throw new NotImplementedException();

            var file = SearchRoots.ByAssemblyContext("clisettings.json");

            var conf = (IConfiguration)new ConfigurationBuilder()
                .SetBasePath(file.DirectoryName)
                .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                .Build();

            var userName = "bhbk";
            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            var uow = new UnitOfWork(conf["Databases:AuroraEntities"], instance);
            var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                .Where(x => x.UserName == userName).ToLambda(),
                    new List<Expression<Func<tbl_Users, object>>>()
                    {
                        x => x.tbl_UserPasswords,
                        x => x.tbl_UserPrivateKeys,
                        x => x.tbl_UserPublicKeys
                    }).SingleOrDefault();

            var path = PathHelpers.GetUserRoot(conf, user).FullName;
            var priv = user.tbl_UserPrivateKeys.OrderBy(x => x.Created).FirstOrDefault();
            var pub = user.tbl_UserPublicKeys.OrderBy(x => x.Created).FirstOrDefault();

            //sandbox private newopenssh key format
            var privNewOpenSsh = SshPrivateKeyFormat.NewOpenSsh;
            KeyHelpers.ExportSshPrivateKey(user, priv, privNewOpenSsh, priv.KeyValuePass,
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privNewOpenSsh.ToString().ToLower() + ".txt"));
            KeyHelpers.ImportSshPrivateKey(uow, user, SignatureHashAlgorithm.SHA256, priv.KeyValuePass, Dns.GetHostName(),
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privNewOpenSsh.ToString().ToLower() + ".txt"));

            //sandbox private openssh key format
            var privOpenSsh = SshPrivateKeyFormat.OpenSsh;
            KeyHelpers.ExportSshPrivateKey(user, priv, privOpenSsh, priv.KeyValuePass,
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privOpenSsh.ToString().ToLower() + ".txt"));
            KeyHelpers.ImportSshPrivateKey(uow, user, SignatureHashAlgorithm.SHA256, priv.KeyValuePass, Dns.GetHostName(),
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privOpenSsh.ToString().ToLower() + ".txt"));

            //sandbox private pkcs8 key format
            var privPcks8 = SshPrivateKeyFormat.Pkcs8;
            KeyHelpers.ExportSshPrivateKey(user, priv, privPcks8, priv.KeyValuePass,
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privPcks8.ToString().ToLower() + ".txt"));
            KeyHelpers.ImportSshPrivateKey(uow, user, SignatureHashAlgorithm.SHA256, priv.KeyValuePass, Dns.GetHostName(),
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privPcks8.ToString().ToLower() + ".txt"));

            //sandbox private putty key format
            var privPutty = SshPrivateKeyFormat.Putty;
            KeyHelpers.ExportSshPrivateKey(user, priv, privPutty, priv.KeyValuePass,
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privPutty.ToString().ToLower() + ".txt"));
            KeyHelpers.ImportSshPrivateKey(uow, user, SignatureHashAlgorithm.SHA256, priv.KeyValuePass, Dns.GetHostName(),
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privPutty.ToString().ToLower() + ".txt"));

            //sandbox public opensshbase64 key format
            KeyHelpers.ExportSshPublicKeyBase64(user, pub,
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub.opensshbase64.txt"));
            KeyHelpers.ImportSshPublicKeyBase64(uow, user, SignatureHashAlgorithm.SHA256, Dns.GetHostName(),
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub.opensshbase64.txt"));

            //sandbox public pkcs8 key format
            var pubPkcs8 = SshPublicKeyFormat.Pkcs8;
            KeyHelpers.ExportSshPublicKey(user, pub, pubPkcs8,
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub." + pubPkcs8.ToString().ToLower() + ".txt"));
            //KeyHelpers.ImportSshPublicKey(uow, user, SignatureHashAlgorithm.SHA256, Dns.GetHostName(),
            //    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub." + pubPkcs8.ToString().ToLower() + ".txt"));

            //sandbox public ssh2base64 key format
            var pubSsh2Base64 = SshPublicKeyFormat.Ssh2Base64;
            KeyHelpers.ExportSshPublicKey(user, pub, pubSsh2Base64,
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub." + pubSsh2Base64.ToString().ToLower() + ".txt"));
            KeyHelpers.ImportSshPublicKey(uow, user, SignatureHashAlgorithm.SHA256, Dns.GetHostName(),
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub." + pubSsh2Base64.ToString().ToLower() + ".txt"));

            //sandbox public ssh2raw key format
            var pubSsh2Raw = SshPublicKeyFormat.Ssh2Raw;
            KeyHelpers.ExportSshPublicKey(user, pub, pubSsh2Raw,
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub." + pubSsh2Raw.ToString().ToLower()));
            KeyHelpers.ImportSshPublicKey(uow, user, SignatureHashAlgorithm.SHA256, Dns.GetHostName(),
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub." + pubSsh2Raw.ToString().ToLower()));
        }
    }
}
