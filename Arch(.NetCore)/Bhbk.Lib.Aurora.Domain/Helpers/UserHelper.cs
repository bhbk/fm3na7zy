using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

/*
 * https://docs.microsoft.com/en-us/windows/win32/secauthn/logonuserexexw
 * https://stackoverflow.com/questions/60839845/net-core-windowsidentity-impersonation-does-not-seem-to-be-working
 * https://daoudisamir.com/impersonate-users-in-c/
 * https://github.com/dotnet/runtime/issues/29935
 */

namespace Bhbk.Lib.Aurora.Domain.Helpers
{
    public static class UserHelper
    {
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NETWORK = 3;

        public static SafeAccessTokenHandle GetSafeAccessTokenHandle(string domain, string user, string pass)
        {
            SafeAccessTokenHandle safeAccessTokenHandle;

            bool returnValue = LogonUser(user, domain, pass, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                out safeAccessTokenHandle);

            if (returnValue == false)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return safeAccessTokenHandle;
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);
    }
}