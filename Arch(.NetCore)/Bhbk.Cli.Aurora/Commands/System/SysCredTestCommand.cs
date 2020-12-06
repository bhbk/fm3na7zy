using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.CommandLine.IO;
using ManyConsole;
using System;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysCredTestCommand : ConsoleCommand
    {
        private string _credDomain, _credLogin, _credPass;

        public SysCredTestCommand()
        {
            IsCommand("sys-cred-test", "Test credential for system");

            HasRequiredOption("d|domain=", "Enter domain", arg =>
            {
                _credDomain = arg;
            });

            HasRequiredOption("l|login=", "Enter login", arg =>
            {
                _credLogin = arg;
            });

            HasOption("p|pass=", "Enter password", arg =>
            {
                _credPass = arg;
            });
        }

        [SupportedOSPlatform("windows")]
        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (string.IsNullOrEmpty(_credPass))
                {
                    Console.Out.WriteLine();
                    Console.Out.Write("  *** Enter credential password to use *** : ");
                    _credPass = StandardInput.GetHiddenInput();
                }

                /*
                 * Get the user token for the specified user, domain, and password using the unmanaged LogonUser method.
                 * The local machine name can be used for the domain name to impersonate a user on this machine.
                 */
                var safeAccessTokenHandle = UserHelper.GetSafeAccessTokenHandle(_credDomain, _credLogin, _credPass);

                Console.Out.WriteLine();
                Console.Out.WriteLine("Beginning user is " + WindowsIdentity.GetCurrent().Name);

                /*
                 * to run as unimpersonated, pass 'SafeAccessTokenHandle.InvalidHandle' instead of variable 'safeAccessTokenHandle'
                 */
                WindowsIdentity.RunImpersonated(safeAccessTokenHandle, () =>
                {
                    Console.Out.WriteLine();
                    Console.Out.WriteLine("Impersonated user is " + WindowsIdentity.GetCurrent().Name);
                });

                Console.Out.WriteLine();
                Console.Out.WriteLine("Ending user is " + WindowsIdentity.GetCurrent().Name);

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
