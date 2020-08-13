using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using System;
using System.Collections.Generic;

namespace Bhbk.Cli.Aurora.Helpers
{
    public class ConsoleHelper
    {
        public static void StdOutAmbassadors(IEnumerable<tbl_Ambassadors> creds)
        {
            foreach (var cred in creds)
            {
                Console.Out.WriteLine($"  Ambassador GUID '{cred.Id}'{(cred.Immutable ? " is immutable" : null)}");
                Console.Out.WriteLine($"    Login domain '{cred.Domain}' Login user '{cred.UserName}'");
                Console.Out.WriteLine();
            }
        }

        public static void StdOutKeyPairs(IEnumerable<tbl_PublicKeys> keys)
        {
            foreach (var key in keys)
            {
                Console.Out.WriteLine($"  Public '{key.KeyAlgo}' key with GUID '{key.Id}'{(key.Immutable ? " is immutable" : null)}. " +
                    $"Created {key.Created}.");
                Console.Out.WriteLine($"    Sig '{key.SigValue}'");
                Console.Out.WriteLine();

                if (key.PrivateKeyId != null)
                    Console.Out.WriteLine($"    Private '{key.KeyAlgo}' key GUID '{key.PrivateKeyId}'{(key.PrivateKey.Immutable ? " is immutable" : null)}. " +
                        $"Created {key.Created}.");
                else
                    Console.Out.WriteLine($"    Private key not available");

                Console.Out.WriteLine();
            };
        }

        public static void StdOutSettings(IEnumerable<tbl_Settings> configs)
        {
            foreach (var config in configs)
            {
                Console.Out.WriteLine($"  Config GUID '{config.Id}'{(config.Immutable ? " is immutable" : null)}");
                Console.Out.WriteLine($"    Config key '{config.ConfigKey}' Config value '{config.ConfigValue}'");
                Console.Out.WriteLine();
            }
        }

        public static void StdOutUserMounts(IEnumerable<tbl_UserMounts> mounts)
        {
            foreach(var mount in mounts)
            {
                Console.Out.WriteLine($"  Mount for user GUID '{mount.UserId}'{(mount.Immutable ? " is immutable" : null)}");
                Console.Out.WriteLine($"    Mount '{mount.ServerAddress}{mount.ServerShare}' using '{mount.AuthType}' protocol");
                Console.Out.WriteLine();
            }
        }
    }
}
