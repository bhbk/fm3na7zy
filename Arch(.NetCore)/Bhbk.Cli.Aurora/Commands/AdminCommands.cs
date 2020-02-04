using System;
using System.Collections.Generic;
using System.Text;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Hashing;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebex;
using Rebex.Net;
using Rebex.Security.Cryptography;
using Serilog;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

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
            var priv = user.tbl_UserPrivateKeys.First();
            var pub = user.tbl_UserPublicKeys.First();

            KeyHelpers.ExportSshPrivateKey(user, priv, SshPrivateKeyFormat.Pkcs8, priv.KeyValuePass,
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pkcs8.key.txt"));

            KeyHelpers.ExportSshPrivateKey(user, priv, SshPrivateKeyFormat.OpenSsh, priv.KeyValuePass,
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".openssh.key.txt"));

            KeyHelpers.ExportSshPublicKey(user, pub, SshPublicKeyFormat.Pkcs8,
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pkcs8.pub.txt"));

            KeyHelpers.ExportSshPublicKey(user, pub, SshPublicKeyFormat.Ssh2Base64,
                new FileInfo(path + Path.DirectorySeparatorChar + userName + ".ssh2base64.pub.txt"));
        }
    }
}
