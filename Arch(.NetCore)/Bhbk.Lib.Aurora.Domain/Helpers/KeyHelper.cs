using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Rebex.Net;
using Rebex.Security.Certificates;
using Rebex.Security.Cryptography.Pkcs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Bhbk.Lib.Aurora.Domain.Helpers
{
	public class KeyHelper
	{
		public static void CheckDaemonSshPrivKey(IUnitOfWork uow,
			SshHostKeyAlgorithm keyAlgo, int privKeySize, string privKeyPass, SignatureHashAlgorithm sigAlgo)
		{
			var keyAlgoStr = keyAlgo.ToString();
			var privKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKeys>()
				.Where(x => x.KeyAlgo == keyAlgoStr && x.UserId == null && x.Immutable == true).ToLambda())
				.SingleOrDefault();

			if (privKey == null)
			{
				KeyHelper.CreateDaemonSshPrivKey(uow,
					SshPrivateKeyFormat.Pkcs8, keyAlgo, privKeySize, privKeyPass,
					SshPublicKeyFormat.Pkcs8, sigAlgo);

				uow.Commit();
			}
		}

		public static tbl_PrivateKeys CreateDaemonSshPrivKey(IUnitOfWork uow,
			SshPrivateKeyFormat privKeyFormat, SshHostKeyAlgorithm keyAlgo, int privKeySize, string privKeyPass,
			SshPublicKeyFormat pubKeyFormat, SignatureHashAlgorithm sigAlgo)
		{
			var privId = Guid.NewGuid();
			var pubId = Guid.NewGuid();
			var privStream = new MemoryStream();
			var pubStream = new MemoryStream();
			var keyPair = SshPrivateKey.Generate(keyAlgo, privKeySize);

			keyPair.Save(privStream, privKeyPass, privKeyFormat);
			keyPair.SavePublicKey(pubStream, pubKeyFormat);

			var privKey = uow.PrivateKeys.Create(
				new tbl_PrivateKeys
				{
					Id = privId,
					PublicKeyId = pubId,
					UserId = null,
					KeyValue = Encoding.ASCII.GetString(privStream.ToArray()),
					KeyAlgo = keyAlgo.ToString(),
					KeyPass = privKeyPass,
					KeyFormat = privKeyFormat.ToString(),
					Enabled = true,
					Created = DateTime.Now,
					LastUpdated = null,
					Immutable = true,
				});

			uow.PublicKeys.Create(
				new tbl_PublicKeys
				{
					Id = pubId,
					PrivateKeyId = privId,
					UserId = null,
					KeyValue = Encoding.ASCII.GetString(pubStream.ToArray()),
					KeyAlgo = keyAlgo.ToString(),
					KeyFormat = pubKeyFormat.ToString(),
					SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
					SigAlgo = sigAlgo.ToString(),
					Enabled = true,
					Created = DateTime.Now,
					LastUpdated = null,
					Immutable = true,
				});

			uow.Commit();

			return privKey;
		}

		public static tbl_PrivateKeys CreateUserSshPrivKey(IUnitOfWork uow, tbl_Users user,
			SshPrivateKeyFormat privKeyFormat, SshHostKeyAlgorithm keyAlgo, int privKeySize, string privKeyPass,
			SshPublicKeyFormat pubKeyFormat, SignatureHashAlgorithm sigAlgo, string hostname)
		{
			var privId = Guid.NewGuid();
			var pubId = Guid.NewGuid();
			var privStream = new MemoryStream();
			var pubStream = new MemoryStream();
			var keyPair = SshPrivateKey.Generate(keyAlgo, privKeySize);

			keyPair.Save(privStream, privKeyPass, privKeyFormat);
			keyPair.SavePublicKey(pubStream, pubKeyFormat);

			var privKey = uow.PrivateKeys.Create(
				new tbl_PrivateKeys
				{
					Id = privId,
					PublicKeyId = pubId,
					UserId = user.Id,
					KeyValue = Encoding.ASCII.GetString(privStream.ToArray()),
					KeyAlgo = keyAlgo.ToString(),
					KeyPass = privKeyPass,
					KeyFormat = privKeyFormat.ToString(),
					Enabled = true,
					Created = DateTime.Now,
					LastUpdated = null,
					Immutable = false,
				});

			uow.PublicKeys.Create(
				new tbl_PublicKeys
				{
					Id = pubId,
					PrivateKeyId = privId,
					UserId = user.Id,
					KeyValue = Encoding.ASCII.GetString(pubStream.ToArray()),
					KeyAlgo = keyAlgo.ToString(),
					KeyFormat = pubKeyFormat.ToString(),
					SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
					SigAlgo = sigAlgo.ToString(),
					Hostname = hostname,
					Enabled = true,
					Created = DateTime.Now,
					LastUpdated = null,
					Immutable = false,
				});

			uow.Commit();

			return privKey;
		}

		public static ICollection<tbl_PrivateKeys> RefreshSshPrivKeys(IUnitOfWork uow, ICollection<tbl_PrivateKeys> keys)
		{
			var privKeys = new List<tbl_PrivateKeys>();

			foreach (var key in keys)
			{
				var privBytes = Encoding.ASCII.GetBytes(key.KeyValue);
				var privStream = new MemoryStream();
				var privKey = new SshPrivateKey(privBytes, key.KeyPass);

				SshHostKeyAlgorithm keyAlgo;
				SshPrivateKeyFormat keyFormat;

				if (!Enum.TryParse<SshHostKeyAlgorithm>(key.KeyAlgo, true, out keyAlgo))
					throw new InvalidCastException();

				if (!Enum.TryParse<SshPrivateKeyFormat>(key.KeyFormat, true, out keyFormat))
					throw new InvalidCastException();

				privKey.Save(privStream, key.KeyPass, keyFormat);

				key.KeyValue = Encoding.ASCII.GetString(privStream.ToArray());
				key.KeyAlgo = keyAlgo.ToString();
				key.KeyFormat = keyFormat.ToString();
				key.LastUpdated = DateTime.Now;

				uow.PrivateKeys.Update(key);

				privKeys.Add(key);
			}

			uow.Commit();

			return privKeys;
		}

		public static ICollection<tbl_PublicKeys> RefreshSshPubKeys(IUnitOfWork uow, ICollection<tbl_PublicKeys> keys)
		{
			var pubKeys = new List<tbl_PublicKeys>();

			foreach (var key in keys)
			{
				var pubBytes = Encoding.ASCII.GetBytes(key.KeyValue);
				var pubStream = new MemoryStream();
				var pubKey = new SshPublicKey(pubBytes);

				SshHostKeyAlgorithm keyAlgo;
				SshPublicKeyFormat keyFormat;
				SignatureHashAlgorithm sigAlgo;

				if (!Enum.TryParse<SshHostKeyAlgorithm>(key.KeyAlgo, true, out keyAlgo))
					throw new InvalidCastException();

				if (!Enum.TryParse<SshPublicKeyFormat>(key.KeyFormat, true, out keyFormat))
					throw new InvalidCastException();

				if (!Enum.TryParse<SignatureHashAlgorithm>(key.SigAlgo, true, out sigAlgo))
					throw new InvalidCastException();

				pubKey.SavePublicKey(pubStream, keyFormat);

				key.KeyValue = Encoding.ASCII.GetString(pubStream.ToArray());
				key.KeyAlgo = keyAlgo.ToString();
				key.KeyFormat = keyFormat.ToString();
				key.SigValue = pubKey.Fingerprint.ToString(sigAlgo, false);
				key.SigAlgo = sigAlgo.ToString();
				key.LastUpdated = DateTime.Now;

				uow.PublicKeys.Update(key);

				pubKeys.Add(key);
			}

			uow.Commit();

			return pubKeys;
		}

		public static SshPrivateKey ExportSshPrivKey(tbl_PrivateKeys key, 
			SshPrivateKeyFormat privKeyFormat, string privKeyPass, FileInfo outputFile)
		{
			var privBytes = Encoding.ASCII.GetBytes(key.KeyValue);
			var privStream = new MemoryStream();
			var privKey = new SshPrivateKey(privBytes, key.KeyPass);
			privKey.Save(privStream, privKeyPass, privKeyFormat);

			File.WriteAllBytes(outputFile.FullName, privStream.ToArray());

			return privKey;
		}

		public static SshPublicKey ExportSshPubKey(tbl_PublicKeys key, 
			SshPublicKeyFormat pubKeyFormat, FileInfo outputFile)
		{
			var pubBytes = Encoding.ASCII.GetBytes(key.KeyValue);
			var pubKeyInfo = new PublicKeyInfo();
			pubKeyInfo.Load(new MemoryStream(pubBytes));

			var pubStream = new MemoryStream();
			var pubKey = new SshPublicKey(pubKeyInfo);
			pubKey.SavePublicKey(pubStream, pubKeyFormat);

			File.WriteAllBytes(outputFile.FullName, pubStream.ToArray());

			return pubKey;
		}

		/*
		 * openssh uses base64 and special formatting for public keys
		 * https://man.openbsd.org/ssh-keygen
		 */
		public static SshPublicKey ExportSshPubKeyBase64(tbl_Users user, tbl_PublicKeys key, FileInfo outputFile)
		{
			var pubBytes = Encoding.ASCII.GetBytes(key.KeyValue);
			var pubKeyInfo = new PublicKeyInfo();
			pubKeyInfo.Load(new MemoryStream(pubBytes));

			var pubStream = new MemoryStream();
			var pubKey = new SshPublicKey(pubKeyInfo);
			pubKey.SavePublicKey(pubStream, SshPublicKeyFormat.Pkcs8);

			var data = new string($"ssh-rsa {Convert.ToBase64String(pubKey.GetPublicKey())} {user.UserName}@{key.Hostname}");

			File.WriteAllText(outputFile.FullName, data);

			return pubKey;
		}

		/*
		 * openssh uses base64 and special formatting for public keys
		 * https://man.openbsd.org/ssh-keygen
		 */
		public static ICollection<SshPublicKey> ExportSshPubKeysBase64(tbl_Users user, ICollection<tbl_PublicKeys> keys, FileInfo outputFile)
		{
			var sb = new StringBuilder();
			var pubKeys = new List<SshPublicKey>();

			foreach (var key in keys)
			{
				var pubBytes = Encoding.ASCII.GetBytes(key.KeyValue);
				var pubKeyInfo = new PublicKeyInfo();
				pubKeyInfo.Load(new MemoryStream(pubBytes));

				var pubStream = new MemoryStream();
				var pubKey = new SshPublicKey(pubKeyInfo);
				pubKey.SavePublicKey(pubStream, SshPublicKeyFormat.Pkcs8);

				sb.AppendLine(new string($"ssh-rsa {Convert.ToBase64String(pubKey.GetPublicKey())} {user.UserName}@{key.Hostname}"));
				pubKeys.Add(pubKey);
			}

			File.WriteAllText(outputFile.FullName, sb.ToString());

			return pubKeys;
		}

		public static tbl_PrivateKeys ImportSshPrivKey(IUnitOfWork uow, tbl_Users user,
			SshPrivateKeyFormat privKeyFormat, SshHostKeyAlgorithm keyAlgo, string privKeyPass,
			SshPublicKeyFormat pubKeyFormat, SignatureHashAlgorithm sigAlgo, string hostname, FileInfo inputFile)
		{
			tbl_PrivateKeys pubKey = null;
			var keyPair = new SshPrivateKey(inputFile.FullName, privKeyPass);

			var privId = Guid.NewGuid();
			var pubId = Guid.NewGuid();
			var privStream = new MemoryStream();
			var pubStream = new MemoryStream();

			keyPair.Save(privStream, privKeyPass, privKeyFormat);
			keyPair.SavePublicKey(pubStream, pubKeyFormat);

			var privKeyValue = Encoding.ASCII.GetString(privStream.ToArray());
			var pubKeyValue = Encoding.ASCII.GetString(pubStream.ToArray());

			var privKeyFound = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKeys>()
				.Where(x => x.KeyValue == privKeyValue).ToLambda()).SingleOrDefault();

			var pubKeyFound = uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
				.Where(x => x.KeyValue == pubKeyValue).ToLambda()).SingleOrDefault();

			if (privKeyFound == null 
				&& pubKeyFound == null)
			{
				uow.PrivateKeys.Create(
					new tbl_PrivateKeys
					{
						Id = privId,
						PublicKeyId = pubId,
						UserId = user.Id,
						KeyValue = privKeyValue,
						KeyAlgo = keyAlgo.ToString(),
						KeyPass = privKeyPass,
						KeyFormat = privKeyFormat.ToString(),
						Enabled = true,
						Created = DateTime.Now,
						LastUpdated = null,
						Immutable = false,
					});

				uow.PublicKeys.Create(
					new tbl_PublicKeys
					{
						Id = pubId,
						PrivateKeyId = privId,
						UserId = user.Id,
						KeyValue = pubKeyValue,
						KeyAlgo = keyAlgo.ToString(),
						KeyFormat = pubKeyFormat.ToString(),
						SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
						SigAlgo = sigAlgo.ToString(),
						Hostname = hostname,
						Enabled = true,
						Created = DateTime.Now,
						LastUpdated = null,
						Immutable = false,
					});
			}
			else if (privKeyFound != null 
				&& pubKeyFound == null)
			{
				uow.PublicKeys.Create(
					new tbl_PublicKeys
					{
						Id = pubId,
						PrivateKeyId = privKeyFound.Id,
						UserId = user.Id,
						KeyValue = pubKeyValue,
						KeyAlgo = keyAlgo.ToString(),
						KeyFormat = pubKeyFormat.ToString(),
						SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
						SigAlgo = sigAlgo.ToString(),
						Hostname = hostname,
						Enabled = true,
						Created = DateTime.Now,
						LastUpdated = null,
						Immutable = false,
					});
			}
			else if (privKeyFound == null 
				&& pubKeyFound != null)
			{
				uow.PrivateKeys.Create(
					new tbl_PrivateKeys
					{
						Id = privId,
						PublicKeyId = pubKeyFound.Id,
						UserId = user.Id,
						KeyValue = privKeyValue,
						KeyAlgo = sigAlgo.ToString(),
						KeyPass = privKeyPass,
						Enabled = true,
						Created = DateTime.Now,
						LastUpdated = null,
						Immutable = false,
					});
			}

			uow.Commit();

			return pubKey;
		}

		public static tbl_PublicKeys ImportSshPubKey(IUnitOfWork uow, tbl_Users user, 
			SshHostKeyAlgorithm keyAlgo, SshPublicKeyFormat keyFormat, SignatureHashAlgorithm sigAlgo, string hostname, FileInfo inputFile)
		{
			tbl_PublicKeys pubKey = null;
			var key = new SshPublicKey(inputFile.FullName);
			var keyStream = new MemoryStream();
			key.SavePublicKey(keyStream, keyFormat);

			var pubKeyValue = Encoding.ASCII.GetString(keyStream.ToArray());
			var pubKeyFound = uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
				.Where(x => x.KeyValue == pubKeyValue).ToLambda());

			if (pubKeyFound == null)
			{
				pubKey = uow.PublicKeys.Create(
					new tbl_PublicKeys
					{
						Id = Guid.NewGuid(),
						UserId = user.Id,
						KeyValue = pubKeyValue,
						KeyAlgo = keyAlgo.ToString(),
						KeyFormat = keyFormat.ToString(),
						SigValue = key.Fingerprint.ToString(sigAlgo, false),
						SigAlgo = sigAlgo.ToString(),
						Hostname = hostname,
						Enabled = true,
						Created = DateTime.Now,
						LastUpdated = null,
						Immutable = false,
					});
			}

			uow.Commit();

			return pubKey;
		}

		/*
		 * openssh uses base64 and special formatting for public keys
		 * https://man.openbsd.org/ssh-keygen
		 */
		public static SshPublicKey ImportSshPubKeyBase64(IUnitOfWork uow, tbl_Users user, 
			SignatureHashAlgorithm sigAlgo, FileInfo inputFile)
		{
			var file = File.ReadAllText(inputFile.FullName);
			var base64 = file.Split(" ");
			var asciiBytes = Convert.FromBase64String(base64[1]);

			var comment = base64[2].Split("@");

			var pubKey = new SshPublicKey(asciiBytes);
			var pubKeyBase64 = Convert.ToBase64String(pubKey.GetPublicKey(), Base64FormattingOptions.None);

			if (!uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
				.Where(x => x.KeyValue == pubKeyBase64).ToLambda()).Any())
			{
				uow.PublicKeys.Create(
					new tbl_PublicKeys
					{
						Id = Guid.NewGuid(),
						UserId = user.Id,
						KeyValue = pubKeyBase64,
						KeyAlgo = pubKey.KeyAlgorithm.ToString(),
						SigValue = pubKey.Fingerprint.ToString(sigAlgo, false),
						SigAlgo = sigAlgo.ToString(),
						Hostname = comment[1],
						Enabled = true,
						Created = DateTime.Now
					});
			}

			uow.Commit();

			return pubKey;
		}

		/*
		 * openssh uses base64 and special formatting for public keys
		 * https://man.openbsd.org/ssh-keygen
		 */
		public static ICollection<SshPublicKey> ImportSshPubKeysBase64(IUnitOfWork uow, tbl_Users user, 
			SignatureHashAlgorithm sigAlgo, FileInfo inputFile)
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

				if (!uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
					.Where(x => x.KeyValue == pubKeyBase64).ToLambda()).Any())
				{
					uow.PublicKeys.Create(
						new tbl_PublicKeys
						{
							Id = Guid.NewGuid(),
							UserId = user.Id,
							KeyValue = pubKeyBase64,
							KeyAlgo = pubKey.KeyAlgorithm.ToString(),
							SigValue = pubKey.Fingerprint.ToString(sigAlgo, false),
							SigAlgo = sigAlgo.ToString(),
							Hostname = comment[1],
							Enabled = true,
							Created = DateTime.Now
						});
				}
			}

			uow.Commit();

			return pubKeys;
		}
	}
}
