using Bhbk.Daemon.Aurora.SSH.Providers;
using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Microsoft.Extensions.DependencyInjection;
using Rebex;
using Rebex.IO.FileSystem;
using System;

namespace Bhbk.Daemon.Aurora.SSH.Factories
{
    public static class FileSystemFactory
    {
        public static FileSystemProvider CreateUserFileSystem(IServiceScopeFactory factory, tbl_Users user)
        {
            LogLevel fsLogger;
            FileSystemTypes fsType;

            var fsSettings = new FileSystemProviderSettings()
            {
                EnableGetContentMethodForDirectories = false,
                EnableGetLengthMethodForDirectories = false,
                EnableSaveContentMethodForDirectories = false,
                EnableStrictChecks = false,
            };

            if (!string.IsNullOrEmpty(user.Debugger))
            {
                if (!Enum.TryParse<LogLevel>(user.Debugger, true, out fsLogger))
                    throw new InvalidCastException();

                var fileName = String.Format("appdebug-{0:yyyyMMdd}-{1}.log", DateTime.Now, user.UserName);

                fsSettings.LogWriter = new FileLogWriter(fileName, fsLogger);
            }

            if (!Enum.TryParse<FileSystemTypes>(user.FileSystem, true, out fsType))
                throw new InvalidCastException();

            switch (fsType)
            {
                case FileSystemTypes.Composite:
                    return new ReadWriteFileSystem(fsSettings, factory, user);

                case FileSystemTypes.Memory:
                    return new MemoryFileSystemProvider();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
