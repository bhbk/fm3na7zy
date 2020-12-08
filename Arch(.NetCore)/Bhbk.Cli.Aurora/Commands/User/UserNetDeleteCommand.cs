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
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserNetDeleteCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private E_Login _user;
        private bool _deleteAll = false;

        public UserNetDeleteCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("user-net-delete", "Delete allow/deny network for user");

            HasRequiredOption("u|user=", "Enter existing user", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                    .Where(x => x.UserName == arg).ToLambda(),
                        new List<Expression<Func<E_Login, object>>>()
                        {
                            x => x.Networks,
                        })
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasOption("a|delete-all", "Delete all networks for user", arg =>
            {
                _deleteAll = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var exists = _user.Networks;

                StandardOutputFactory.Networks(exists);
                Console.Out.WriteLine();

                if (_deleteAll == true)
                {
                    Console.Out.Write("  *** Enter 'yes' to delete all networks for user *** : ");
                    var input = StandardInput.GetInput();
                    Console.Out.WriteLine();

                    if (input.ToLower() == "yes")
                    {
                        _uow.Networks.Delete(exists);
                        _uow.Commit();
                    }
                }
                else
                {
                    Console.Out.Write("  *** Enter GUID of network for user to delete *** : ");
                    var input = Guid.Parse(StandardInput.GetInput());

                    var network = exists.Where(x => x.Id == input)
                        .SingleOrDefault();

                    if (network != null)
                    {
                        _uow.Networks.Delete(network);
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
