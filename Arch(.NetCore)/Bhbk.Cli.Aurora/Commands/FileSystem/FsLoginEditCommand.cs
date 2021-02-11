using Bhbk.Cli.Aurora.IO;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands
{
    public class FsLoginEditCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private Ambassador_EF _ambassador = null;
        private FileSystem_EF _fileSystem = null;
        private FileSystemLogin_EF _fileSystemLogin = null;
        private Login_EF _user = null;
        private string _chrootPath;
        private int _smbAuthType = Int32.MinValue;
        private AuthType _smbAuthTypeEnum;
        private string _smbAuthTypeList = string.Join(", ", Enum.GetNames(typeof(AuthType))
            .Where(x => x.Equals("anonymous", StringComparison.OrdinalIgnoreCase)
                || x.Equals("basic", StringComparison.OrdinalIgnoreCase)
                || x.Equals("negotiate", StringComparison.OrdinalIgnoreCase)
                || x.Equals("ntlm", StringComparison.OrdinalIgnoreCase)));
        private bool? _isReadOnly;

        public FsLoginEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("fs-login-edit", "Edit file-system membership for user");

            HasRequiredOption("f|file-system=", "Enter existing file-system group", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No file-system group given ***");

                _fileSystem = _uow.FileSystems.Get(QueryExpressionFactory.GetQueryExpression<FileSystem_EF>()
                    .Where(x => x.Name == arg).ToLambda(),
                        new List<Expression<Func<FileSystem_EF, object>>>()
                        {
                            x => x.FileSystemType,
                            x => x.Files,
                            x => x.Folders,
                            x => x.Usage,
                        })
                    .SingleOrDefault();

                if (_fileSystem == null)
                    throw new ConsoleHelpAsException($"  *** Invalid file-system group '{arg}' ***");
            });

            HasRequiredOption("u|user=", "Enter existing user", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No user given ***");

                _user = _uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<Login_EF>()
                    .Where(x => x.UserName == arg && x.IsDeletable == true).ToLambda())
                    .SingleOrDefault();

                if (_user == null)
                    throw new ConsoleHelpAsException($"  *** Invalid user '{arg}' ***");
            });

            HasOption("t|auth-type=", $"Enter auth type (only for '{FileSystemType_E.SMB}' file-system)", arg =>
            {
                CheckRequiredArguments();

                if (!Enum.TryParse(arg, true, out _smbAuthTypeEnum))
                    throw new ConsoleHelpAsException($"  *** Invalid auth type, options are '{_smbAuthTypeList}' ***");

                _smbAuthType = (int)_smbAuthTypeEnum;
            });

            HasOption("a|ambassador=", $"Enter ambassador credential to use (only for '{FileSystemType_E.SMB}' file-system)", arg =>
            {
                CheckRequiredArguments();

                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No ambassador given ***");

                _ambassador = _uow.Ambassadors.Get(QueryExpressionFactory.GetQueryExpression<Ambassador_EF>()
                    .Where(x => x.UserPrincipalName == arg && x.IsDeletable == true).ToLambda())
                    .SingleOrDefault();

                if (_ambassador == null)
                    throw new ConsoleHelpAsException($"  *** Invalid ambassador '{arg}' ***");
            });

            HasOption("c|chroot-path=", "Enter chroot path", arg =>
            {
                CheckRequiredArguments();

                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No chroot path given ***");

                if (!FilePathHelper.IsValidPosixPath(arg.ToLower()))
                    throw new ConsoleHelpAsException($"  *** Invalid posix compliant path '{arg}' ***");

                _chrootPath = arg.ToLower();
            });

            HasOption("r|readonly=", "Is read-only?", arg =>
            {
                CheckRequiredArguments();

                _isReadOnly = bool.Parse(arg);
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                _fileSystemLogin = _uow.FileSystemLogins.Get(QueryExpressionFactory.GetQueryExpression<FileSystemLogin_EF>()
                    .Where(x => x.UserId == _user.UserId && x.FileSystemId == _fileSystem.Id).ToLambda())
                    .SingleOrDefault();

                if (_fileSystemLogin == null)
                    throw new ConsoleHelpAsException($"  *** No membership for user '{_user.UserName}' in file-system group '{_fileSystem.Name}' exists ***");

                /*
                 * when file-system group type not smb do not use args only needed for smb...
                 */
                if (_fileSystem.FileSystemTypeId != (int)FileSystemType_E.SMB
                    && (_smbAuthType != Int32.MinValue || _ambassador != null))
                    throw new ConsoleHelpAsException($"  *** Invalid options for '{(FileSystemType_E)_fileSystem.FileSystemTypeId}' file-system group type ***");

                if (_smbAuthType != Int32.MinValue)
                    _fileSystemLogin.SmbAuthTypeId = _smbAuthType;

                if (_ambassador != null)
                    _fileSystemLogin.AmbassadorId = _ambassador.Id;

                if (_chrootPath != null)
                    _fileSystemLogin.ChrootPath = _chrootPath;

                if (_isReadOnly.HasValue)
                    _fileSystemLogin.IsReadOnly = _isReadOnly.Value;

                _fileSystemLogin = _uow.FileSystemLogins.Update(_fileSystemLogin);
                _uow.Commit();

                _fileSystemLogin = _uow.FileSystemLogins.Get(QueryExpressionFactory.GetQueryExpression<FileSystemLogin_EF>()
                    .Where(x => x.UserId == _user.UserId && x.FileSystemId == _fileSystem.Id).ToLambda(),
                        new List<Expression<Func<FileSystemLogin_EF, object>>>()
                        {
                            x => x.Ambassador,
                            x => x.FileSystem,
                            x => x.Login,
                            x => x.SmbAuthType,
                        })
                    .SingleOrDefault();

                FormatOutput.Write(_fileSystemLogin, true);

                return FormatOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return FormatOutput.AngryFarewell(ex);
            }
        }
    }
}
