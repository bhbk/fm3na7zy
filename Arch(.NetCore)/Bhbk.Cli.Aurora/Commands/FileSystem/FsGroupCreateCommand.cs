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
    public class FsGroupCreateCommand : ConsoleCommand
    {
        private readonly IConfiguration _conf;
        private readonly IUnitOfWork _uow;
        private FileSystemType_E _fileSystemType;
        private readonly string _fileSystemTypeList = string.Join(", ", Enum.GetNames(typeof(FileSystemType_E)));
        private string _fileSystemName, _description, _uncPath;

        public FsGroupCreateCommand()
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("clisettings.json", optional: false, reloadOnChange: true)
                .Build();

            var env = new ContextService(InstanceContext.DeployedOrLocal);
            _uow = new UnitOfWork(_conf["Databases:AuroraEntities_EF6"], env);

            IsCommand("fs-group-create", "Create file-system group on system");

            HasRequiredOption("f|file-system=", "Enter file-system that does not exist already", arg =>
            {
                if (string.IsNullOrEmpty(arg))
                    throw new ConsoleHelpAsException($"  *** No file-system group given ***");

                var fileSystem = _uow.FileSystems.Get(QueryExpressionFactory.GetQueryExpression<FileSystem_EF>()
                    .Where(x => x.Name == arg).ToLambda())
                    .SingleOrDefault();

                if (fileSystem != null)
                    throw new ConsoleHelpAsException($"  *** The file-system group '{arg}' already exists ***");

                _fileSystemName = arg;
            });

            HasRequiredOption("t|type=", "Enter type of filesystem", arg =>
            {
                if (!Enum.TryParse(arg, true, out _fileSystemType))
                    throw new ConsoleHelpAsException($"  *** Invalid filesystem type, options are '{_fileSystemTypeList}' ***");
            });

            HasOption("d|description=", "Enter description", arg =>
            {
                CheckRequiredArguments();

                if (!string.IsNullOrEmpty(arg))
                    _description = arg;
            });

            HasOption("p|path=", $"Enter full UNC path to share (only for '{FileSystemType_E.SMB}' file-system)", arg =>
            {
                CheckRequiredArguments();

                if (!FilePathHelper.IsValidUncPath(arg.ToLower()))
                    throw new ConsoleHelpAsException($"  *** Invalid UNC path '{arg}' ***");

                _uncPath = arg.ToLower();
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                /*
                 * when file-system type if smb additional args are needed...
                 */
                if (_fileSystemType == FileSystemType_E.SMB
                    && _uncPath == null)
                    throw new ConsoleHelpAsException($"  *** Missing options for '{(FileSystemType_E)_fileSystemType}' file-system ***");

                var fileSystem = new FileSystem_EF
                {
                    Name = _fileSystemName,
                    FileSystemTypeId = (int)_fileSystemType,
                    Description = _description,
                    IsEnabled = true,
                    IsDeletable = true,
                };

                if (_description != null)
                    fileSystem.Description = _description;

                if (_uncPath != null)
                    fileSystem.UncPath = _uncPath;

                fileSystem = _uow.FileSystems.Create(fileSystem);
                _uow.Commit();

                fileSystem = _uow.FileSystems.Get(QueryExpressionFactory.GetQueryExpression<FileSystem_EF>()
                    .Where(x => x.Id == fileSystem.Id).ToLambda(),
                        new List<Expression<Func<FileSystem_EF, object>>>()
                        {
                            x => x.Files,
                            x => x.Folders,
                            x => x.Logins,
                            x => x.Usage,
                        })
                    .SingleOrDefault();

                FormatOutput.Write(fileSystem, true);

                return FormatOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return FormatOutput.AngryFarewell(ex);
            }
        }
    }
}
