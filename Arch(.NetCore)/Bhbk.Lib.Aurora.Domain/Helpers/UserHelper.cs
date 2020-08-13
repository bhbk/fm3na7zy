using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Microsoft.Win32.SafeHandles;
using Rebex.Net;
using Rebex.Security.Cryptography.Pkcs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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

        public static bool ValidatePubKey(ICollection<tbl_PublicKeys> userKeys, SshPublicKey loginKey)
        {
            var loginStream = new MemoryStream();
            loginKey.SavePublicKey(loginStream, SshPublicKeyFormat.Pkcs8);

            var login = Encoding.ASCII.GetString(loginStream.ToArray());

            foreach (var userKey in userKeys)
            {
                var pubBytes = Encoding.ASCII.GetBytes(userKey.KeyValue);
                var pubKeyInfo = new PublicKeyInfo();
                pubKeyInfo.Load(new MemoryStream(pubBytes));

                var pubStream = new MemoryStream();
                var pubKey = new SshPublicKey(pubKeyInfo);
                pubKey.SavePublicKey(pubStream, SshPublicKeyFormat.Pkcs8);

                var pubResult = Encoding.ASCII.GetString(pubStream.ToArray());

                if (login == pubResult)
                    return true;
            }

            return false;
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);
    }
}