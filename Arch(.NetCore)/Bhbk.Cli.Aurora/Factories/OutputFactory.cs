using Bhbk.Lib.Aurora.Data_EF6.Models;
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
                Console.Out.WriteLine($"  [Credential GUID] {cred.Id}{(!cred.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(cred.IsEnabled ? " is enabled" : " is disabled")} [Created] {cred.CreatedUtc.LocalDateTime}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    [Login] domain:{cred.Domain} user:{cred.UserName}");

                Console.ResetColor();
            }
        }

        public static void StdOutCredentialSecrets(IEnumerable<Credential> creds)
        {
            foreach (var cred in creds)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [Credential GUID] {cred.Id}'{(!cred.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(cred.IsEnabled ? " is enabled" : " is disabled")} [Created] {cred.CreatedUtc.LocalDateTime}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    Encrypted password '{cred.EncryptedPassword}'");

                Console.ResetColor();
            }
        }

        public static void StdOutKeyPairs(IEnumerable<PublicKey> pubKeys, IEnumerable<PrivateKey> privKeys)
        {
            foreach (var pubKey in pubKeys)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [Public key GUID] {pubKey.Id} [Algo] {pubKey.KeyAlgo}{(!pubKey.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(pubKey.IsEnabled ? " is enabled" : " is disabled")} [Created] {pubKey.CreatedUtc.LocalDateTime}");
                Console.Out.WriteLine($"    [Sig] {pubKey.SigValue}");

                Console.ForegroundColor = ConsoleColor.White;
                if (pubKey.PrivateKeyId != null)
                {
                    var privKey = privKeys.Where(x => x.PublicKeyId == pubKey.Id)
                        .Single();

                    Console.Out.WriteLine($"    [Private key GUID] {privKey.Id} [Algo] {privKey.KeyAlgo}{(!privKey.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(privKey.IsEnabled ? " is enabled" : " is disabled")} [Created] {pubKey.CreatedUtc.LocalDateTime}");
                }
                else
                    Console.Out.WriteLine($"    [Private key] none");

                Console.ResetColor();
            };
        }

        public static void StdOutKeyPairSecrets(IEnumerable<PrivateKey> privKeys)
        {
            foreach (var key in privKeys)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [Private key GUID] {key.Id} [Algo] {key.KeyAlgo}{(!key.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(key.IsEnabled ? " is enabled" : " is disabled")} [Created] {key.CreatedUtc.LocalDateTime}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    Encrypted password '{key.KeyPass}'");

                Console.ResetColor();
            };
        }

        public static void StdOutNetworks(IEnumerable<Network> networks)
        {
            Console.Out.WriteLine();

            foreach (var net in networks.OrderBy(x => x.SequenceId))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Out.WriteLine($"  [Network GUID] {net.Id} [Seq] {net.SequenceId} [Action] {net.Action} [CIDR] {net.Address}" +
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
                Console.Out.WriteLine($"  [Config GUID] {config.Id}{(!config.IsDeletable ? " is not deletable" : null)}" +
                    $" [Created] {config.CreatedUtc.LocalDateTime}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    [Key] {config.ConfigKey} [Value] {config.ConfigValue}");

                Console.ResetColor();
            }
        }

        public static void StdOutSessions(IEnumerable<Session> sessions)
        {
            Console.Out.WriteLine();

            foreach (var session in sessions)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"  [Remote] {session.RemoteEndPoint}" +
                    $" [Action] {session.CallPath} [Detail] {session.Details}" +
                    $"{(string.IsNullOrEmpty(session.RemoteSoftwareIdentifier) ? null : " [Using] " + (session.RemoteSoftwareIdentifier) + "")}" +
                    $" [Created] {session.CreatedUtc.LocalDateTime}");

                Console.ResetColor();
            }
        }

        public static void StdOutUsers(IEnumerable<User> users)
        {
            foreach (var user in users)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [User GUID] {user.IdentityId} [Alias] {user.IdentityAlias}{(!user.IsDeletable ? " is not deletable and" : null)}" +
                    $"{(user.IsEnabled ? " is enabled" : " is disabled")} [Created] {user.CreatedUtc.LocalDateTime}");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Out.WriteLine($"    File system is {user.FileSystemType} and mounts as {(user.IsFileSystemReadOnly ? "read-only" : "read-write")}" +
                    $"{(string.IsNullOrEmpty(user.FileSystemChrootPath) ? null : " with chroot to " + (user.FileSystemChrootPath) + "")}");
                Console.Out.WriteLine($"    Password authentication is {(user.IsPasswordRequired ? "enabled" : "disabled")} ");
                Console.Out.WriteLine($"    Public key authentication is {(user.IsPublicKeyRequired ? "enabled" : "disabled")} ");
                Console.Out.WriteLine($"    Concurrent session maximum {user.ConcurrentSessions} is allowed");
                Console.Out.WriteLine($"    Quota maximum is {user.QuotaInBytes / 1024f} MB and {user.QuotaUsedInBytes / 1024f} MB is currently used");

                Console.ResetColor();
            }
        }

        public static void StdOutUserMounts(IEnumerable<UserMount> mounts)
        {
            foreach (var mount in mounts)
            {
                Console.Out.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Out.WriteLine($"  [Mount for] {mount.User.IdentityAlias} [Path] {mount.ServerAddress}{mount.ServerShare}" +
                    $" [Protocl] {mount.AuthType}");

                Console.ForegroundColor = ConsoleColor.White;
                if (mount.CredentialId.HasValue)
                    Console.Out.WriteLine($"    Mount credential [Domain] {mount.Credential.Domain} [User] {mount.Credential.UserName}");

                Console.ResetColor();
            }
        }
    }
}
