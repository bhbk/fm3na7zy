using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
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

namespace Bhbk.Cli.Aurora.Commands.System
{
    public class SysKeyEditCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private IEnumerable<PublicKey_EF> _pubKeys;
        private IEnumerable<PrivateKey_EF> _privKeys;
        private PublicKey_EF _pubKey;
        private PrivateKey_EF _privKey;
        private bool? _isEnabled, _isDeletable;

        public SysKeyEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-key-edit", "Edit public/private key for system");

            HasRequiredOption("i|id=", "Enter GUID of public key in key pair to edit", arg =>
            {
                var id = Guid.Parse(arg);

                _privKeys = _uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey_EF>()
                    .Where(x => x.UserId == null).ToLambda());

                _pubKeys = _uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<PublicKey_EF>()
                    .Where(x => x.UserId == null).ToLambda());

                _privKey = _privKeys.Where(x => x.PublicKeyId == id)
                    .SingleOrDefault();

                _pubKey = _pubKeys.Where(x => x.Id == id)
                    .SingleOrDefault();

                if (_pubKey == null)
                    throw new ConsoleHelpAsException($"*** Invalid public key GUID '{id}' ***");
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
                if (_pubKey != null)
                {
                    if (_privKey != null)
                    {
                        if (_isEnabled.HasValue)
                            _privKey.IsEnabled = _isEnabled.Value;

                        if (_isDeletable.HasValue)
                            _privKey.IsDeletable = _isDeletable.Value;

                        _uow.PrivateKeys.Update(_privKey);
                    }

                    if (_isEnabled.HasValue)
                        _pubKey.IsEnabled = _isEnabled.Value;

                    if (_isDeletable.HasValue)
                        _pubKey.IsDeletable = _isDeletable.Value;

                    _uow.PublicKeys.Update(_pubKey);
                    _uow.Commit();

                    FormatOutput.Write(_pubKey, _privKey, true);
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
