using System;
using System.Collections.Generic;
using System.Text;
using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Common.Services;
using Bhbk.Lib.Cryptography.Hashing;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using Rebex.Net;
using Rebex.Security.Certificates;

namespace Bhbk.Cli.Aurora.Commands
{
    public class KeyCommands : ConsoleCommand
    {
        private static SshPrivateKeyFormat _sshPrivCmdType;
        private static SshPublicKeyFormat _sshPubCmdType;
        private static string _sshPrivKeyList = string.Join(", ", Enum.GetNames(typeof(SshPrivateKeyFormat)));
        private static string _sshPubKeyList = string.Join(", ", Enum.GetNames(typeof(SshPublicKeyFormat)));

        public KeyCommands()
        {
            IsCommand("admin", "Do things with aurora entities...");

            HasOption("c=|create", "Create an entity", arg =>
            {
                if (!Enum.TryParse<SshPrivateKeyFormat>(arg, out _sshPrivCmdType))
                    throw new ConsoleHelpAsException("Invalid entity type. Possible are " + _sshPrivKeyList);
            });

            HasOption("d=|delete", "Delete an entity", arg =>
            {
                if (!Enum.TryParse<SshPublicKeyFormat>(arg, out _sshPubCmdType))
                    throw new ConsoleHelpAsException("Invalid entity type. Possible are " + _sshPubKeyList);
            });
        }

        public override int Run(string[] remainingArguments)
        {
            throw new NotImplementedException();
        }
    }
}
