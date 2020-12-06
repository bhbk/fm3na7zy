using Bhbk.Cli.Aurora.Factories;
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

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysKeyEditCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private Guid _id;
        private bool? _isEnabled, _isDeletable;

        public SysKeyEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("sys-key-edit", "Edit public/private key for system");

            HasRequiredOption("i|id=", "Enter GUID of public key in key pair to edit", arg =>
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
                var privKeys = _uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey>()
                    .Where(x => x.IdentityId == null).ToLambda());

                var pubKeys = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<PublicKey>()
                    .Where(x => x.IdentityId == null).ToLambda());

                var privKey = privKeys.Where(x => x.PublicKeyId == _id)
                    .SingleOrDefault();

                var pubKey = pubKeys.Where(x => x.Id == _id)
                    .SingleOrDefault();

                if (pubKey == null)
                    throw new ConsoleHelpAsException($"*** Invalid public key GUID '{_id}' ***");

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

                    OutputFactory.StdOutKeyPairs(new List<PublicKey> { pubKey }, new List<PrivateKey> { privKey });
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
