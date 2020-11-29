using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Primitives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Cli.Aurora.Helpers
{
    public class ConsoleHelper
    {
        public static void StdOutCredentials(IEnumerable<Credential> creds)
        {
            foreach (var cred in creds)
            {
                Console.Out.WriteLine($"  Credential GUID '{cred.Id}'{(!cred.IsDeletable ? " is immutable" : null)}");
                Console.Out.WriteLine($"    Login domain '{cred.Domain}' Login user '{cred.UserName}'");
            }
        }

        public static void StdOutCredentialSecrets(IEnumerable<Credential> creds)
        {
            foreach (var cred in creds)
            {
                Console.Out.WriteLine($"  Credential GUID '{cred.Id}'{(!cred.IsDeletable ? " is immutable" : null)}");
                Console.Out.WriteLine($"    Pass ciphertext '{cred.Password}'");
            }
        }

        public static void StdOutKeyPairs(IEnumerable<PublicKey> keys)
        {
            foreach (var key in keys)
            {
                Console.Out.WriteLine($"  Public '{key.KeyAlgo}' key with GUID '{key.Id}'{(!key.IsDeletable ? " is immutable" : null)}. " +
                    $"Created {key.CreatedUtc}.");
                Console.Out.WriteLine($"    Sig '{key.SigValue}'");

                if (key.PrivateKeyId != null)
                    Console.Out.WriteLine($"    Private '{key.KeyAlgo}' key GUID '{key.PrivateKeyId}'{(!key.PrivateKey.IsDeletable ? " is immutable" : null)}. " +
                        $"Created {key.CreatedUtc}.");
                else
                    Console.Out.WriteLine($"    Private key not available");
            };
        }

        public static void StdOutKeyPairSecrets(IEnumerable<PrivateKey> keys)
        {
            foreach (var key in keys)
            {
                Console.Out.WriteLine($"  Private '{key.KeyAlgo}' key with GUID '{key.Id}'{(!key.IsDeletable ? " is immutable" : null)}. " +
                    $"Created {key.CreatedUtc}.");
                Console.Out.WriteLine($"    Key pass ciphertext '{key.KeyPass}'");
            };
        }

        public static void StdOutNetworks(IEnumerable<Network> nets)
        {
            foreach (var net in nets.Where(x => x.Action == NetworkAction.Deny.ToString()).OrderBy(x => x.Address))
                Console.Out.WriteLine($"  Deny {net.Address} with GUID '{net.Id}'");

            foreach (var net in nets.Where(x => x.Action == NetworkAction.Allow.ToString()).OrderBy(x => x.Address))
                Console.Out.WriteLine($"  Allow {net.Address} with GUID '{net.Id}'");
        }

        public static void StdOutSettings(IEnumerable<Setting> configs)
        {
            foreach (var config in configs)
            {
                Console.Out.WriteLine($"  Config GUID '{config.Id}'{(!config.IsDeletable ? " is immutable" : null)}");
                Console.Out.WriteLine($"    Config key '{config.ConfigKey}' Config value '{config.ConfigValue}'");
            }
        }

        public static void StdOutUserMounts(IEnumerable<UserMount> mounts)
        {
            foreach (var mount in mounts)
            {
                Console.Out.WriteLine($"  Mount for user GUID '{mount.IdentityId}'{(!mount.IsDeletable ? " is immutable" : null)}");
                Console.Out.WriteLine($"    Mount '{mount.ServerAddress}{mount.ServerShare}' using '{mount.AuthType}' protocol");
            }
        }
    }
}
