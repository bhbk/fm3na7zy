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
    public class UserAlertDeleteCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private User _user;
        private bool _deleteAll = false;

        public UserAlertDeleteCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("user-alert-delete", "Delete alert for user");

            HasRequiredOption("u|user=", "Enter existing user", arg =>
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

            HasOption("a|delete-all", "Delete all alerts for user", arg =>
            {
                _deleteAll = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var exists = _user.Alerts;

                OutputFactory.StdOutUserAlerts(exists);
                Console.Out.WriteLine();

                if (_deleteAll == true)
                {
                    Console.Out.Write("  *** Enter 'yes' to delete all alerts for user *** : ");
                    var input = StandardInput.GetInput();
                    Console.Out.WriteLine();

                    if (input.ToLower() == "yes")
                    {
                        _uow.UserAlerts.Delete(exists);
                        _uow.Commit();
                    }
                }
                else
                {
                    Console.Out.Write("  *** Enter GUID of alert for user to delete *** : ");
                    var input = Guid.Parse(StandardInput.GetInput());

                    var alert = exists.Where(x => x.Id == input)
                        .SingleOrDefault();

                    if (alert != null)
                    {
                        _uow.UserAlerts.Delete(alert);
                        _uow.Commit();
                    }
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