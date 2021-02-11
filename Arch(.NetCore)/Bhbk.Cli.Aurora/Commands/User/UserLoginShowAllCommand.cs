using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.QueryExpression;
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
    public class UserLoginShowAllCommand : ConsoleCommand
    {
        private IConfiguration _conf;
        private IUnitOfWork _uow;
        private string _filter;
        private int _count;

        public UserLoginShowAllCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("user-login-show-all", "Show all login(s) for user(s)");

            HasRequiredOption("c|count=", "Enter how many user login(s) to display", arg =>
            {
                if (!string.IsNullOrEmpty(arg))
                    _count = int.Parse(arg);
            });

            HasOption("f|filter=", "Enter user (full or partial) login to look for", arg =>
            {
                CheckRequiredArguments();

                if (!string.IsNullOrEmpty(arg))
                    _filter = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                IQueryExpression<Login_EF> expression =
                    QueryExpressionFactory.GetQueryExpression<Login_EF>();

                if (!string.IsNullOrEmpty(_filter))
                    expression = expression.Where(x => x.UserName.Contains(_filter));

                var results = _uow.Logins.Get(expression.ToLambda(),
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
                    .OrderBy(x => x.UserName);

                var users = results.TakeLast(_count);

                if (results.Count() != users.Count())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Out.WriteLine($"  *** Showing {_count} user(s). Not showing {results.Count() - users.Count()} user(s) ***");
                    Console.Out.WriteLine();
                    Console.ResetColor();
                }

                if (users.Where(x => x.AuthTypeId == (int)AuthType_E.Identity).Any())
                {
                    Console.Out.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Out.WriteLine($"  [incoming (identity) user(s)]");
                    Console.ResetColor();

                    foreach (var user in users.Where(x => x.AuthTypeId == (int)AuthType_E.Identity))
                        FormatOutput.Write(user, false);
                }

                if (users.Where(x => x.AuthTypeId == (int)AuthType_E.Local).Any())
                {
                    Console.Out.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Out.WriteLine($"  [outgoing (local) user(s)]");
                    Console.ResetColor();

                    foreach (var user in users.Where(x => x.AuthTypeId == (int)AuthType_E.Local))
                        FormatOutput.Write(user, false);
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
