using Bhbk.Daemon.Aurora.SSH.FileSystems;
using Bhbk.Daemon.Aurora.SSH.Helpers;
using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Microsoft.Extensions.DependencyInjection;
using Rebex;
using Rebex.IO.FileSystem;
using Serilog;
using System;
using System.Reflection;

namespace Bhbk.Daemon.Aurora.SSH.Factories
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

                fsSettings.LogWriter = new LogWriterHelper(logger, user, fsLogLevel);
            }

            if (!Enum.TryParse<FileSystemTypes>(user.FileSystem, true, out fsType))
                throw new InvalidCastException();

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            switch (fsType)
            {
                case FileSystemTypes.Composite:
                    {
                        Log.Information($"'{callPath}' '{user.UserName}' initialize '{typeof(CompositeReadWriteFileSystem).Name}'");

                        return new CompositeReadWriteFileSystem(fsSettings, factory, user);
                    }

                case FileSystemTypes.Memory:
                    {
                        Log.Information($"'{callPath}' '{user.UserName}' initialize '{typeof(MemoryFileSystemProvider).Name}'");

                        return new MemoryFileSystemProvider();
                    }

                case FileSystemTypes.SMB:
                    {
                        Log.Information($"'{callPath}' '{user.UserName}' initialize '{typeof(SmbReadWriteFileSystem).Name}'");

                        return new SmbReadWriteFileSystem(fsSettings, factory, user);
                    }

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
