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

                Console.Out.WriteLine($"  Credential GUID '{cred.Id}'{(!cred.IsDeletable ? " is not deletable" : null)}'. " +
                    $"Created {cred.CreatedUtc.LocalDateTime}.");
                Console.Out.WriteLine($"    Login domain '{cred.Domain}' Login user '{cred.UserName}'");
            }
        }

        public static void StdOutKeyPairs(IEnumerable<PublicKey> pubKeys, IEnumerable<PrivateKey> privKeys)
        {
            foreach (var pubKey in pubKeys)
            {
                Console.Out.WriteLine();

                Console.Out.WriteLine($"  Public key GUID '{pubKey.Id}' of type '{pubKey.KeyAlgo}'{(!pubKey.IsDeletable ? " is not deletable" : null)}. " +
                    $"Created {pubKey.CreatedUtc.LocalDateTime}.");
                Console.Out.WriteLine($"    Sig '{pubKey.SigValue}'");

                if (pubKey.PrivateKeyId != null)
                {
                    var privKey = privKeys.Where(x => x.Id == pubKey.PrivateKeyId).Single();

                    Console.Out.WriteLine($"    Private key GUID '{privKey.Id}' of type '{privKey.KeyAlgo}'{(!privKey.IsDeletable ? " is not deletable" : null)}. " +
                        $"Created {pubKey.CreatedUtc.LocalDateTime}.");
                }
                else
                    Console.Out.WriteLine($"    Private key not available");
            };
        }

        public static void StdOutNetworks(IEnumerable<Network> nets)
        {
            Console.Out.WriteLine();

            foreach (var net in nets.OrderBy(x => x.SequenceId))
            {
                Console.Out.WriteLine($"  Network GUID '{net.Id}' sequence '{net.SequenceId}' is '{net.Action}' for '{net.Address}'.");
            }
        }

        public static void StdOutSecretsCredentials(IEnumerable<Credential> creds)
        {
            foreach (var cred in creds)
            {
                Console.Out.WriteLine();

                Console.Out.WriteLine($"  Credential GUID '{cred.Id}'{(!cred.IsDeletable ? " is not deletable" : null)}'. " +
                    $"Created {cred.CreatedUtc.LocalDateTime}.");
                Console.Out.WriteLine($"    Pass ciphertext '{cred.EncryptedPassword}'");
            }
        }

        public static void StdOutSecretsKeypairs(IEnumerable<PrivateKey> keys)
        {
            foreach (var key in keys)
            {
                Console.Out.WriteLine();

                Console.Out.WriteLine($"  Private key GUID '{key.Id}' of type '{key.KeyAlgo}'{(!key.IsDeletable ? " is not deletable" : null)}. " +
                    $"Created {key.CreatedUtc}.");
                Console.Out.WriteLine($"    Key pass ciphertext '{key.KeyPass}'");
            };
        }

        public static void StdOutSettings(IEnumerable<Setting> configs)
        {
            foreach (var config in configs)
            {
                Console.Out.WriteLine();

                Console.Out.WriteLine($"  Config GUID '{config.Id}'{(!config.IsDeletable ? " is not deletable" : null)}. " +
                    $"Created {config.CreatedUtc.LocalDateTime}.");
                Console.Out.WriteLine($"    Config key '{config.ConfigKey}' Config value '{config.ConfigValue}'");
            }
        }

        public static void StdOutUsers(IEnumerable<User> users)
        {
            foreach (var user in users)
            {
                Console.Out.WriteLine();

                Console.Out.WriteLine($"  User GUID '{user.IdentityId}' with alias '{user.IdentityAlias}'. Created {user.CreatedUtc.LocalDateTime}.");
                Console.Out.WriteLine($"    Public key authentication is {(user.IsPublicKeyRequired ? "enabled" : "disabled")}");
                Console.Out.WriteLine($"    Password authentication is {(user.IsPasswordRequired ? "enabled" : "disabled")}");
                Console.Out.WriteLine($"    File system is '{user.FileSystemType}' and mounts as {(user.IsFileSystemReadOnly ? "read only" : "read write")}");
            }
        }

        public static void StdOutUserMounts(IEnumerable<UserMount> mounts)
        {
            foreach (var mount in mounts)
            {
                Console.Out.WriteLine();

                Console.Out.WriteLine($"  Mount for user '{mount.User.IdentityAlias}'{(!mount.IsDeletable ? " is not deletable" : null)}");
                Console.Out.WriteLine($"    Mount path '{mount.ServerAddress}{mount.ServerShare}' using '{mount.AuthType}' protocol");

                if (mount.CredentialId.HasValue)
                    Console.Out.WriteLine($"    Mount credential '{mount.Credential.Domain}\\{mount.Credential.UserName}'");
            }
        }
    }
}
