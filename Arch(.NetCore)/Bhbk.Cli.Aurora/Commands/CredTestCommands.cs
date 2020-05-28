using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.CommandLine.IO;
using ManyConsole;
using System;
using System.Security.Principal;

namespace Bhbk.Cli.Aurora.Commands
{
    public class CredTestCommands : ConsoleCommand
    {
        public CredTestCommands()
        {
            IsCommand("test-credential", "Test system credential");

        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                /*
                 * Get the user token for the specified user, domain, and password using the unmanaged LogonUser method.
                 * The local machine name can be used for the domain name to impersonate a user on this machine.
                 */
                Console.Write("  *** Enter domain to use *** : ");
                string domainName = StandardInput.GetInput();
                Console.Out.WriteLine();

                Console.Write("  *** Enter user to use *** : ");
                string userName = StandardInput.GetInput();
                Console.Out.WriteLine();

                Console.Write("  *** Enter password to use *** : ");
                string password = StandardInput.GetHiddenInput();
                Console.Out.WriteLine();

                var safeAccessTokenHandle = UserHelper.GetSafeAccessTokenHandle(domainName, userName, password);

                Console.Out.WriteLine("Beginning user is " + WindowsIdentity.GetCurrent().Name);
                Console.Out.WriteLine();

                /*
                 * to run as unimpersonated, pass 'SafeAccessTokenHandle.InvalidHandle' instead of variable 'safeAccessTokenHandle'
                 */
                WindowsIdentity.RunImpersonated(safeAccessTokenHandle, () =>
                {
                    Console.Out.WriteLine("Impersonated user is " + WindowsIdentity.GetCurrent().Name);
                    Console.Out.WriteLine();
                });

                Console.Out.WriteLine("Ending user is " + WindowsIdentity.GetCurrent().Name);
                Console.Out.WriteLine();

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }
    }
}
