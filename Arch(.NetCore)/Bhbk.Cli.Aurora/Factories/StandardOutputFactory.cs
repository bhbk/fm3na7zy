using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Primitives.Enums;
using ManyConsole;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Cli.Aurora.Factories
{
    public class StandardOutputFactory
    {
        public static void Alerts(IEnumerable<E_Alert> alerts)
        {
            foreach (var alert in alerts.OrderBy(x => x.ToDisplayName))
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Out.WriteLine($"  [alert GUID] {alert.Id} {(alert.IsEnabled ? "is enabled" : "is disabled")} [display-name] '{alert.ToDisplayName}'" +
                    $"{(string.IsNullOrEmpty(alert.ToEmailAddress) ? null : " [email] " + alert.ToEmailAddress)}" +
                    $"{(string.IsNullOrEmpty(alert.ToPhoneNumber) ? null : " [text] " + alert.ToPhoneNumber)}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    [on-delete] {(alert.OnDelete ? "enabled" : "disabled")}" +
                    $" [on-download] {(alert.OnDownload ? "enabled" : "disabled")}" +
                    $" [on-upload] {(alert.OnUpload ? "enabled" : "disabled")}");

                Console.ResetColor();
            }
        }

        public static void Ambassadors(IEnumerable<E_Ambassador> creds)
        {
            foreach (var cred in creds)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [ambassador GUID] {cred.Id}{(!cred.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(cred.IsEnabled ? " is enabled" : " is disabled")} [created] {cred.CreatedUtc.LocalDateTime}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    [login] {cred.UserName}");

                Console.ResetColor();
            }
        }

        public static void AmbassadorSecrets(IEnumerable<E_Ambassador> creds)
        {
            foreach (var cred in creds)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [ambassador GUID] {cred.Id}'{(!cred.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(cred.IsEnabled ? " is enabled" : " is disabled")} [created] {cred.CreatedUtc.LocalDateTime}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    [encrypted password] '{cred.EncryptedPass}'");

                Console.ResetColor();
            }
        }

        public static void KeyPairs(IEnumerable<E_PublicKey> pubKeys, IEnumerable<E_PrivateKey> privKeys)
        {
            foreach (var pubKey in pubKeys)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [public key GUID] {pubKey.Id} [algo] {pubKey.KeyAlgo}{(!pubKey.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(pubKey.IsEnabled ? " is enabled" : " is disabled")} [created] {pubKey.CreatedUtc.LocalDateTime}");
                Console.Out.WriteLine($"    [Sig] {pubKey.SigValue}");

                Console.ForegroundColor = ConsoleColor.White;
                if (pubKey.PrivateKeyId != null)
                {
                    var privKey = privKeys.Where(x => x.PublicKeyId == pubKey.Id)
                        .Single();

                    Console.Out.WriteLine($"    [private key GUID] {privKey.Id} [algo] {privKey.KeyAlgo}{(!privKey.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(privKey.IsEnabled ? " is enabled" : " is disabled")} [created] {pubKey.CreatedUtc.LocalDateTime}");
                }
                else
                    Console.Out.WriteLine($"    [private key] none");

                Console.ResetColor();
            };
        }

        public static void KeyPairSecrets(IEnumerable<E_PrivateKey> privKeys)
        {
            foreach (var key in privKeys)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [private key GUID] {key.Id} [algo] {key.KeyAlgo}{(!key.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(key.IsEnabled ? " is enabled" : " is disabled")} [created] {key.CreatedUtc.LocalDateTime}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    [encrypted password] {key.EncryptedPass}");

                Console.ResetColor();
            };
        }

        public static void Logins(IEnumerable<E_Login> users, string extras = null)
        {
            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(extras))
                {
                    Console.Out.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                    Console.ForegroundColor = ConsoleColor.White;

                Console.Out.WriteLine($"  [user GUID] {user.UserId} [alias] {user.UserName}{(!user.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(user.IsEnabled ? " is enabled" : " is disabled")} [created] {user.CreatedUtc.LocalDateTime}");

                if (!string.IsNullOrEmpty(extras))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Out.WriteLine($"    file system type is '{user.FileSystemType}' and mounts as {(user.IsFileSystemReadOnly ? "read-only" : "read-write")}" +
                        $"{(string.IsNullOrEmpty(user.FileSystemChrootPath) ? null : " with chroot to " + (user.FileSystemChrootPath) + "")}" +
                        $"{Environment.NewLine}    password authentication is {(user.IsPasswordRequired ? "enabled" : "disabled")} " +
                        $"{Environment.NewLine}    public key authentication is {(user.IsPublicKeyRequired ? "enabled" : "disabled")} " +
                        $"{Environment.NewLine}    session maximum is {user.Usage.SessionMax} and {user.Usage.SessionsInUse} currently used");

                    FileSystemProviderType userFileSystem;
                    Enum.TryParse(user.FileSystemType, out userFileSystem);

                    switch (userFileSystem)
                    {
                        case FileSystemProviderType.Database:
                            Console.Out.WriteLine($"    quota maximum is {user.Usage.QuotaInBytes / 1048576f}MB and quota used {user.Usage.QuotaUsedInBytes / 1048576f}MB");
                            break;
                        case FileSystemProviderType.Memory:
                            Console.Out.WriteLine($"    quota maximum is 100MB and quota used is N/A... all deleted at session end");
                            break;
                        case FileSystemProviderType.SMB:
                            Console.Out.WriteLine($"    quota maximum is N/A... dependant on storage backing the mount");
                            break;
                        default:
                            var validFileSystemtypes = string.Join(", ", Enum.GetNames(typeof(FileSystemProviderType)));
                            throw new ConsoleHelpAsException($"  *** Invalid filesystem type. Options are '{validFileSystemtypes}' ***");
                    }
                }

                Console.ResetColor();
            }
        }

        public static void LoginSecrets(IEnumerable<E_Login> logins)
        {
            foreach (var login in logins)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [login GUID] {login.UserId}'{(!login.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(login.IsEnabled ? " is enabled" : " is disabled")} [created] {login.CreatedUtc.LocalDateTime}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    [encrypted password] '{login.EncryptedPass}'");

                Console.ResetColor();
            }
        }

        public static void Mounts(IEnumerable<E_Mount> mounts)
        {
            foreach (var mount in mounts)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [mount for] {mount.Login.UserName} [path] {mount.ServerAddress}{mount.ServerShare}" +
                    $" [protocol] {mount.AuthType}");

                Console.ForegroundColor = ConsoleColor.White;
                if (mount.AmbassadorId.HasValue)
                    Console.Out.WriteLine($"    [ambassador GUID] {mount.Ambassador.Id} [username] {mount.Ambassador.UserName}");

                Console.ResetColor();
            }
        }

        public static void Networks(IEnumerable<E_Network> networks)
        {
            if (networks.Count() > 0)
                Console.Out.WriteLine();

            foreach (var net in networks.OrderBy(x => x.SequenceId))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Out.WriteLine($"  [network GUID] {net.Id} [seq] {net.SequenceId} [action] {net.Action} [cidr] {net.Address}" +
                    $"{(net.IsEnabled ? " is enabled" : " is disabled")}");

                Console.ResetColor();
            }
        }

        public static void Sessions(IEnumerable<E_Session> sessions, string extras = null)
        {
            foreach (var session in sessions)
            {
                if (session.IsActive)
                    Console.Out.Write("  *");
                else
                    Console.Out.Write("  ");

                if (!string.IsNullOrEmpty(extras))
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
        }

        public static void Settings(IEnumerable<E_Setting> settings)
        {
            foreach (var setting in settings)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [setting GUID] {setting.Id}{(!setting.IsDeletable ? " is not deletable" : null)}" +
                    $" [when] {setting.CreatedUtc.LocalDateTime}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    [key] {setting.ConfigKey} [value] {setting.ConfigValue}");

                Console.ResetColor();
            }
        }
    }
}
