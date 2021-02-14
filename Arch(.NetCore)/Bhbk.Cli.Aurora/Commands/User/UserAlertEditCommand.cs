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
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands.User
{
    public class UserAlertEditCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private Login_EF _user;
        private Guid _id;
        private string _displayName, _emailAddress, _phoneNumber, _comment;
        private bool? _delete, _download, _upload, _isEnabled;

        public UserAlertEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("user-alert-edit", "Edit alert for user");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                    .Where(x => x.UserName == arg).ToLambda(),
                        new List<Expression<Func<Login_EF, object>>>()
                        {
                            x => x.Alerts,
                        })
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasRequiredOption("i|id=", "Enter GUID of alert to edit", arg =>
            {
                _id = Guid.Parse(arg);
            });

            HasOption("d|display-name=", "Enter display name for recipient", arg =>
            {
                CheckRequiredArguments();

                if (!string.IsNullOrEmpty(arg))
                    _displayName = arg;
            });

            HasOption("m|email=", "Enter email address for recipient", arg =>
            {
                CheckRequiredArguments();

                if (!string.IsNullOrEmpty(arg))
                    _emailAddress = arg;

                if (!new EmailAddressAttribute().IsValid(_emailAddress))
                    throw new ConsoleHelpAsException($"  *** Invalid email address '{_emailAddress}' ***");
            });

            HasOption("t|text=", "Enter phone number (e.164 format) for recipient", arg =>
            {
                CheckRequiredArguments();

                if (!string.IsNullOrEmpty(arg))
                    _phoneNumber = arg;

                try
                {
                    var check = PhoneNumberUtil.GetInstance();
                    var phoneNumber = check.Parse(_phoneNumber, null);

                    if (!check.IsValidNumber(phoneNumber))
                        throw new Exception();
                }
                catch (Exception)
                {
                    throw new ConsoleHelpAsException($"  *** Invalid phone number '{_phoneNumber}' (not e.164 format) ***");
                }
            });

            HasOption("c|comment=", "Enter comment", arg =>
            {
                CheckRequiredArguments();

                if (!string.IsNullOrEmpty(arg))
                    _comment = arg;
            });

            HasOption("x|on-delete=", "Is sent when delete happens", arg =>
            {
                CheckRequiredArguments();

                _delete = bool.Parse(arg);
            });

            HasOption("y|on-download=", "Is sent when download happens", arg =>
            {
                CheckRequiredArguments();

                _download = bool.Parse(arg);
            });

            HasOption("z|on-upload=", "Is sent when upload happens", arg =>
            {
                CheckRequiredArguments();

                _upload = bool.Parse(arg);
            });

            HasOption("e|enabled=", "Is enabled", arg =>
            {
                CheckRequiredArguments();

                _isEnabled = bool.Parse(arg);
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var alert = _user.Alerts.Where(x => x.Id == _id)
                    .SingleOrDefault();

                if (alert == null)
                    throw new ConsoleHelpAsException($"  *** Invalid alert GUID '{_id}' ***");

                if (!string.IsNullOrEmpty(_displayName))
                    alert.ToDisplayName = _displayName;

                if (!string.IsNullOrEmpty(_emailAddress))
                    alert.ToEmailAddress = _emailAddress;

                if (!string.IsNullOrEmpty(_phoneNumber))
                    alert.ToPhoneNumber = _phoneNumber;

                if (_delete.HasValue)
                    alert.OnDelete = _delete.Value;

                if (_download.HasValue)
                    alert.OnDownload = _download.Value;

                if (_upload.HasValue)
                    alert.OnUpload = _upload.Value;

                if (_isEnabled.HasValue)
                    alert.IsEnabled = _isEnabled.Value;

                _uow.Alerts.Update(alert);
                _uow.Commit();

                FormatOutput.Write(alert, true);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
