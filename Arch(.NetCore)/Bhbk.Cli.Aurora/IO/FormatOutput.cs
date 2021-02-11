using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.CommandLine.IO;
using ManyConsole;
using Rebex.Net;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;

namespace Bhbk.Cli.Aurora.IO
{
    public class FormatOutput : StandardOutput
    {
        public static void Write(Alert_EF alert, bool? details = null)
        {
            if (details.HasValue && details.Value)
            {
                Console.Out.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.Out.WriteLine($"   [display name] '{alert.ToDisplayName}' [guid] {alert.Id} {(alert.IsEnabled ? "is enabled" : "is disabled")}" +
                $"{(string.IsNullOrEmpty(alert.ToEmailAddress) ? null : " [email] '" + alert.ToEmailAddress + "'")}" +
                $"{(string.IsNullOrEmpty(alert.ToPhoneNumber) ? null : " [text] '" + alert.ToPhoneNumber + "'")}");

            Console.Out.WriteLine($"    [on delete] {(alert.OnDelete ? "enabled" : "disabled")}" +
                $" [on download] {(alert.OnDownload ? "enabled" : "disabled")}" +
                $" [on upload] {(alert.OnUpload ? "enabled" : "disabled")}");

            if (details.HasValue && details.Value)
            {
                Console.ForegroundColor = ConsoleColor.White;

                if (!string.IsNullOrEmpty(alert.Comment))
                    Console.Out.WriteLine($"    [comment] '" + alert.Comment + "'");

                Console.Out.WriteLine($"   [created] {alert.CreatedUtc.LocalDateTime}");
            }

            Console.ResetColor();
        }

        public static void Write(Ambassador_EF ambassador, bool? details = null)
        {
            if (details.HasValue && details.Value)
            {
                Console.Out.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.Out.WriteLine($"  [ambassador upn] {ambassador.UserPrincipalName} [guid] {ambassador.Id}{(!ambassador.IsDeletable ? " is not deletable and" : null)} " +
                $"{(ambassador.IsEnabled ? "is enabled" : "is disabled")}");

            if (details.HasValue && details.Value)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"   [created] {ambassador.CreatedUtc.LocalDateTime}");
            }

            Console.ResetColor();
        }

