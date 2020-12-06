using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Primitives.Enums;
using ManyConsole;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Cli.Aurora.Factories
{
    public class OutputFactory
    {
        public static void StdOutCredentials(IEnumerable<Credential> creds)
        {
            foreach (var cred in creds)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [credential GUID] {cred.Id}{(!cred.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(cred.IsEnabled ? " is enabled" : " is disabled")} [created] {cred.CreatedUtc.LocalDateTime}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    login [domain] {cred.Domain} [user] {cred.UserName}");

                Console.ResetColor();
            }
        }

        public static void StdOutCredentialSecrets(IEnumerable<Credential> creds)
        {
            foreach (var cred in creds)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [credential GUID] {cred.Id}'{(!cred.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(cred.IsEnabled ? " is enabled" : " is disabled")} [created] {cred.CreatedUtc.LocalDateTime}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    [encrypted password] '{cred.EncryptedPassword}'");

                Console.ResetColor();
            }
        }

        public static void StdOutKeyPairs(IEnumerable<PublicKey> pubKeys, IEnumerable<PrivateKey> privKeys)
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

        public static void StdOutKeyPairSecrets(IEnumerable<PrivateKey> privKeys)
        {
            foreach (var key in privKeys)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [private key GUID] {key.Id} [algo] {key.KeyAlgo}{(!key.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(key.IsEnabled ? " is enabled" : " is disabled")} [created] {key.CreatedUtc.LocalDateTime}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    [encrypted password] {key.KeyPass}");

                Console.ResetColor();
            };
        }

        public static void StdOutNetworks(IEnumerable<Network> networks)
        {
            Console.Out.WriteLine();

            foreach (var net in networks.OrderBy(x => x.SequenceId))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Out.WriteLine($"  [network GUID] {net.Id} [seq] {net.SequenceId} [action] {net.Action} [cidr] {net.Address}" +
                    $"{(net.IsEnabled ? " is enabled" : " is disabled")}");

                Console.ResetColor();
            }
        }

        public static void StdOutSettings(IEnumerable<Setting> configs)
        {
            foreach (var config in configs)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [config GUID] {config.Id}{(!config.IsDeletable ? " is not deletable" : null)}" +
                    $" [created] {config.CreatedUtc.LocalDateTime}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    [key] {config.ConfigKey} [value] {config.ConfigValue}");

                Console.ResetColor();
            }
        }

        public static void StdOutSessions(IEnumerable<Session> sessions)
        {
            Console.Out.WriteLine();

            foreach (var session in sessions)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"  [local] {session.LocalEndPoint} [remote] {session.RemoteEndPoint}" +
                    $" [action] {session.CallPath} [detail] {session.Details}" +
                    $"{(string.IsNullOrEmpty(session.RemoteSoftwareIdentifier) ? null : " [software] " + (session.RemoteSoftwareIdentifier) + "")}" +
                    $" [created] {session.CreatedUtc.LocalDateTime}");

                Console.ResetColor();
            }
        }

        public static void StdOutUsers(IEnumerable<User> users)
        {
            foreach (var user in users)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [user GUID] {user.IdentityId} [alias] {user.IdentityAlias}{(!user.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(user.IsEnabled ? " is enabled" : " is disabled")} [created] {user.CreatedUtc.LocalDateTime}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    file system type is '{user.FileSystemType}' and mounts as {(user.IsFileSystemReadOnly ? "read-only" : "read-write")}" +
                    $"{(string.IsNullOrEmpty(user.FileSystemChrootPath) ? null : " with chroot to " + (user.FileSystemChrootPath) + "")}" +
                    $"{Environment.NewLine}    password authentication is {(user.IsPasswordRequired ? "enabled" : "disabled")} " +
                    $"{Environment.NewLine}    public key authentication is {(user.IsPublicKeyRequired ? "enabled" : "disabled")} " +
                    $"{Environment.NewLine}    session maximum is {user.SessionMax} and {user.SessionsInUse} currently used");

                FileSystemProviderType userFileSystem;
                Enum.TryParse(user.FileSystemType, out userFileSystem);

                switch (userFileSystem)
                {
                    case FileSystemProviderType.Database:
                        Console.Out.WriteLine($"    quota maximum is {user.QuotaInBytes / 1048576f}MB and quota used {user.QuotaUsedInBytes / 1048576f}MB");
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

                Console.ResetColor();
            }
        }

        public static void StdOutUserAlerts(IEnumerable<UserAlert> alerts)
        {
            foreach (var alert in alerts.OrderBy(x => x.ToDisplayName))
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Out.WriteLine($"  [alert GUID] {alert.Id} [display-name] '{alert.ToDisplayName}'" +
                    $" [email] {(string.IsNullOrEmpty(alert.ToEmailAddress) ? " none " : alert.ToEmailAddress)}" +
                    $" [text] {(string.IsNullOrEmpty(alert.ToPhoneNumber) ? " none " : alert.ToPhoneNumber)}" +
                    $"{(alert.IsEnabled ? " is enabled" : " is disabled")}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    [on-delete] {(alert.OnDelete ? "enabled" : "disabled")}" +
                    $" [on-download] {(alert.OnDownload ? "enabled" : "disabled")}" +
                    $" [on-upload] {(alert.OnUpload ? "enabled" : "disabled")}");

                Console.ResetColor();
            }
        }

        public static void StdOutUserMounts(IEnumerable<UserMount> mounts)
        {
            foreach (var mount in mounts)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [mount for] {mount.User.IdentityAlias} [path] {mount.ServerAddress}{mount.ServerShare}" +
                    $" [protocol] {mount.AuthType}");

                Console.ForegroundColor = ConsoleColor.White;
                if (mount.CredentialId.HasValue)
                    Console.Out.WriteLine($"    mount credential [domain] {mount.Credential.Domain} [user] {mount.Credential.UserName}");

                Console.ResetColor();
            }
        }
    }
}
