using Bhbk.Daemon.Aurora.SFTP.FileSystems;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Microsoft.Extensions.DependencyInjection;
using Rebex;
using Rebex.IO.FileSystem;
using Serilog;
using System;
using System.Reflection;

namespace Bhbk.Daemon.Aurora.SFTP.Factories
{
    public static class FileSystemFactory
    {
        public static FileSystemProvider CreateFileSystem(IServiceScopeFactory factory, tbl_Users user, ILogger logger)
        {
            LogLevel fsLogLevel;
            FileSystemTypes fsType;

            var fsSettings = new FileSystemProviderSettings()
            {
                EnableGetContentMethodForDirectories = false,
                EnableGetLengthMethodForDirectories = false,
                EnableSaveContentMethodForDirectories = false,
                EnableStrictChecks = false,
            };

            if (!string.IsNullOrEmpty(user.DebugLevel))
            {
                if (!Enum.TryParse<LogLevel>(user.DebugLevel, true, out fsLogLevel))
                    throw new InvalidCastException();

                fsSettings.LogWriter = new LogHelper(logger, user, fsLogLevel);
            }

            if (!Enum.TryParse<FileSystemTypes>(user.FileSystemType, true, out fsType))
                throw new InvalidCastException();

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            switch (fsType)
            {
                case FileSystemTypes.Composite:
                    {
                        if (!user.FileSystemReadOnly)
                        {
                            Log.Information($"'{callPath}' '{user.UserName}' initialize '{typeof(CompositeReadWriteFileSystem).Name}'");

                            return new CompositeReadWriteFileSystem(fsSettings, factory, user);
                        }
                        else
                        {
                            Log.Information($"'{callPath}' '{user.UserName}' initialize '{typeof(CompositeReadOnlyFileSystem).Name}'");

                            return new CompositeReadOnlyFileSystem(fsSettings, factory, user);
                        }
                    }

                case FileSystemTypes.Memory:
                    {
                        if (!user.FileSystemReadOnly)
                        {
                            Log.Information($"'{callPath}' '{user.UserName}' initialize '{typeof(MemoryFileSystemProvider).Name}'");

                            return new MemoryFileSystemProvider();
                        }
                        else
                        {
                            Log.Information($"'{callPath}' '{user.UserName}' initialize '{typeof(MemoryFileSystemProvider).Name}'");

                            throw new NotImplementedException();
                        }
                    }

                case FileSystemTypes.SMB:
                    {
                        if (!user.FileSystemReadOnly)
                        {
                            Log.Information($"'{callPath}' '{user.UserName}' initialize '{typeof(SmbReadWriteFileSystem).Name}'");

                            return new SmbReadWriteFileSystem(fsSettings, factory, user);
                        }
                        else
                        {
                            Log.Information($"'{callPath}' '{user.UserName}' initialize '{typeof(SmbReadOnlyFileSystem).Name}'");

                            return new SmbReadOnlyFileSystem(fsSettings, factory, user);
                        }
                    }

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
