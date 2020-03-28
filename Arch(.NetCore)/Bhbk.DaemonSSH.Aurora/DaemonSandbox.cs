using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Rebex.Net;
using Rebex.Security.Certificates;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace Bhbk.DaemonSSH.Aurora
{
    public class DaemonSandbox
    {
        public static void Files(IConfiguration conf, IUnitOfWork uow, tbl_Users user)
        {
            try
            {
                //uow.UserPrivateKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_UserPrivateKeys>()
                //    .Where(x => x.UserId == user.Id).ToLambda());
                //uow.UserPublicKeys.Delete(QueryExpressionFactory.GetQueryExpression<tbl_UserPublicKeys>()
                //    .Where(x => x.UserId == user.Id).ToLambda());
                //uow.Commit();

                //KeyHelpers.GenerateSshPrivateKey(uow, user, SshHostKeyAlgorithm.RSA, 2048, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256, Dns.GetHostName());

                var userName = user.UserName;
                var path = PathHelpers.GetUserRoot(conf, user).FullName;
                var priv = user.tbl_UserPrivateKeys.OrderBy(x => x.Created).FirstOrDefault();
                var pub = user.tbl_UserPublicKeys.OrderBy(x => x.Created).FirstOrDefault();

                //sandbox private newopenssh key format
                var privNewOpenSsh = SshPrivateKeyFormat.NewOpenSsh;
                KeyHelpers.ExportSshPrivateKey(user, priv, privNewOpenSsh, priv.KeyValuePass,
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privNewOpenSsh.ToString().ToLower() + ".txt"));
                var privKey = KeyHelpers.ImportSshPrivateKey(uow, user, SignatureHashAlgorithm.SHA256, priv.KeyValuePass, Dns.GetHostName(),
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privNewOpenSsh.ToString().ToLower() + ".txt"));

                //sandbox private openssh key format
                var privOpenSsh = SshPrivateKeyFormat.OpenSsh;
                KeyHelpers.ExportSshPrivateKey(user, priv, privOpenSsh, priv.KeyValuePass,
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privOpenSsh.ToString().ToLower() + ".txt"));
                privKey = KeyHelpers.ImportSshPrivateKey(uow, user, SignatureHashAlgorithm.SHA256, priv.KeyValuePass, Dns.GetHostName(),
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privOpenSsh.ToString().ToLower() + ".txt"));

                //sandbox private pkcs8 key format
                var privPcks8 = SshPrivateKeyFormat.Pkcs8;
                KeyHelpers.ExportSshPrivateKey(user, priv, privPcks8, priv.KeyValuePass,
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privPcks8.ToString().ToLower() + ".txt"));
                privKey = KeyHelpers.ImportSshPrivateKey(uow, user, SignatureHashAlgorithm.SHA256, priv.KeyValuePass, Dns.GetHostName(),
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privPcks8.ToString().ToLower() + ".txt"));

                //sandbox private putty key format
                var privPutty = SshPrivateKeyFormat.Putty;
                KeyHelpers.ExportSshPrivateKey(user, priv, privPutty, priv.KeyValuePass,
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privPutty.ToString().ToLower() + ".txt"));
                privKey = KeyHelpers.ImportSshPrivateKey(uow, user, SignatureHashAlgorithm.SHA256, priv.KeyValuePass, Dns.GetHostName(),
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".key." + privPutty.ToString().ToLower() + ".txt"));

                //sandbox public opensshbase64 key format
                KeyHelpers.ExportSshPublicKeyBase64(user, pub,
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub.opensshbase64.txt"));
                var pubKey = KeyHelpers.ImportSshPublicKeyBase64(uow, user, SignatureHashAlgorithm.SHA256, Dns.GetHostName(),
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub.opensshbase64.txt"));

                //sandbox public pkcs8 key format
                var pubPkcs8 = SshPublicKeyFormat.Pkcs8;
                KeyHelpers.ExportSshPublicKey(user, pub, pubPkcs8,
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub." + pubPkcs8.ToString().ToLower() + ".txt"));
                pubKey = KeyHelpers.ImportSshPublicKey(uow, user, SignatureHashAlgorithm.SHA256, Dns.GetHostName(),
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub." + pubPkcs8.ToString().ToLower() + ".txt"));

                //sandbox public ssh2base64 key format
                var pubSsh2Base64 = SshPublicKeyFormat.Ssh2Base64;
                KeyHelpers.ExportSshPublicKey(user, pub, pubSsh2Base64,
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub." + pubSsh2Base64.ToString().ToLower() + ".txt"));
                pubKey = KeyHelpers.ImportSshPublicKey(uow, user, SignatureHashAlgorithm.SHA256, Dns.GetHostName(),
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub." + pubSsh2Base64.ToString().ToLower() + ".txt"));

                //sandbox public ssh2raw key format
                var pubSsh2Raw = SshPublicKeyFormat.Ssh2Raw;
                KeyHelpers.ExportSshPublicKey(user, pub, pubSsh2Raw,
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub." + pubSsh2Raw.ToString().ToLower()));
                pubKey = KeyHelpers.ImportSshPublicKey(uow, user, SignatureHashAlgorithm.SHA256, Dns.GetHostName(),
                    new FileInfo(path + Path.DirectorySeparatorChar + userName + ".pub." + pubSsh2Raw.ToString().ToLower()));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}
