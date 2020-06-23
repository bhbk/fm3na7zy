using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using System;
using System.Collections.Generic;

namespace Bhbk.Cli.Aurora.Helpers
{
    public class ConsoleHelper
    {
        public static void OutSysCredentials(IEnumerable<tbl_SysCredentials> creds)
        {
            foreach (var cred in creds)
            {
                Console.Out.WriteLine($"  Credential GUID '{cred.Id}'{(cred.Immutable ? " is immutable" : null)}");
                Console.Out.WriteLine($"    Login domain '{cred.Domain}' Login user '{cred.UserName}'");
                Console.Out.WriteLine();
            }
        }

        public static void OutUserPublicKeyPairs(IEnumerable<tbl_UserPublicKeys> keys)
        {
            foreach (var key in keys)
            {
                Console.Out.WriteLine($"  Public key GUID '{key.Id}'{(key.Immutable ? " is immutable" : null)}");
                Console.Out.WriteLine($"    {key.KeySigAlgo} fingerprint '{key.KeySig}'");

                if (key.PrivateKeyId != null)
                    Console.Out.WriteLine($"  Private key GUID '{key.PrivateKeyId}'{(key.PrivateKey.Immutable ? " is immutable" : null)}");
                else
                    Console.Out.WriteLine($"  Private key not available");

                Console.Out.WriteLine();
            };
        }

        public static void OutUserMounts(IEnumerable<tbl_UserMounts> mounts)
        {
            foreach(var mount in mounts)
            {
                Console.Out.WriteLine($"  Mount for user GUID '{mount.UserId}'{(mount.Immutable ? " is immutable" : null)}");
                Console.Out.WriteLine($"    Mount '{mount.ServerName}{mount.ServerPath}' using '{mount.AuthType}' protocol");
                Console.Out.WriteLine();

            }
        }
    }
}
