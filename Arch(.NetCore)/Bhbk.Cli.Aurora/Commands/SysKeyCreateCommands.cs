using Bhbk.Cli.Aurora.Factories;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Aurora.Data_EF6.Models;
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

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysKeyCreateCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private SshHostKeyAlgorithm _keyAlgo;
        private readonly string _keyAlgoList = string.Join(", ", Enum.GetNames(typeof(SshHostKeyAlgorithm)));
        private string _privKeyPass;
        private int _privKeySize;

        public SysKeyCreateCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-key-create", "Create private/public key for system");

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

            HasOption("p|pass=", "Enter private key password", arg =>
            {
                _privKeyPass = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
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

                var privKey = KeyHelper.CreatePrivKey(_conf, _uow, _keyAlgo, _privKeySize, _privKeyPass, SignatureHashAlgorithm.SHA256);

                var pubKey = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<PublicKey>()
                    .Where(x => x.PrivateKeyId == privKey.Id).ToLambda(),
                        new List<Expression<Func<PublicKey, object>>>()
                        {
                            x => x.PrivateKey,
                        })
                    .Single();

                OutputFactory.StdOutKeyPairs(new List<PublicKey>() { pubKey });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
