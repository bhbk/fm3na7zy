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
    public class UserKeyDeleteCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private User _user;
        private bool _deleteAll = false;

        public UserKeyDeleteCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("user-key-delete", "Delete public/private key for user");

            HasRequiredOption("u|user=", "Enter existing user", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                    .Where(x => x.IdentityAlias == arg).ToLambda(),
                        new List<Expression<Func<User, object>>>()
                        {
                            x => x.PrivateKeys,
                            x => x.PublicKeys,
                        })
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasOption("d|delete-all", "Delete all public/private key pairs for user", arg =>
            {
                _deleteAll = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var pubKeys = _user.PublicKeys.Where(x => x.IsDeletable == true);
                var privKeys = _user.PrivateKeys.Where(x => x.IsDeletable == true);

                OutputFactory.StdOutKeyPairs(pubKeys.OrderBy(x => x.CreatedUtc), privKeys);
                Console.Out.WriteLine();

                if (_deleteAll == true)
                {
                    Console.Out.Write("  *** Enter 'yes' to delete all public/private key pairs for user *** : ");
                    var input = StandardInput.GetInput();
                    Console.Out.WriteLine();

                    if (input.ToLower() == "yes")
                    {
                        _uow.PrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<PrivateKey>()
                            .Where(x => x.IdentityId == _user.IdentityId && x.IsDeletable == true).ToLambda());

                        _uow.Commit();

                        _uow.PublicKeys.Delete(QueryExpressionFactory.GetQueryExpression<PublicKey>()
                            .Where(x => x.IdentityId == _user.IdentityId && x.IsDeletable == true).ToLambda());

                        _uow.Commit();
                    }
                }
                else
                {
                    Console.Out.Write("  *** Enter GUID of public key to delete *** : ");
                    var input = Guid.Parse(StandardInput.GetInput());

                    var pubKey = pubKeys.Where(x => x.Id == input)
                        .SingleOrDefault();

                    if (pubKey != null)
                    {
                        _uow.PrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<PrivateKey>()
                            .Where(x => x.IdentityId == _user.IdentityId && x.Id == pubKey.PrivateKeyId && x.IsDeletable == true).ToLambda());

                        _uow.Commit();

                        _uow.PublicKeys.Delete(QueryExpressionFactory.GetQueryExpression<PublicKey>()
                            .Where(x => x.IdentityId == _user.IdentityId && x.Id == pubKey.Id && x.IsDeletable == true).ToLambda());

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
