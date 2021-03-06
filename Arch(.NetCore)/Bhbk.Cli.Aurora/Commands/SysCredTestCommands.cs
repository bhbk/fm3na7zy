﻿using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.CommandLine.IO;
using ManyConsole;
using System;
using System.Security.Principal;

namespace Bhbk.Cli.Aurora.Commands
{
    public class SysCredTestCommands : ConsoleCommand
    {
        private static string _credDomain;
        private static string _credLogin;
        private static string _credPass;

        public SysCredTestCommands()
        {
            IsCommand("sys-cred-test", "Test system credential");

            HasRequiredOption("d|domain=", "Enter credential domain", arg =>
            {
                _credDomain = arg;
            });

            HasRequiredOption("l|login=", "Enter credential login", arg =>
            {
                _credLogin = arg;
            });

            HasOption("p|pass=", "Enter credential password", arg =>
            {
                _credPass = arg;
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (string.IsNullOrEmpty(_credPass))
                {
                    Console.Out.Write("  *** Enter credential password to use *** : ");
                    _credPass = StandardInput.GetHiddenInput();

                    Console.Out.WriteLine();
                }

                /*
                 * Get the user token for the specified user, domain, and password using the unmanaged LogonUser method.
                 * The local machine name can be used for the domain name to impersonate a user on this machine.
                 */
                var safeAccessTokenHandle = UserHelper.GetSafeAccessTokenHandle(_credDomain, _credLogin, _credPass);

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
