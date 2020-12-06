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
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserAlertCreateCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private User _user;
        private string _displayName, _emailAddress, _textAddress;
        private bool _delete = false, _download = false, _upload = false;

        public UserAlertCreateCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("user-alert-create", "Create alert for user");

            HasRequiredOption("u|user=", "Enter user that already exists", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                    .Where(x => x.IdentityAlias == arg).ToLambda(),
                        new List<Expression<Func<User, object>>>()
                        {
                            x => x.Alerts,
                        })
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasRequiredOption("d|display-name=", "Enter display name for recipient", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _displayName = arg;
            });

            HasOption("e|email=", "Enter email address for recipient", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _emailAddress = arg;

                if (!new EmailAddressAttribute().IsValid(_emailAddress))
                    throw new ConsoleHelpAsException($"  *** Invalid email address '{_emailAddress}' ***");
            });

            HasOption("t|text=", "Enter phone number (e.164 format) for recipient", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _textAddress = arg;

                try
                {
                    var check = PhoneNumberUtil.GetInstance();
                    var phoneNumber = check.Parse(_textAddress, null);

                    if (!check.IsValidNumber(phoneNumber))
                        throw new Exception();
                }
                catch (Exception)
                {
                    throw new ConsoleHelpAsException($"  *** Invalid phone number '{_textAddress}' (not e.164 format) ***");
                }
            });

            HasOption("x|on-delete", "Is sent when delete happens", arg =>
            {
                _delete = true;
            });

            HasOption("y|on-download", "Is sent when download happens", arg =>
            {
                _download = true;
            });

            HasOption("z|on-upload", "Is sent when upload happens", arg =>
            {
                _upload = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (!string.IsNullOrEmpty(_emailAddress))
                {
                    var found = _user.Alerts.Where(x => x.ToEmailAddress == _emailAddress)
                        .SingleOrDefault();

                    if (found != null)
                    {
                        Console.Out.WriteLine("  *** The alert entered already exists for user ***");
                        OutputFactory.StdOutUserAlerts(new List<UserAlert> { found });

                        return StandardOutput.FondFarewell();
                    }
                }

                if (!string.IsNullOrEmpty(_textAddress))
                {
                    var found = _user.Alerts.Where(x => x.ToPhoneNumber == _textAddress)
                        .SingleOrDefault();

                    if (found != null)
                    {
                        Console.Out.WriteLine("  *** The alert entered already exists for user ***");
                        OutputFactory.StdOutUserAlerts(new List<UserAlert> { found });

                        return StandardOutput.FondFarewell();
                    }
                }

                var alert = _uow.UserAlerts.Create(
                    new UserAlert
                    {
                        IdentityId = _user.IdentityId,
                        OnDelete = _delete,
                        OnDownload = _download,
                        OnUpload = _upload,
                        ToDisplayName = _displayName,
                        ToEmailAddress = _emailAddress ?? null,
                        ToPhoneNumber = _textAddress ?? null,
                        IsEnabled = false,
                    });

                _uow.Commit();

                OutputFactory.StdOutUserAlerts(new List<UserAlert> { alert });

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
