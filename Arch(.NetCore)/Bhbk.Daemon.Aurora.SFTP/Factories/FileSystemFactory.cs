using Bhbk.Daemon.Aurora.SFTP.FileSystems;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Microsoft.Extensions.DependencyInjection;
using Rebex;
using Rebex.IO.FileSystem;
using Serilog;
using System;

namespace Bhbk.Daemon.Aurora.SFTP.Factories
{
    internal static class FileSystemFactory
    {
        internal static FileSystemProvider CreateFileSystem(IServiceScopeFactory factory, ILogger logger, tbl_Users user, 
            string identityUser, string identityPass)
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

            switch (fsType)
            {
                case FileSystemTypes.Composite:
                    {
                        if (!user.FileSystemReadOnly)
                            return new CompositeReadWriteFileSystem(fsSettings, factory, user);
                        else
                            return new CompositeReadOnlyFileSystem(fsSettings, factory, user);
                    }

                case FileSystemTypes.Memory:
                    {
                        if (!user.FileSystemReadOnly)
                            return new MemoryReadWriteFileSystem(fsSettings, factory, user);
                        else
                            return new MemoryReadOnlyFileSystem(fsSettings, factory, user);
                    }

                case FileSystemTypes.SMB:
                    {
                        if (!user.FileSystemReadOnly)
                            return new SmbReadWriteFileSystem(fsSettings, factory, user, identityUser, identityPass);
                        else
                            return new SmbReadOnlyFileSystem(fsSettings, factory, user, identityUser, identityPass);
                    }

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
