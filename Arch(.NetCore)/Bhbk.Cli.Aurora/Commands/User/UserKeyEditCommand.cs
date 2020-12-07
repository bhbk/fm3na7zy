﻿using Bhbk.Cli.Aurora.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.CommandLine.IO;
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

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserKeyEditCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private UserLogin _user;
        private Guid _id;
        private bool? _isEnabled, _isDeletable;

        public UserKeyEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("user-key-edit", "Edit public/private key for user");

            HasRequiredOption("u|user=", "Enter user that already exists", arg =>
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

            HasRequiredOption("i|id=", "Enter GUID of network to edit", arg =>
            {
                _id = Guid.Parse(arg);
            });

            HasOption("e|enabled=", "Is enabled", arg =>
            {
                _isEnabled = bool.Parse(arg);
            });

            HasOption("d|deletable=", "Is deletable", arg =>
            {
                _isDeletable = bool.Parse(arg);
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var privKeys = _user.PrivateKeys;
                var pubKeys = _user.PublicKeys;

                var pubKey = pubKeys.Where(x => x.Id == _id)
                    .SingleOrDefault();

                var privKey = privKeys.Where(x => x.PublicKeyId == _id)
                    .SingleOrDefault();

                if (pubKey == null)
                    throw new ConsoleHelpAsException($"  *** Invalid public key GUID '{_id}' ***");

                if (pubKey != null)
                {
                    if (privKey != null)
                    {
                        if (_isEnabled.HasValue)
                            privKey.IsEnabled = _isEnabled.Value;

                        if (_isDeletable.HasValue)
                            privKey.IsDeletable = _isDeletable.Value;

                        _uow.PrivateKeys.Update(privKey);
                    }

                    if (_isEnabled.HasValue)
                        pubKey.IsEnabled = _isEnabled.Value;

                    if (_isDeletable.HasValue)
                        pubKey.IsDeletable = _isDeletable.Value;

                    _uow.PublicKeys.Update(pubKey);
                    _uow.Commit();

                    StandardOutputFactory.KeyPairs(new List<PublicKey> { pubKey }, new List<PrivateKey> { privKey });
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
