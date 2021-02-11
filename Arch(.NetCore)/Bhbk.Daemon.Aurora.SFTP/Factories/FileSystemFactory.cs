using Bhbk.Daemon.Aurora.SFTP.Providers;
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
        internal static FileSystemProvider CreateFileSystem(IServiceScopeFactory factory, ILogger logger, FileSystemLogin_EF fileSystem,
            string identityUser, string identityPass)
        {
            var fsSettings = new FileSystemProviderSettings()
            {
                EnableGetContentMethodForDirectories = false,
                EnableGetLengthMethodForDirectories = false,
                EnableSaveContentMethodForDirectories = false,
                EnableStrictChecks = false,
            };

            var fsDebugLevel = (LogLevel)fileSystem.Login.DebugTypeId;

            switch (fsDebugLevel)
            {
                case LogLevel.Verbose:
                case LogLevel.Debug:
                case LogLevel.Info:
                case LogLevel.Error:
                    fsSettings.LogWriter = new LogHelper(logger, fileSystem, fsDebugLevel);
                    break;

                case LogLevel.Off:
                    fsSettings.LogWriter = null;
                    break;

                default:
                    throw new NotImplementedException();
            }

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            switch (fileSystem.FileSystem.FileSystemTypeId)
            {
                case (int)FileSystemType_E.Database:
                    {
                        if (fileSystem.IsReadOnly)
                        {
                            Log.Information($"'{callPath}' '{fileSystem.Login.UserName}' initialize '{typeof(DatabaseReadOnlyFileProvider).Name}'");

                            if (string.IsNullOrEmpty(fileSystem.ChrootPath))
                                return new DatabaseReadOnlyFileProvider(fsSettings, factory, fileSystem);
                            else
                            {
                                var chroot = new MountCapableFileSystemProvider();
                                chroot.Mount(fileSystem.ChrootPath, new DatabaseReadOnlyFileProvider(fsSettings, factory, fileSystem));

                                return chroot;
                            }
                        }
                        else
                        {
                            Log.Information($"'{callPath}' '{fileSystem.Login.UserName}' initialize '{typeof(DatabaseReadWriteFileProvider).Name}'");

                            if (string.IsNullOrEmpty(fileSystem.ChrootPath))
                                return new DatabaseReadWriteFileProvider(fsSettings, factory, fileSystem);
                            else
                            {
                                var chroot = new MountCapableFileSystemProvider();
                                chroot.Mount(fileSystem.ChrootPath, new DatabaseReadWriteFileProvider(fsSettings, factory, fileSystem));

                                return chroot;
                            }
                        }
                    }

                case (int)FileSystemType_E.Memory:
                    {
                        if (fileSystem.IsReadOnly)
                        {
                            Log.Information($"'{callPath}' '{fileSystem.Login.UserName}' initialize '{typeof(MemoryReadOnlyFileProvider).Name}'");

                            if (string.IsNullOrEmpty(fileSystem.ChrootPath))
                                return new MemoryReadOnlyFileProvider(fsSettings, factory, fileSystem);
                            else
                            {
                                var chroot = new MountCapableFileSystemProvider();
                                chroot.Mount(fileSystem.ChrootPath, new MemoryReadOnlyFileProvider(fsSettings, factory, fileSystem));

                                return chroot;
                            }
                        }
                        else
                        {
                            Log.Information($"'{callPath}' '{fileSystem.Login.UserName}' initialize '{typeof(MemoryReadWriteFileProvider).Name}'");

                            if (string.IsNullOrEmpty(fileSystem.ChrootPath))
                                return new MemoryReadWriteFileProvider(fsSettings, factory, fileSystem);
                            else
                            {
                                var chroot = new MountCapableFileSystemProvider();
                                chroot.Mount(fileSystem.ChrootPath, new MemoryReadWriteFileProvider(fsSettings, factory, fileSystem));

                                return chroot;
                            }
                        }
                    }

                case (int)FileSystemType_E.SMB:
                    {
                        if (fileSystem.IsReadOnly)
                        {
                            Log.Information($"'{callPath}' '{fileSystem.Login.UserName}' initialize '{typeof(SmbReadOnlyFileProvider).Name}'");

                            if (string.IsNullOrEmpty(fileSystem.ChrootPath))
                                return new SmbReadOnlyFileProvider(fsSettings, factory, fileSystem, identityUser, identityPass);
                            else
                            {
                                var chroot = new MountCapableFileSystemProvider();
                                chroot.Mount(fileSystem.ChrootPath, new SmbReadOnlyFileProvider(fsSettings, factory, fileSystem, identityUser, identityPass));

                                return chroot;
                            }
                        }
                        else
                        {
                            Log.Information($"'{callPath}' '{fileSystem.Login.UserName}' initialize '{typeof(SmbReadWriteFileProvider).Name}'");

                            if (string.IsNullOrEmpty(fileSystem.ChrootPath))
                                return new SmbReadWriteFileProvider(fsSettings, factory, fileSystem, identityUser, identityPass);
                            else
                            {
                                var chroot = new MountCapableFileSystemProvider();
                                chroot.Mount(fileSystem.ChrootPath, new SmbReadWriteFileProvider(fsSettings, factory, fileSystem, identityUser, identityPass));

                                return chroot;
                            }
                        }
                    }

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
