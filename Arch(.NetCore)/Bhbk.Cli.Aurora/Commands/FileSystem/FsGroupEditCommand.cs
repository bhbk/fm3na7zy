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
using System.Linq;
using System.Linq.Expressions;

namespace Bhbk.Cli.Aurora.Commands.FileSystem
{
    public class FsGroupEditCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private FileSystem_EF _fileSystem = null;
        private string _name, _uncPath;
        private bool? _isEnabled, _isDeletable;

        public FsGroupEditCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("fs-group-edit", "Edit file-system group on system");

            HasRequiredOption("f|file-system=", "Enter existing file-system group", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No file-system group given ***");

                _fileSystem = _uow.FileSystems.Get(QueryExpressionFactory.GetQueryExpression<FileSystem_EF>()
                    .Where(x => x.Name == arg).ToLambda())
                    .SingleOrDefault();

                if (_fileSystem == null)
                    throw new ConsoleHelpAsException($"  *** Invalid file-system group '{arg}' ***");
            });

            HasOption("n|name=", "Enter name", arg =>
            {
                CheckRequiredArguments();

                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No name given ***");

                _name = arg;
            });

            HasOption("d|description=", "Enter description", arg =>
            {
                CheckRequiredArguments();

                if (!string.IsNullOrEmpty(arg))
                    _fileSystem.Description = arg;
            });

            HasOption("q|quota-max=", "Enter quota maximum (in bytes)", arg =>
            {
                CheckRequiredArguments();

                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No quota maximum given ***");

                _fileSystem.Usage.QuotaInBytes = Int32.Parse(arg);
            });

            HasOption("p|path=", $"Enter full UNC path to share (only for '{FileSystemType_E.SMB}' file-system group type)", arg =>
            {
                CheckRequiredArguments();

                if (!FilePathHelper.IsValidUncPath(arg.ToLower()))
                    throw new ConsoleHelpAsException($"  *** Invalid UNC path '{arg}' ***");

                _uncPath = arg.ToLower();
            });

            HasOption("e|enabled=", "Is enabled?", arg =>
            {
                CheckRequiredArguments();

                _isEnabled = bool.Parse(arg);
            });

            HasOption("x|deletable=", "Is deletable?", arg =>
            {
                CheckRequiredArguments();

                _isDeletable = bool.Parse(arg);
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                /*
                 * when file-systme group type smb additional args needed...
                 */

                if (_fileSystem.FileSystemTypeId == (int)FileSystemType_E.SMB
                    && _fileSystem.UncPath == null
                    && _uncPath == null)
                    throw new ConsoleHelpAsException($"  *** Invalid options for '{(FileSystemType_E)_fileSystem.FileSystemTypeId}' file-system group type ***");

                /*
                 * when file-system group name already exists do not allow rename...
                 */

                if (_name != null)
                {
                    if (_uow.FileSystems.Get()
                        .Where(x => x.Name.ToLower() == _name.ToLower()).Any())
                        throw new ConsoleHelpAsException($"  *** The file-system group '{_fileSystem.Name}' already exists ***");

                    _fileSystem.Name = _name;
                }

                if (_uncPath != null)
                    _fileSystem.UncPath = _uncPath;

                if (_isEnabled.HasValue)
                    _fileSystem.IsEnabled = _isEnabled.Value;

                if (_isDeletable.HasValue)
                    _fileSystem.IsDeletable = _isDeletable.Value;

                _fileSystem = _uow.FileSystems.Update(_fileSystem);
                _uow.Commit();

                _fileSystem = _uow.FileSystems.Get(QueryExpressionFactory.GetQueryExpression<FileSystem_EF>()
                    .Where(x => x.Id == _fileSystem.Id).ToLambda(),
                        new List<Expression<Func<FileSystem_EF, object>>>()
                        {
                            x => x.Files,
                            x => x.Folders,
                            x => x.Logins,
                            x => x.Usage,
                        })
                    .SingleOrDefault();

                FormatOutput.Write(_fileSystem, true);

                return FormatOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return FormatOutput.AngryFarewell(ex);
            }
        }
    }
}
