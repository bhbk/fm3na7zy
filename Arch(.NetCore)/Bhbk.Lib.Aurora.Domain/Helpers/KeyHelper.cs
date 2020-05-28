using Bhbk.Lib.Aurora.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Rebex.Net;
using Rebex.Security.Certificates;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bhbk.Lib.Aurora.Domain.Helpers
{
	public class KeyHelper
	{
		public static SshPrivateKey GenerateSshPrivateKey(IUnitOfWork uow, tbl_Users user, SshHostKeyAlgorithm keyAlgo, int keySize, string keyPass, SignatureHashAlgorithm hashAlgo, string hostname)
		{
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

		public static SshPrivateKey ExportSshPrivateKey(tbl_Users user, tbl_UserPrivateKeys key, SshPrivateKeyFormat keyFormat, string keyPass, FileInfo outputFile)
		{
			using (var stream = new MemoryStream())
			{
				var privKey = new SshPrivateKey(Convert.FromBase64String(key.KeyValueBase64), key.KeyValuePass);
				privKey.Save(stream, keyPass, keyFormat);

				File.WriteAllBytes(outputFile.FullName, stream.ToArray());

				return privKey;
			}
		}

		public static SshPublicKey ExportSshPublicKey(tbl_Users user, tbl_UserPublicKeys key, SshPublicKeyFormat keyFormat, FileInfo outputFile)
		{
			using (var stream = new MemoryStream())
			{
				var pubKey = new SshPublicKey(Convert.FromBase64String(key.KeyValueBase64));
				pubKey.SavePublicKey(stream, keyFormat);

				File.WriteAllBytes(outputFile.FullName, stream.ToArray());

				return pubKey;
			}
		}

		/*
		 * openssh uses base64 and special formatting for public keys
		 * https://man.openbsd.org/ssh-keygen
		 */
		public static SshPublicKey ExportSshPublicKeyBase64(tbl_Users user, tbl_UserPublicKeys key, FileInfo outputFile)
		{
			var pubKey = new SshPublicKey(Convert.FromBase64String(key.KeyValueBase64));
			var data = new string($"ssh-rsa {Convert.ToBase64String(pubKey.GetPublicKey())} {user.UserName}@{key.Hostname}");

			File.WriteAllText(outputFile.FullName, data);

			return pubKey;
		}

		/*
		 * openssh uses base64 and special formatting for public keys
		 * https://man.openbsd.org/ssh-keygen
		 */
		public static ICollection<SshPublicKey> ExportSshPublicKeysBase64(tbl_Users user, ICollection<tbl_UserPublicKeys> keys, FileInfo outputFile)
		{
			var sb = new StringBuilder();
			var pubKeys = new List<SshPublicKey>();

			foreach (var key in keys)
			{
				var pubKey = new SshPublicKey(Convert.FromBase64String(key.KeyValueBase64));

				sb.AppendLine(new string($"ssh-rsa {Convert.ToBase64String(pubKey.GetPublicKey())} {user.UserName}@{key.Hostname}"));
				pubKeys.Add(pubKey);
			}

			File.WriteAllText(outputFile.FullName, sb.ToString());

			return pubKeys;
		}

		public static SshPrivateKey ImportSshPrivateKey(IUnitOfWork uow, tbl_Users user, SignatureHashAlgorithm hashAlgo, string keyPass, string hostname, FileInfo inputFile)
		{
			var key = new SshPrivateKey(inputFile.FullName, keyPass);

			var privId = Guid.NewGuid();
			var pubId = Guid.NewGuid();

			var privKeyBase64 = Convert.ToBase64String(key.GetPrivateKey(), Base64FormattingOptions.None);
			var pubKeyBase64 = Convert.ToBase64String(key.GetPublicKey(), Base64FormattingOptions.None);

			var privKeyExists = uow.UserPrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserPrivateKeys>()
				.Where(x => x.KeyValueBase64 == privKeyBase64).ToLambda()).SingleOrDefault();
			var pubKeyExists = uow.UserPublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserPublicKeys>()
				.Where(x => x.KeyValueBase64 == pubKeyBase64).ToLambda()).SingleOrDefault();

			if (privKeyExists == null && pubKeyExists == null)
			{
				uow.UserPrivateKeys.Create(
					new tbl_UserPrivateKeys
					{
						Id = privId,
						PublicKeyId = pubId,
						UserId = user.Id,
						KeyValueBase64 = privKeyBase64,
						KeyValueAlgo = hashAlgo.ToString(),
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
						KeyValueBase64 = pubKeyBase64,
						KeyValueAlgo = key.KeyAlgorithm.ToString(),
						KeySig = key.Fingerprint.ToString(hashAlgo, false),
						KeySigAlgo = hashAlgo.ToString(),
						Hostname = hostname,
						Enabled = true,
						Created = DateTime.Now
					});
				uow.Commit();
			}
			else if (privKeyExists != null && pubKeyExists == null)
			{
				uow.UserPublicKeys.Create(
					new tbl_UserPublicKeys
					{
						Id = pubId,
						PrivateKeyId = privKeyExists.Id,
						UserId = user.Id,
						KeyValueBase64 = pubKeyBase64,
						KeyValueAlgo = key.KeyAlgorithm.ToString(),
						KeySig = key.Fingerprint.ToString(hashAlgo, false),
						KeySigAlgo = hashAlgo.ToString(),
						Hostname = hostname,
						Enabled = true,
						Created = DateTime.Now
					});
				uow.Commit();
			}
			else if (privKeyExists == null && pubKeyExists != null)
			{
				uow.UserPrivateKeys.Create(
					new tbl_UserPrivateKeys
					{
						Id = privId,
						PublicKeyId = pubKeyExists.Id,
						UserId = user.Id,
						KeyValueBase64 = privKeyBase64,
						KeyValueAlgo = hashAlgo.ToString(),
						KeyValuePass = keyPass,
						Enabled = true,
						Created = DateTime.Now
					});
				uow.Commit();
			}

			return key;
		}

		public static SshPublicKey ImportSshPublicKey(IUnitOfWork uow, tbl_Users user, SignatureHashAlgorithm hashAlgo, string hostname, FileInfo inputFile)
		{
			var pubKey = new SshPublicKey(inputFile.FullName);
			var pubKeyBase64 = Convert.ToBase64String(pubKey.GetPublicKey(), Base64FormattingOptions.None);

			var pubKeyExists = uow.UserPublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserPublicKeys>()
				.Where(x => x.KeyValueBase64 == pubKeyBase64).ToLambda());

			if (pubKeyExists == null)
			{
				uow.UserPublicKeys.Create(
					new tbl_UserPublicKeys
					{
						Id = Guid.NewGuid(),
						UserId = user.Id,
						KeyValueBase64 = pubKeyBase64,
						KeyValueAlgo = pubKey.KeyAlgorithm.ToString(),
						KeySig = pubKey.Fingerprint.ToString(hashAlgo, false),
						KeySigAlgo = hashAlgo.ToString(),
						Hostname = hostname,
						Enabled = true,
						Created = DateTime.Now
					});
				uow.Commit();
			}

			return pubKey;
		}

		/*
		 * openssh uses base64 and special formatting for public keys
		 * https://man.openbsd.org/ssh-keygen
		 */
		public static SshPublicKey ImportSshPublicKeyBase64(IUnitOfWork uow, tbl_Users user, SignatureHashAlgorithm hashAlgo, FileInfo inputFile)
		{
			var file = File.ReadAllText(inputFile.FullName);
			var base64 = file.Split(" ");
			var asciiBytes = Convert.FromBase64String(base64[1]);

			var comment = base64[2].Split("@");

			var pubKey = new SshPublicKey(asciiBytes);
			var pubKeyBase64 = Convert.ToBase64String(pubKey.GetPublicKey(), Base64FormattingOptions.None);

			if (!uow.UserPublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserPublicKeys>()
				.Where(x => x.KeyValueBase64 == pubKeyBase64).ToLambda()).Any())
			{
				uow.UserPublicKeys.Create(
					new tbl_UserPublicKeys
					{
						Id = Guid.NewGuid(),
						UserId = user.Id,
						KeyValueBase64 = pubKeyBase64,
						KeyValueAlgo = pubKey.KeyAlgorithm.ToString(),
						KeySig = pubKey.Fingerprint.ToString(hashAlgo, false),
						KeySigAlgo = hashAlgo.ToString(),
						Hostname = comment[1],
						Enabled = true,
						Created = DateTime.Now
					});
				uow.Commit();
			}

			return pubKey;
		}

		/*
		 * openssh uses base64 and special formatting for public keys
		 * https://man.openbsd.org/ssh-keygen
		 */
		public static ICollection<SshPublicKey> ImportSshPublicKeysBase64(IUnitOfWork uow, tbl_Users user, SignatureHashAlgorithm hashAlgo, FileInfo inputFile)
		{
			var lines = File.ReadAllLines(inputFile.FullName);
			var pubKeys = new List<SshPublicKey>();

			foreach (var line in lines)
			{
				var base64 = line.Split(" ");
				var asciiBytes = Convert.FromBase64String(base64[1]);

				var comment = base64[2].Split("@");

				var pubKey = new SshPublicKey(asciiBytes);
				var pubKeyBase64 = Convert.ToBase64String(pubKey.GetPublicKey(), Base64FormattingOptions.None);

				pubKeys.Add(pubKey);

				if (!uow.UserPublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserPublicKeys>()
					.Where(x => x.KeyValueBase64 == pubKeyBase64).ToLambda()).Any())
				{
					uow.UserPublicKeys.Create(
						new tbl_UserPublicKeys
						{
							Id = Guid.NewGuid(),
							UserId = user.Id,
							KeyValueBase64 = pubKeyBase64,
							KeyValueAlgo = pubKey.KeyAlgorithm.ToString(),
							KeySig = pubKey.Fingerprint.ToString(hashAlgo, false),
							KeySigAlgo = hashAlgo.ToString(),
							Hostname = comment[1],
							Enabled = true,
							Created = DateTime.Now
						});
					uow.Commit();
				}
			}

			return pubKeys;
		}

		[Obsolete]
		public static List<SshPublicKey> ImportSshPublicKeys(string path)
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
	}
}
