using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
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
        private Login_EF _user;
        private Guid _id;
        private FileSystemType_E _fileSystem;
        private readonly string _fileSystemList = string.Join(", ", Enum.GetNames(typeof(FileSystemType_E)));
        private string _alias, _comment;
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

                _user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                    .Where(x => x.UserId == _id).ToLambda(),
                        new List<Expression<Func<Login_EF, object>>>()
                        {
                            x => x.Alerts,
                            x => x.Networks,
                            x => x.PrivateKeys,
                            x => x.PublicKeys,
                            x => x.Sessions,
                            x => x.Settings,
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

                _alias = arg;
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

            HasOption("c|comment=", "Enter new comment", arg =>
            {
                CheckRequiredArguments();

                if (!string.IsNullOrEmpty(arg))
                    _comment = arg;
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
                /*
                 * when login already exists do not allow rename...
                 */
                if (_alias != null)
                {
                    if (_uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                        .Where(x => x.UserName.ToLower() == _alias.ToLower()).ToLambda())
                        .Any())
                        throw new ConsoleHelpAsException($"  *** The alias '{_user.UserName}' already exists ***");

                    _user.UserName = _alias.ToLower();
                }

                if (_isEnabled.HasValue)
                    _user.IsEnabled = _isEnabled.Value;

                if (_isDeletable.HasValue)
                    _user.IsDeletable = _isDeletable.Value;

                _uow.Logins.Update(_user);
                _uow.LoginUsages.Update(_user.Usage);
                _uow.Commit();

                FormatOutput.Write(_user, true);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
