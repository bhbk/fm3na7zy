using Bhbk.Cli.Aurora.Helpers;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Cryptography.Entropy;
using Rebex.Net;
using Rebex.Security.Certificates;
using System.Net;
using System.Text;

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysKeysImportCommands : ConsoleCommand
    {
        private static IConfiguration _conf;
        private static IUnitOfWork _uow;
        private static FileInfo _path;
        private static string _privKeyPass;
        private static SshHostKeyAlgorithm _keyAlgo;
        private static SignatureHashAlgorithm _sigAlgo;
        private static string _keyAlgoList = string.Join(", ", Enum.GetNames(typeof(SshHostKeyAlgorithm)));
        private static string _sigAlgoList = string.Join(", ", Enum.GetNames(typeof(SignatureHashAlgorithm)));

        public SysKeysImportCommands()
        {
            IsCommand("sys-keys-import", "Import public/private key pairs for system");

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
                _conf = (IConfiguration)new ConfigurationBuilder()
                    .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var instance = new ContextService(InstanceContext.DeployedOrLocal);
                _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
