using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Primitives.Enums;
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

                Console.Out.WriteLine($"  Credential GUID '{cred.Id}'{(!cred.IsDeletable ? " is immutable" : null)}");
                Console.Out.WriteLine($"    Login domain '{cred.Domain}' Login user '{cred.UserName}'");
            }
        }

        public static void StdOutCredentialSecrets(IEnumerable<Credential> creds)
        {
            foreach (var cred in creds)
            {
                Console.Out.WriteLine();

                Console.Out.WriteLine($"  Credential GUID '{cred.Id}'{(!cred.IsDeletable ? " is immutable" : null)}");
                Console.Out.WriteLine($"    Pass ciphertext '{cred.Password}'");
            }
        }

        public static void StdOutKeyPairs(IEnumerable<PublicKey> keys)
        {
            foreach (var key in keys)
            {
                Console.Out.WriteLine();

                Console.Out.WriteLine($"  Public key GUID '{key.Id}' of type '{key.KeyAlgo}'{(!key.IsDeletable ? " is not deletable" : null)}. " +
                    $"Created {key.CreatedUtc}.");
                Console.Out.WriteLine($"    Sig '{key.SigValue}'");

                if (key.PrivateKeyId != null)
                    Console.Out.WriteLine($"    Private key GUID '{key.PrivateKeyId}' of type '{key.KeyAlgo}'{(!key.PrivateKey.IsDeletable ? " is not deletable" : null)}. " +
                        $"Created {key.CreatedUtc}.");
                else
                    Console.Out.WriteLine($"    Private key not available");
            };
        }

        public static void StdOutKeyPairSecrets(IEnumerable<PrivateKey> keys)
        {
            foreach (var key in keys)
            {
                Console.Out.WriteLine();

                Console.Out.WriteLine($"  Private key GUID '{key.Id}' of type '{key.KeyAlgo}'{(!key.IsDeletable ? " is immutable" : null)}. " +
                    $"Created {key.CreatedUtc}.");
                Console.Out.WriteLine($"    Key pass ciphertext '{key.KeyPass}'");
            };
        }

        public static void StdOutNetworks(IEnumerable<Network> nets)
        {
            Console.Out.WriteLine();

            foreach (var net in nets.Where(x => x.Action == NetworkActionType.Deny.ToString()).OrderBy(x => x.Address))
            {
                Console.Out.WriteLine($"  Network GUID '{net.Id}' is {net.Action} for {net.Address}");
            }

            foreach (var net in nets.Where(x => x.Action == NetworkActionType.Allow.ToString()).OrderBy(x => x.Address))
            {
                Console.Out.WriteLine($"  Network GUID '{net.Id}' is {net.Action} for {net.Address}");
            }
        }

        public static void StdOutSettings(IEnumerable<Setting> configs)
        {
            foreach (var config in configs)
            {
                Console.Out.WriteLine();

                Console.Out.WriteLine($"  Config GUID '{config.Id}'{(!config.IsDeletable ? " is immutable" : null)}");
                Console.Out.WriteLine($"    Config key '{config.ConfigKey}' Config value '{config.ConfigValue}'");
            }
        }

        public static void StdOutUsers(IEnumerable<User> users)
        {
            foreach(var user in users)
            {
                Console.Out.WriteLine();

                Console.Out.WriteLine($"  User GUID '{user.IdentityId}' with alias '{user.IdentityAlias}'");
                Console.Out.WriteLine($"    File system is '{user.FileSystemType}' and mount as {(user.FileSystemReadOnly ? "read only" : "read write")}");
                Console.Out.WriteLine($"    Public key authentication is {(user.RequirePublicKey ? "enabled" : "disabled")}");
                Console.Out.WriteLine($"    Password authentication is {(user.RequirePassword ? "enabled" : "disabled")}");
            }
        }

        public static void StdOutUserMounts(IEnumerable<UserMount> mounts)
        {
            foreach (var mount in mounts)
            {
                Console.Out.WriteLine();

                Console.Out.WriteLine($"  Mount for user GUID '{mount.User.IdentityId}' with alias '{mount.User.IdentityAlias}'{(!mount.IsDeletable ? " is not deletable" : null)}");
                Console.Out.WriteLine($"    Mount path '{mount.ServerAddress}{mount.ServerShare}' using '{mount.AuthType}' protocol");

                if (mount.CredentialId.HasValue)
                    Console.Out.WriteLine($"    Mount credential '{mount.Credential.Domain}\\{mount.Credential.UserName}'");
            }
        }
    }
}
