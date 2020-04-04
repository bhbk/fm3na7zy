using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using System;
using System.Collections.Generic;

namespace Bhbk.Cli.Aurora.Helpers
{
    public class ConsoleHelpers
    {
        public static void ConsolePrintUserKeyPairs(IEnumerable<tbl_UserPublicKeys> keys)
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
    }
}
