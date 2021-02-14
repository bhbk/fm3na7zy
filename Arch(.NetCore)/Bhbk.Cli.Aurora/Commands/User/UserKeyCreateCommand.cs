using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
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
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace Bhbk.Cli.Aurora.Commands.User
{
    public class UserKeyCreateCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private Login_EF _user;
        private int _privKeySize;
        private SshHostKeyAlgorithm _keyAlgo;
        private string _keyAlgoList = string.Join(", ", Enum.GetNames(typeof(SshHostKeyAlgorithm))
            .Where(x => x.Equals("rsa", StringComparison.OrdinalIgnoreCase)
                || x.Equals("dss", StringComparison.OrdinalIgnoreCase)
                || x.Equals("ed25519", StringComparison.OrdinalIgnoreCase)
                || x.Equals("ecdsanistp256", StringComparison.OrdinalIgnoreCase)
                || x.Equals("ecdsanistp384", StringComparison.OrdinalIgnoreCase)
                || x.Equals("ecdsanistp521", StringComparison.OrdinalIgnoreCase)));
        private string _privKeyPass;
        private string _pubKeyComment;

        public UserKeyCreateCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("user-key-create", "Create public/private key for user");

            HasRequiredOption("u|user=", "Enter user that already exists", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                    .Where(x => x.UserName == arg).ToLambda(),
                        new List<Expression<Func<Login_EF, object>>>()
                        {
                            x => x.PrivateKeys,
                            x => x.PublicKeys,
                        })
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasRequiredOption("a|algorithm=", "Enter key algorithm", arg =>
            {
                if (!Enum.TryParse(arg, out _keyAlgo))
                    throw new ConsoleHelpAsException($"  *** Invalid key algorithm. Options are '{_keyAlgoList}' ***");
            });

            HasRequiredOption("s|size=", "Enter key size", arg =>
            {
                if (!int.TryParse(arg, out _privKeySize))
                    throw new ConsoleHelpAsException($"  *** Invalid key size '{_privKeySize}' ***");
            });

            HasOption("p|passphrase=", "Enter private key password", arg =>
            {
                CheckRequiredArguments();

                _privKeyPass = arg;
            });

            HasOption("c|comment=", "Enter public key comment", arg =>
            {
                CheckRequiredArguments();

                _pubKeyComment = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                if (string.IsNullOrEmpty(_privKeyPass))
                {
                    _privKeyPass = AlphaNumeric.CreateString(32);
                    Console.Out.WriteLine($"  *** The private key password *** : {_privKeyPass}");
                    Console.Out.WriteLine();
                }
                else
                {
                    Console.Out.Write("  *** Enter password for the private key *** : ");
                    _privKeyPass = StandardInput.GetHiddenInput();
                    Console.Out.WriteLine();
                }

                if (string.IsNullOrEmpty(_pubKeyComment))
                    _pubKeyComment = Dns.GetHostName();

                var (pubKey, privKey) = KeyHelper.CreateKeyPair(_conf, _uow, _user, _keyAlgo, SignatureHashAlgorithm.SHA256, _privKeySize, _privKeyPass, _pubKeyComment);

                if (pubKey != null)
                    _uow.PublicKeys.Create(pubKey);

                _uow.Commit();

                if (privKey != null)
                    _uow.PrivateKeys.Create(privKey);

                _uow.Commit();

                if (pubKey != null)
                    Log.Information($"{callPath} '{_user.UserName}' creating new public key... " +
                        $"{Environment.NewLine} [algo] {(SshHostKeyAlgorithm)pubKey.KeyAlgorithmId} [format] {(SshPublicKeyFormat)pubKey.KeyFormatId} " +
                        $"{Environment.NewLine} [sig] {pubKey.SigValue}" +
                        $"{Environment.NewLine}{pubKey.KeyValue}");

                if (privKey != null)
                    Log.Information($"{callPath} '{_user.UserName}' creating new private key... " +
                        $"{Environment.NewLine} [algo] {(SshHostKeyAlgorithm)privKey.KeyAlgorithmId} [format] {(SshPrivateKeyFormat)privKey.KeyFormatId} " +
                        $"{Environment.NewLine}{privKey.KeyValue}");

                FormatOutput.Write(pubKey, privKey, true);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
