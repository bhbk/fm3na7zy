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
        internal static FileSystemProvider CreateFileSystem(IServiceScopeFactory factory, ILogger logger, Login_EF user,
            string identityUser, string identityPass)
        {
            var fsSettings = new FileSystemProviderSettings()
            {
                EnableGetContentMethodForDirectories = false,
                EnableGetLengthMethodForDirectories = false,
                EnableSaveContentMethodForDirectories = false,
                EnableStrictChecks = false,
            };

            if (user.DebugTypeId != (int)LogLevel.Off)
                fsSettings.LogWriter = new LogHelper(logger, user, (LogLevel)user.DebugTypeId);

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            switch (user.FileSystemTypeId)
            {
                case (int)FileSystemType_E.Database:
                    {
                        if (user.IsFileSystemReadOnly)
                        {
                            Log.Information($"'{callPath}' '{user.UserName}' initialize '{typeof(DatabaseReadOnlyFileSystem).Name}'");

                            if (string.IsNullOrEmpty(user.FileSystemChrootPath))
                                return new DatabaseReadOnlyFileSystem(fsSettings, factory, user);
                            else
                            {
                                var chroot = new MountCapableFileSystemProvider();
                                chroot.Mount(user.FileSystemChrootPath, new DatabaseReadOnlyFileSystem(fsSettings, factory, user));

                                return chroot;
                            }
                        }
                        else
                        {
                            Log.Information($"'{callPath}' '{user.UserName}' initialize '{typeof(DatabaseReadWriteFileSystem).Name}'");

                            if (string.IsNullOrEmpty(user.FileSystemChrootPath))
                                return new DatabaseReadWriteFileSystem(fsSettings, factory, user);
                            else
                            {
                                var chroot = new MountCapableFileSystemProvider();
                                chroot.Mount(user.FileSystemChrootPath, new DatabaseReadWriteFileSystem(fsSettings, factory, user));

                                return chroot;
                            }
                        }
                    }

                case (int)FileSystemType_E.Memory:
                    {
                        if (user.IsFileSystemReadOnly)
                        {
                            Log.Information($"'{callPath}' '{user.UserName}' initialize '{typeof(MemoryReadOnlyFileSystem).Name}'");

                            if (string.IsNullOrEmpty(user.FileSystemChrootPath))
                                return new MemoryReadOnlyFileSystem(fsSettings, factory, user);
                            else
                            {
                                var chroot = new MountCapableFileSystemProvider();
                                chroot.Mount(user.FileSystemChrootPath, new MemoryReadOnlyFileSystem(fsSettings, factory, user));

                                return chroot;
                            }
                        }
                        else
                        {
                            Log.Information($"'{callPath}' '{user.UserName}' initialize '{typeof(MemoryReadWriteFileSystem).Name}'");

                            if (string.IsNullOrEmpty(user.FileSystemChrootPath))
                                return new MemoryReadWriteFileSystem(fsSettings, factory, user);
                            else
                            {
                                var chroot = new MountCapableFileSystemProvider();
                                chroot.Mount(user.FileSystemChrootPath, new MemoryReadWriteFileSystem(fsSettings, factory, user));

                                return chroot;
                            }
                        }
                    }

                case (int)FileSystemType_E.SMB:
                    {
                        if (user.IsFileSystemReadOnly)
                        {
                            Log.Information($"'{callPath}' '{user.UserName}' initialize '{typeof(SmbReadOnlyFileSystem).Name}'");

                            if (string.IsNullOrEmpty(user.FileSystemChrootPath))
                                return new SmbReadOnlyFileSystem(fsSettings, factory, user, identityUser, identityPass);
                            else
                            {
                                var chroot = new MountCapableFileSystemProvider();
                                chroot.Mount(user.FileSystemChrootPath, new SmbReadOnlyFileSystem(fsSettings, factory, user, identityUser, identityPass));

                                return chroot;
                            }
                        }
                        else
                        {
                            Log.Information($"'{callPath}' '{user.UserName}' initialize '{typeof(SmbReadWriteFileSystem).Name}'");

                            if (string.IsNullOrEmpty(user.FileSystemChrootPath))
                                return new SmbReadWriteFileSystem(fsSettings, factory, user, identityUser, identityPass);
                            else
                            {
                                var chroot = new MountCapableFileSystemProvider();
                                chroot.Mount(user.FileSystemChrootPath, new SmbReadWriteFileSystem(fsSettings, factory, user, identityUser, identityPass));

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