        public static void Write(FileSystem_EF fileSystem, bool? detail = null)
        {
            var fileSystemType = (FileSystemType_E)fileSystem.FileSystemTypeId;

            if (detail.HasValue && detail.Value)
            {
                Console.Out.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.Out.WriteLine($"  [name] '{fileSystem.Name}' " +
                $"[type] {(FileSystemType_E)fileSystem.FileSystemTypeId} " +
                $"[guid] {fileSystem.Id}{(!fileSystem.IsDeletable ? " is not deletable and" : null)} " +
                $"{(fileSystem.IsEnabled ? "is enabled" : "is disabled")} ");

            if (detail.HasValue && detail.Value)
            {
                Console.ForegroundColor = ConsoleColor.White;

                switch (fileSystemType)
                {
                    case FileSystemType_E.Database:
                        Console.Out.WriteLine($"   [quota maximum] {fileSystem.Usage.QuotaInBytes / 1048576f}MB " +
                            $"[quota used] {fileSystem.Usage.QuotaUsedInBytes / 1048576f}MB");
                        break;

                    case FileSystemType_E.Memory:
                        Console.Out.WriteLine($"   [quota maximum] 100MB and quota used is N/A... all deleted at session end");
                        break;

                    case FileSystemType_E.SMB:
                        Console.Out.WriteLine($"   [quota maximum] N/A... dependant on storage backing the mount");
                        break;

                    default:
                        var fileSystemTypes = string.Join(", ", Enum.GetNames(typeof(FileSystemType_E)));
                        throw new ConsoleHelpAsException($"  *** Invalid filesystem type, options are '{fileSystemTypes}' ***");
                }

                if (fileSystem.FileSystemTypeId == (int)FileSystemType_E.SMB)
                {
                    if (fileSystem.UncPath == null)
                    {
                        Console.Out.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Out.WriteLine($"  *** No UNC path defined for '{FileSystemType_E.SMB}' filesystem ***");
                    }
                    else
                        Console.Out.WriteLine($"   [unc path] '{fileSystem.UncPath}'");
                }

                if (!string.IsNullOrEmpty(fileSystem.Description))
                    Console.Out.WriteLine($"   [description] '" + fileSystem.Description + "'");

                Console.Out.WriteLine($"   [created] {fileSystem.CreatedUtc.LocalDateTime}");
            }

            Console.ResetColor();
        }

        public static void Write(FileSystemLogin_EF fileSystemLogin, bool? detail = null)
        {
            var fileSystemType = (FileSystemType_E)fileSystemLogin.FileSystem.FileSystemTypeId;

            if (detail.HasValue && detail.Value)
            {
                Console.Out.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.Out.WriteLine($"  [name] '{fileSystemLogin.FileSystem.Name}' " +
                $"[type] {(FileSystemType_E)fileSystemType} " +
                $"[access] {(fileSystemLogin.IsReadOnly ? "read-only" : "read-write")} " +
                $"[guid] {fileSystemLogin.FileSystemId} " +
                $"{(fileSystemLogin.FileSystem.IsEnabled ? "is enabled" : "is disabled")}");

            if (detail.HasValue && detail.Value)
            {
                Console.ForegroundColor = ConsoleColor.White;

                switch (fileSystemType)
                {
                    case FileSystemType_E.Database:
                        Console.Out.WriteLine($"   [quota maximum] {fileSystemLogin.FileSystem.Usage.QuotaInBytes / 1048576f}MB " +
                            $"[quota used] {fileSystemLogin.FileSystem.Usage.QuotaUsedInBytes / 1048576f}MB");
                        break;

                    case FileSystemType_E.Memory:
                        Console.Out.WriteLine($"   [quota maximum] 100MB and quota used is N/A... all deleted at session end");
                        break;

                    case FileSystemType_E.SMB:
                        Console.Out.WriteLine($"   [quota maximum] N/A... dependant on storage backing the mount");
                        break;

                    default:
                        var fileSystemTypes = string.Join(", ", Enum.GetNames(typeof(FileSystemType_E)));
                        throw new ConsoleHelpAsException($"  *** Invalid filesystem type, options are '{fileSystemTypes}' ***");
                }

                if (fileSystemLogin.FileSystem.FileSystemTypeId == (int)FileSystemType_E.SMB)
                {
                    if (fileSystemLogin.FileSystem.UncPath == null)
                    {
                        Console.Out.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Out.WriteLine($"  *** No UNC path defined for '{FileSystemType_E.SMB}' filesystem ***");
                        Console.Out.WriteLine();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Out.WriteLine($"   [unc path] '{fileSystemLogin.FileSystem.UncPath}' " +
                            $"[auth-type] {(fileSystemLogin.SmbAuthTypeId.HasValue ? $"{(AuthType)fileSystemLogin.SmbAuthTypeId}" : "none")}");
                    }

                    Console.ForegroundColor = ConsoleColor.White;
                    if (fileSystemLogin.AmbassadorId.HasValue)
                        Console.Out.WriteLine($"   [ambassador upn] {fileSystemLogin.Ambassador.UserPrincipalName} [guid] {fileSystemLogin.Ambassador.Id} ");
                    else
                        Console.Out.WriteLine($"   [ambassador upn] none");
                }

                if (fileSystemLogin.ChrootPath != null)
                    Console.Out.WriteLine($"   [chroot] '{fileSystemLogin.ChrootPath}'");

                if (!string.IsNullOrEmpty(fileSystemLogin.FileSystem.Description))
                    Console.Out.WriteLine($"   [description] '" + fileSystemLogin.FileSystem.Description + "'");

                Console.Out.WriteLine($"   [created] {fileSystemLogin.FileSystem.CreatedUtc.LocalDateTime}");
            }

            Console.ResetColor();
        }

        public static void Write(Login_EF user, bool? details = null)
        {
            if (details.HasValue && details.Value)
            {
                Console.Out.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.Out.WriteLine($"  [login] {user.UserName} [type] {(AuthType_E)user.AuthTypeId} " +
                $"[guid] {user.UserId}{(!user.IsDeletable ? " is not deletable and" : null)} " +
                $"{(user.IsEnabled ? "is enabled" : "is disabled")}");

            if (details.HasValue && details.Value)
            {
                Console.ForegroundColor = ConsoleColor.White;

                Console.Out.WriteLine($"   [password authentication] {(user.IsPasswordRequired ? "enabled" : "disabled")} " +
                    $"{Environment.NewLine}   [public key authentication] {(user.IsPublicKeyRequired ? "enabled" : "disabled")} " +
                    $"{Environment.NewLine}   [session maximum] {user.Usage.SessionMax} and {user.Usage.SessionsInUse} currently used");

                if (!string.IsNullOrEmpty(user.Comment))
                    Console.Out.WriteLine($"   [comment] '" + user.Comment + "'");

                Console.Out.WriteLine($"   [created] {user.CreatedUtc.LocalDateTime}");
            }

            Console.ResetColor();
        }

        public static void Write(PublicKey_EF pubKey, PrivateKey_EF privKey, bool? details = null)
        {
            if (details.HasValue && details.Value)
            {
                Console.Out.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.Out.WriteLine($"  [public key algo] {(SshHostKeyAlgorithm)pubKey.KeyAlgorithmId} " +
                $"[guid] {pubKey.Id}{(!pubKey.IsDeletable ? " is not deletable and" : null)} " +
                $"{(pubKey.IsEnabled ? "is enabled" : "is disabled")}");

            if (details.HasValue && details.Value)
            {
                Console.ForegroundColor = ConsoleColor.White;

                Console.Out.WriteLine($"   [sig] {pubKey.SigValue}");

                if (!string.IsNullOrEmpty(pubKey.Comment))
                    Console.Out.WriteLine($"   [comment] '" + pubKey.Comment + "'");

                Console.Out.WriteLine($"   [created] {pubKey.CreatedUtc.LocalDateTime}");
            }

            if (privKey != null)
            {
                if (details.HasValue && details.Value)
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                else
                    Console.ForegroundColor = ConsoleColor.White;

                Console.Out.WriteLine();
                Console.Out.WriteLine($"   [private key algo] {(SshHostKeyAlgorithm)privKey.KeyAlgorithmId} " +
                    $"[guid] {privKey.Id}{(!privKey.IsDeletable ? " is not deletable and" : null)} " +
                    $"{(privKey.IsEnabled ? "is enabled" : "is disabled")}");

                if (details.HasValue && details.Value)
                    Console.Out.WriteLine($"    [created] {privKey.CreatedUtc.LocalDateTime}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Out.WriteLine($"   [private key] none");
            }

            Console.ResetColor();
        }

        public static void Write(PrivateKey_EF privKey, bool? details = null)
        {
            if (details.HasValue && details.Value)
            {
                Console.Out.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.Out.WriteLine($"  [private key algo] {(SshHostKeyAlgorithm)privKey.KeyAlgorithmId} " +
                $"[guid] {privKey.Id}{(!privKey.IsDeletable ? " is not deletable and" : null)} " +
                $"{(privKey.IsEnabled ? "is enabled" : "is disabled")}");

            if (details.HasValue && details.Value)
            {
                Console.ForegroundColor = ConsoleColor.White;

                Console.Out.WriteLine($"   [encrypted passphrase] {privKey.EncryptedPass}");
                Console.Out.WriteLine($"   [created] {privKey.CreatedUtc.LocalDateTime}");
            }

            Console.ResetColor();
        }

        public static void Write(Network_EF network, bool? details = null)
        {
            if (details.HasValue && details.Value)
            {
                Console.Out.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.Out.WriteLine($"  [seq] {network.SequenceId} [action] {(NetworkActionType_E)network.ActionTypeId} " +
                $"[cidr] {network.Address} " +
                $"[guid] {network.Id} " +
                $"{(network.IsEnabled ? "is enabled" : "is disabled")}");

            if (details.HasValue && details.Value)
            {
                Console.ForegroundColor = ConsoleColor.White;

                if (!string.IsNullOrEmpty(network.Comment))
                    Console.Out.WriteLine($"   [comment] '" + network.Comment + "'");

                Console.Out.WriteLine($"   [created] {network.CreatedUtc.LocalDateTime}");
            }

            Console.ResetColor();
        }

        public static void Write(Session_EF session, bool? details = null)
        {
            if (session.IsActive)
                Console.Out.Write("  *");
            else
                Console.Out.Write("  ");

            if (details.HasValue && details.Value)
            {
                if (string.IsNullOrEmpty(session?.UserName))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Out.Write($"[user] unknown ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Out.Write($"[user] {session.UserName} ");
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Out.Write($"[remote] {session.RemoteEndPoint} [local] {session.LocalEndPoint} [action] {session.CallPath} [detail] {session.Details}" +
                $"{(string.IsNullOrEmpty(session.RemoteSoftwareIdentifier) ? null : " [software] " + session.RemoteSoftwareIdentifier + "")}" +
                $" [when] {session.CreatedUtc.LocalDateTime}" +
                $"{Environment.NewLine}");

            Console.ResetColor();
        }

        public static void Write(Setting_EF setting, bool? details = null)
        {
            if (details.HasValue && details.Value)
            {
                Console.Out.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.Out.WriteLine($"  [setting guid] {setting.Id}{(!setting.IsDeletable ? " is not deletable" : null)}");

            if (details.HasValue && details.Value)
            {
                Console.ForegroundColor = ConsoleColor.White;

                Console.Out.WriteLine($"   [key] {setting.ConfigKey} [value] {setting.ConfigValue}");
                Console.Out.WriteLine($"   [created] {setting.CreatedUtc.LocalDateTime}");
            }

            Console.ResetColor();
        }
    }
}
