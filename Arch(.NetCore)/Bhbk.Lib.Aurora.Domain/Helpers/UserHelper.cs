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
using System.Security.Cryptography;

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

        public static ICollection<Ambassador_EF> ChangeAmbassadorSecrets(ICollection<Ambassador_EF> creds,
            string secretCurrent, string secretNew)
        {
            var ambassadorCreds = new List<Ambassador_EF>();

            foreach (var cred in creds)
            {
                var decryptedPass = AES.DecryptString(cred.EncryptedPass, secretCurrent);
                var encryptedPass = AES.EncryptString(decryptedPass, secretCurrent);

                if (cred.EncryptedPass != encryptedPass)
                    throw new CryptographicException();

                cred.EncryptedPass = AES.EncryptString(decryptedPass, secretNew);

                ambassadorCreds.Add(cred);
            }

            return ambassadorCreds;
        }

        public static ICollection<Login_EF> ChangeLoginSecrets(ICollection<Login_EF> creds,
            string secretCurrent, string secretNew)
        {
            var loginCreds = new List<Login_EF>();

            foreach (var cred in creds)
            {
                var decryptedPass = AES.DecryptString(cred.EncryptedPass, secretCurrent);
                var encryptedPass = AES.EncryptString(decryptedPass, secretCurrent);

                if (cred.EncryptedPass != encryptedPass)
                    throw new CryptographicException();

                cred.EncryptedPass = AES.EncryptString(decryptedPass, secretNew);

                loginCreds.Add(cred);
            }

            return loginCreds;
        }

        public static SafeAccessTokenHandle GetSafeAccessTokenHandle(string domain, string user, string pass)
        {
            SafeAccessTokenHandle safeAccessTokenHandle;

            bool returnValue = LogonUser(user, domain, pass, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                out safeAccessTokenHandle);

            if (returnValue == false)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return safeAccessTokenHandle;
        }

        public static bool ValidatePubKey(ICollection<PublicKey_EF> userKeys, SshPublicKey loginKey)
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

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);
    }
}