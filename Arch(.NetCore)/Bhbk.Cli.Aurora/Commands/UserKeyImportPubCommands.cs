using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using Rebex.Security.Certificates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands
{
    public class UserKeyImportPubCommands : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private FileInfo _path;
        private tbl_User _user;
        private string _pubKeyComment;
        private bool _base64;

        public UserKeyImportPubCommands()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var instance = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities"], instance);

            IsCommand("user-key-import-pub", "Import public key for user");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                    .Where(x => x.IdentityAlias == arg).ToLambda(),
                        new List<Expression<Func<tbl_User, object>>>()
                        {
                            x => x.tbl_PrivateKeys,
                            x => x.tbl_PublicKeys,
                        }).SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasRequiredOption("f|file=", "Enter file for import", arg =>
            {
                _path = new FileInfo(arg);
            });

            HasOption("c|comment=", "Enter public key comment", arg =>
            {
                _pubKeyComment = arg;
            });

            HasOption("b|base64=", "Is base64 \"authorized_keys\" format?", arg =>
            {
                _base64 = bool.Parse(arg);
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (!string.IsNullOrEmpty(_pubKeyComment))
                {
                    Console.Out.Write("  *** Enter user@hostname or a comment for the public key *** : ");
                    _pubKeyComment = StandardInput.GetInput();
                }

                Console.Out.WriteLine("Opened " + _path.FullName);
                Console.Out.WriteLine();

                if (_base64)
                    KeyHelper.ImportPubKeyBase64(_uow, _user, SignatureHashAlgorithm.SHA256, new FileInfo(_path.FullName));
                else
                    KeyHelper.ImportPubKey(_uow, _user, SignatureHashAlgorithm.SHA256, _pubKeyComment, new FileInfo(_path.FullName));

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
