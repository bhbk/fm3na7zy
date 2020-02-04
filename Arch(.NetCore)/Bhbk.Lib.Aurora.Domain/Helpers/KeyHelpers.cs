using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Cryptography.Entropy;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Rebex.Net;
using Rebex.Security.Certificates;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace Bhbk.Lib.Aurora.Domain.Helpers
{
	public class KeyHelpers
    {
		public static List<SshPublicKey> GetPublicKeysForUsers(string path)
		{
			var list = new List<SshPublicKey>();
			string[] files;

			try
			{
				files = Directory.GetFiles(path);
			}
			catch (Exception)
			{
				Log.Error("Unable to access user public key directory '{0}'", path);
				return list;
			}

			for (int i = 0; i < files.Length; i++)
			{
				string file = files[i];

				try
				{
					SshPublicKey key;

					switch (Path.GetExtension(file).ToLowerInvariant())
					{
						case ".pub":
						case ".key":
							{
								key = new SshPublicKey(file);
								list.Add(key);

								Log.Information("User public key '{0}' loaded.\r\nFingerprint: {1}", file, key.Fingerprint.ToString(SignatureHashAlgorithm.MD5));
							}
							break;

						case ".der":
						case ".cer":
						case ".crt":
							{
								var certificate = Certificate.LoadDer(file);
								key = new SshPublicKey(certificate);
								list.Add(key);

								Log.Information("User public key '{0}' loaded.\r\nFingerprint: {1}", file, key.Fingerprint.ToString(SignatureHashAlgorithm.MD5));
							}
							break;

						case "":
							{
								if (Path.GetFileName(file) != "authorized_keys")
									goto default;

								var keys = SshPublicKey.LoadPublicKeys(file);

								if (keys.Length > 0)
								{
									list.AddRange(keys);

									Log.Information("User public keys '{0}' loaded.", file);

									foreach (var item in keys)
										Log.Information("Fingerprint: {0}", item.Fingerprint.ToString(SignatureHashAlgorithm.MD5));
								}
							}
							break;

						default:
							Log.Error("User public key '{0}' file extension is unknown.", file);
							Log.Error("Supported extensions for SSH public keys: '*.pub', '*.key'.");
							Log.Error("Supported extensions for X509 certificates: '*.der', '*.cer', '*.crt'.");
							Log.Error("Supported file name for ~/.ssh/authorized_keys file format: 'authorized_keys'.");

							continue;

					}
				}
				catch (Exception x)
				{
					Log.Error("User public key '{0}' could not be loaded: {1}", file, x.Message);
				}
			}

			return list;
		}

        public static void ExportSshPrivateKey(tbl_Users user, tbl_UserPrivateKeys key, SshPrivateKeyFormat keyFormat, string keyPass, FileInfo outputFile)
        {
            if (string.IsNullOrEmpty(keyPass))
                keyPass = AlphaNumeric.CreateString(32);

            using (var stream = new MemoryStream())
            {
                var privKey = new SshPrivateKey(Convert.FromBase64String(key.KeyValueBase64), key.KeyValuePass);
                privKey.Save(stream, keyPass, keyFormat);

                File.WriteAllBytes(outputFile.FullName, stream.ToArray());
            }
        }

        public static void ExportSshPublicKey(tbl_Users user, tbl_UserPublicKeys key, SshPublicKeyFormat keyFormat, FileInfo outputFile)
        {
            using (var stream = new MemoryStream())
            {
                var pubKey = new SshPublicKey(Convert.FromBase64String(key.KeyValueBase64));
                pubKey.SavePublicKey(stream, keyFormat);

                File.WriteAllBytes(outputFile.FullName, stream.ToArray());
            }
        }

		/*
		 * use format for openssh clients
		 * https://www.rebex.net/sftp.net/features/private-keys.aspx
		 */
		public static void ExportSshPublicKeyBase64(tbl_Users user, tbl_UserPublicKeys key, FileInfo outputFile)
		{
			var pubKey = new SshPublicKey(Convert.FromBase64String(key.KeyValueBase64));
			var content = new string($"ssh-rsa {Convert.ToBase64String(pubKey.GetPublicKey())} {user.UserName}@{key.Hostname}");

			File.WriteAllText(outputFile.FullName, content);
		}

		public static SshPrivateKey ImportSshPublicKey(IUnitOfWork uow, tbl_Users user, SignatureHashAlgorithm hashAlgo, string hostname, FileInfo inputFile)
		{
			var pubKey = new SshPrivateKey(inputFile.FullName);

			uow.UserPublicKeys.Create(
				new tbl_UserPublicKeys
				{
					Id = Guid.NewGuid(),
					UserId = user.Id,
					KeyValueBase64 = Convert.ToBase64String(pubKey.GetPublicKey(), Base64FormattingOptions.None),
					KeyValueAlgo = pubKey.KeyAlgorithm.ToString(),
					KeySig = pubKey.Fingerprint.ToString(hashAlgo, false),
					KeySigAlgo = hashAlgo.ToString(),
					Hostname = hostname,
					Enabled = true,
					Created = DateTime.Now
				});
			uow.Commit();

			return pubKey;
		}

		public static SshPrivateKey CreateSshKeyPair(IUnitOfWork uow, tbl_Users user, SshHostKeyAlgorithm keyAlgo, int keySize, string keyPass, SignatureHashAlgorithm hashAlgo, string hostname)
        {
            if (string.IsNullOrEmpty(keyPass))
                keyPass = AlphaNumeric.CreateString(32);

            var pubId = Guid.NewGuid();
            var privId = Guid.NewGuid();
            var keyPair = SshPrivateKey.Generate(keyAlgo, keySize);

            uow.UserPrivateKeys.Create(
                new tbl_UserPrivateKeys
                {
                    Id = privId,
                    PublicKeyId = pubId,
                    UserId = user.Id,
                    KeyValueBase64 = Convert.ToBase64String(keyPair.GetPrivateKey(), Base64FormattingOptions.None),
                    KeyValueAlgo = keyAlgo.ToString(),
                    KeyValuePass = keyPass,
                    Enabled = true,
                    Created = DateTime.Now
                });
            uow.UserPublicKeys.Create(
                new tbl_UserPublicKeys
                {
                    Id = pubId,
                    PrivateKeyId = privId,
                    UserId = user.Id,
                    KeyValueBase64 = Convert.ToBase64String(keyPair.GetPublicKey(), Base64FormattingOptions.None),
                    KeyValueAlgo = keyAlgo.ToString(),
					KeySig = keyPair.Fingerprint.ToString(hashAlgo, false),
					KeySigAlgo = hashAlgo.ToString(),
                    Hostname = hostname,
                    Enabled = true,
                    Created = DateTime.Now
                });
            uow.Commit();

            return keyPair;
        }
    }
}
