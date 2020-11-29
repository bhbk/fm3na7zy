using Bhbk.Daemon.Aurora.SFTP.FileSystems;
using Bhbk.Lib.Aurora.Data_EF6.Models;
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
    internal static class FileSystemFactory
    {
        internal static FileSystemProvider CreateFileSystem(IServiceScopeFactory factory, ILogger logger, User user,
            string identityUser, string identityPass)
        {
            LogLevel fsLoggerLevel;
            FileSystemProviderType fsType;

            var fsSettings = new FileSystemProviderSettings()
            {
                EnableGetContentMethodForDirectories = false,
                EnableGetLengthMethodForDirectories = false,
                EnableSaveContentMethodForDirectories = false,
                EnableStrictChecks = false,
            };

            if (!string.IsNullOrEmpty(user.Debugger))
            {
                if (!Enum.TryParse<LogLevel>(user.Debugger, true, out fsLoggerLevel))
                    throw new InvalidCastException();

                fsSettings.LogWriter = new LogHelper(logger, user, fsLoggerLevel);
            }

            if (!Enum.TryParse<FileSystemProviderType>(user.FileSystemType, true, out fsType))
                throw new InvalidCastException();

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            switch (fsType)
            {
                case FileSystemProviderType.Composite:
                    {
                        if (user.FileSystemReadOnly)
                        {
                            Log.Information($"'{callPath}' '{user.IdentityAlias}' initialize '{typeof(CompositeReadOnlyFileSystem).Name}'");

                            return new CompositeReadOnlyFileSystem(fsSettings, factory, user);
                        }
                        else
                        {
                            Log.Information($"'{callPath}' '{user.IdentityAlias}' initialize '{typeof(CompositeReadWriteFileSystem).Name}'");

                            return new CompositeReadWriteFileSystem(fsSettings, factory, user);
                        }
                    }

                case FileSystemProviderType.Memory:
                    {
                        if (user.FileSystemReadOnly)
                        {
                            Log.Information($"'{callPath}' '{user.IdentityAlias}' initialize '{typeof(MemoryReadOnlyFileSystem).Name}'");

                            return new MemoryReadOnlyFileSystem(fsSettings, factory, user);
                        }
                        else
                        {
                            Log.Information($"'{callPath}' '{user.IdentityAlias}' initialize '{typeof(MemoryReadWriteFileSystem).Name}'");

                            return new MemoryReadWriteFileSystem(fsSettings, factory, user);
                        }
                    }

                case FileSystemProviderType.SMB:
                    {
                        if (user.FileSystemReadOnly)
                        {
                            Log.Information($"'{callPath}' '{user.IdentityAlias}' initialize '{typeof(SmbReadOnlyFileSystem).Name}'");

                            return new SmbReadOnlyFileSystem(fsSettings, factory, user, identityUser, identityPass);
                        }
                        else
                        {
                            Log.Information($"'{callPath}' '{user.IdentityAlias}' initialize '{typeof(SmbReadWriteFileSystem).Name}'");

                            return new SmbReadWriteFileSystem(fsSettings, factory, user, identityUser, identityPass);
                        }
                    }

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
