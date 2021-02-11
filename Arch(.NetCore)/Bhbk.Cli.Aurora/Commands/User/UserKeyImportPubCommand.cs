using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
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

namespace Bhbk.Cli.Aurora.Commands.User
{
    public class UserKeyImportPubCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private FileInfo _path;
        private Login_EF _user;
        private bool _base64;
        private string _pubKeyComment;

        public UserKeyImportPubCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("user-key-import-pub", "Import public key for user");

            HasRequiredOption("u|user=", "Enter user that exists already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user name given ***");

                _user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                    .Where(x => x.UserName == arg).ToLambda(),
                        new List<Expression<Func<Login_EF, object>>>()
                        {
                            x => x.PrivateKeys,
                            x => x.PublicKeys
                        })
                    .SingleOrDefault();

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

            HasOption("b|base64", "Is base64 \"authorized_keys\" format", arg =>
            {
                _base64 = true;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (string.IsNullOrEmpty(_pubKeyComment))
                {
                    Console.Out.WriteLine();
                    Console.Out.Write("  *** Enter user@hostname or a comment for the public key *** : ");
                    _pubKeyComment = StandardInput.GetInput();
                }

                Console.Out.WriteLine();
                Console.Out.WriteLine("Opened " + _path.FullName);
                Console.Out.WriteLine();

                var pubKeys = new List<PublicKey_EF>();
                var stream = new MemoryStream();

                using (FileStream fileStream = new FileStream(_path.FullName, FileMode.Open, FileAccess.Read))
                    fileStream.CopyTo(stream);

                if (_base64)
                    pubKeys = KeyHelper.ImportPubKeyBase64(_uow, _user, SignatureHashAlgorithm.SHA256, stream).ToList();
                else
                {
                    var pubKey = KeyHelper.ImportPubKey(_uow, _user, SignatureHashAlgorithm.SHA256, _pubKeyComment, stream);

                    if (pubKey != null)
                        pubKeys = new List<PublicKey_EF>() { pubKey };
                }

                if (pubKeys != null)
                {
                    _uow.PublicKeys.Create(pubKeys);
                    _uow.Commit();

                    foreach (var pubKey in pubKeys.OrderBy(x => x.CreatedUtc))
                        FormatOutput.Write(pubKey, _user.PrivateKeys.Where(x => x.PublicKeyId == pubKey.Id).SingleOrDefault(), true);
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
