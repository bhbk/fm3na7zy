using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Aurora.Primitives.Enums;
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

namespace Bhbk.Cli.Aurora.Commands.User
{
    public class UserLoginEditCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private E_Login _user;
        private Guid _id;
        private FileSystemProviderType _fileSystem;
        private readonly string _fileSystemList = string.Join(", ", Enum.GetNames(typeof(FileSystemProviderType)));
        private bool? _isEnabled, _isDeletable;

        public UserLoginEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("user-login-edit", "Edit login for user");

            HasRequiredOption("i|id=", "Enter GUID of user to edit", arg =>
            {
                _id = Guid.Parse(arg);

                _user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                    .Where(x => x.UserId == _id).ToLambda(),
                        new List<Expression<Func<E_Login, object>>>()
                        {
                            x => x.Files,
                            x => x.Folders,
                            x => x.Mount,
                            x => x.Networks,
                            x => x.PrivateKeys,
                            x => x.PublicKeys,
                            x => x.Usage,
                        })
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' or immutable ***");
            });

            HasOption("a|alias=", "Enter alias", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No alias given ***");

                _user.UserName = arg;
            });

            HasOption("f|filesystem=", "Enter type of filesystem", arg =>
            {
                if (!Enum.TryParse(arg, out _fileSystem))
                    throw new ConsoleHelpAsException($"  *** Invalid filesystem type. Options are '{_fileSystemList}' ***");

                _user.FileSystemType = _fileSystem.ToString();
            });

            HasOption("c|chroot=", "Enter chroot path", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No chroot path given ***");

                _user.FileSystemChrootPath = arg;
            });

            HasOption("k|publickey=", "Require public key for authentication", arg =>
            {
                _user.IsPublicKeyRequired = bool.Parse(arg);
            });

            HasOption("p|password=", "Require password for authentication", arg =>
            {
                _user.IsPasswordRequired = bool.Parse(arg);
            });

            HasOption("s|session=", "Enter session maximum", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No session maximum given ***");

                _user.Usage.SessionMax = Int16.Parse(arg);
            });

            HasOption("q|quota=", "Enter quota maximum (in bytes)", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No quota maximum given ***");

                _user.Usage.QuotaInBytes = Int32.Parse(arg);
            });

            HasOption("e|enabled=", "Is user enabled", arg =>
            {
                _isEnabled = bool.Parse(arg);
            });

            HasOption("d|deletable=", "Is user deletable", arg =>
            {
                _isDeletable = bool.Parse(arg);
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (_isEnabled.HasValue)
                    _user.IsEnabled = _isEnabled.Value;

                if (_isDeletable.HasValue)
                    _user.IsDeletable = _isDeletable.Value;

                _uow.Logins.Update(_user);
                _uow.Usages.Update(_user.Usage);
                _uow.Commit();

                FormatOutput.Logins(new List<E_Login> { _user }, true);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
