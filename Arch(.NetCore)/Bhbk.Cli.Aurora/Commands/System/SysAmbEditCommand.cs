using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Encryption;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands.System
{
    public class SysAmbEditCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private Ambassador_EF _ambassador;
        private string _upn;

        public SysAmbEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("sys-amb-edit", "Edit ambassador credential on system");

            HasRequiredOption("a|ambassador=", "Enter existing ambassador credential", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No ambassador credential given ***");

                _ambassador = _uow.Ambassadors.Get(QueryExpressionFactory.GetQueryExpression<Ambassador_EF>()
                    .Where(x => x.UserPrincipalName == arg).ToLambda())
                    .SingleOrDefault();

                if (_ambassador == null)
                    throw new ConsoleHelpAsException($"  *** Invalid ambassador credential '{arg}' ***");
            });

            HasOption("u|upn=", "Enter user principal name", arg =>
            {
                CheckRequiredArguments();

                _upn = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                /*
                 * when ambassador upn already exists do not allow rename...
                 */

                if (_upn != null)
                {
                    if (_uow.Ambassadors.Get(QueryExpressionFactory.GetQueryExpression<Ambassador_EF>()
                        .Where(x => x.UserPrincipalName.ToLower() == _upn.ToLower()).ToLambda())
                        .Any())
                        throw new ConsoleHelpAsException($"  *** The ambassador '{_ambassador.UserPrincipalName}' already exists ***");

                    _ambassador.UserPrincipalName = _upn.ToLower();
                }

                Console.Out.Write("  *** Enter credential password to use *** : ");
                var credPass = StandardInput.GetHiddenInput();

                Console.Out.WriteLine();

                var secret = _conf["Databases:AuroraSecret"];
                var encryptedPass = AES.EncryptString(credPass, secret);
                var decryptedPass = AES.DecryptString(encryptedPass, secret);

                if (credPass != decryptedPass)
                    throw new ArithmeticException();

                if (_upn != null)
                    _ambassador.UserPrincipalName = _upn;

                if (credPass != null)
                    _ambassador.EncryptedPass = encryptedPass;

                _ambassador = _uow.Ambassadors.Update(_ambassador);
                _uow.Commit();

                _ambassador = _uow.Ambassadors.Get(QueryExpressionFactory.GetQueryExpression<Ambassador_EF>()
                    .Where(x => x.Id == _ambassador.Id).ToLambda(),
                        new List<Expression<Func<Ambassador_EF, object>>>()
                        {
                            x => x.FileSystems,
                        })
                    .SingleOrDefault();

                FormatOutput.Write(_ambassador);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
