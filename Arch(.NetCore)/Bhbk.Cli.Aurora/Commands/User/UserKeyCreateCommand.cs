﻿using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
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
using System.Net;

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

            HasRequiredOption("a|alg=", "Enter key algorithm", arg =>
            {
                if (!Enum.TryParse(arg, out _keyAlgo))
                    throw new ConsoleHelpAsException($"  *** Invalid key algorithm. Options are '{_keyAlgoList}' ***");
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

            HasOption("d|dns=", "Enter public key comment", arg =>
            {
                _pubKeyComment = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (string.IsNullOrEmpty(_privKeyPass))
                {
                    _privKeyPass = AlphaNumeric.CreateString(32);
                    Console.Out.WriteLine($"  *** The password for the private key *** : {_privKeyPass}");
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

                var keyPair = KeyHelper.CreateKeyPair(_conf, _uow, _user, _keyAlgo, SignatureHashAlgorithm.SHA256, _privKeySize, _privKeyPass, _pubKeyComment);

                if (keyPair.Item1 != null)
                    _uow.PublicKeys.Create(keyPair.Item1);

                _uow.Commit();

                if (keyPair.Item2 != null)
                    _uow.PrivateKeys.Create(keyPair.Item2);

                _uow.Commit();

                FormatOutput.KeyPairs(new List<PublicKey_EF> { keyPair.Item1 }, new List<PrivateKey_EF> { keyPair.Item2 });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
