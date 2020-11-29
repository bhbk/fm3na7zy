using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Cryptography.Encryption;
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

        public static SafeAccessTokenHandle GetSafeAccessTokenHandle(string domain, string user, string pass)
        {
            SafeAccessTokenHandle safeAccessTokenHandle;

            bool returnValue = LogonUser(user, domain, pass, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                out safeAccessTokenHandle);

            if (returnValue == false)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return safeAccessTokenHandle;
        }

        public static bool ValidatePubKey(ICollection<PublicKey> userKeys, SshPublicKey loginKey)
        {
            var loginStream = new MemoryStream();
            loginKey.SavePublicKey(loginStream, SshPublicKeyFormat.Pkcs8);

            var loginValue = Encoding.UTF8.GetString(loginStream.ToArray());

            foreach (var userKey in userKeys)
            {
                var pubKeyBytes = Encoding.UTF8.GetBytes(userKey.KeyValue);
                var pubKeyInfo = new PublicKeyInfo();
                pubKeyInfo.Load(new MemoryStream(pubKeyBytes));

                var pubStream = new MemoryStream();
                var pubKey = new SshPublicKey(pubKeyInfo);
                pubKey.SavePublicKey(pubStream, SshPublicKeyFormat.Pkcs8);

                var pubKeyValue = Encoding.UTF8.GetString(pubStream.ToArray());

                if (loginValue == pubKeyValue)
                    return true;
            }

            return false;
        }

        public static ICollection<Credential> ChangeCredentialSecrets(ICollection<Credential> creds, 
            string secretCurrent, string secretNew)
        {
            var userCreds = new List<Credential>();

            foreach (var cred in creds)
            {
                var decryptedPass = AES.DecryptString(cred.EncryptedPassword, secretCurrent);
                var encryptedPass = AES.EncryptString(decryptedPass, secretCurrent);

                if (cred.EncryptedPassword != encryptedPass)
                    throw new InvalidOperationException();

                cred.EncryptedPassword = AES.EncryptString(decryptedPass, secretNew);

                userCreds.Add(cred);
            }

            return userCreds;
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);
    }
}