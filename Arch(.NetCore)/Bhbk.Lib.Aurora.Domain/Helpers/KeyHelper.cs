using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Cryptography.Encryption;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Rebex.Net;
using Rebex.Security.Certificates;
using Rebex.Security.Cryptography.Pkcs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bhbk.Lib.Aurora.Domain.Helpers
{
    public class KeyHelper
	{
		public static (PublicKey_EF, PrivateKey_EF) CreateKeyPair(IConfiguration conf, IUnitOfWork uow,
			SshHostKeyAlgorithm keyAlgo, SignatureHashAlgorithm sigAlgo, int keySize, string keyPass)
		{
			var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
			var privId = Guid.NewGuid();
			var pubId = Guid.NewGuid();
			var privStream = new MemoryStream();
			var pubStream = new MemoryStream();
			var keyPair = SshPrivateKey.Generate(keyAlgo, keySize);

			keyPair.Save(privStream, keyPass, SshPrivateKeyFormat.Pkcs8);
			keyPair.SavePublicKey(pubStream, SshPublicKeyFormat.Pkcs8);

			var privKey = new PrivateKey_EF
			{
				Id = privId,
				PublicKeyId = pubId,
				KeyValue = Encoding.UTF8.GetString(privStream.ToArray()),
				KeyAlgorithmId = (int)keyPair.KeyAlgorithm,
				EncryptedPass = AES.EncryptString(keyPass, conf["Databases:AuroraSecret"]),
				KeyFormatId = (int)SshPrivateKeyFormat.Pkcs8,
				IsEnabled = true,
				IsDeletable = true,
			};

			Log.Information($"'{callPath}' 'system' creating new private key... " +
				$"{Environment.NewLine} algo: {privKey.KeyAlgorithmId} format: {privKey.KeyFormatId} " +
				$"{Environment.NewLine}{privKey.KeyValue}");

			var pubKey = new PublicKey_EF
			{
				Id = pubId,
				PrivateKeyId = privId,
				KeyValue = Encoding.UTF8.GetString(pubStream.ToArray()),
				KeyAlgorithmId = (int)keyPair.KeyAlgorithm,
				KeyFormatId = (int)SshPublicKeyFormat.Pkcs8,
				SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
				SigAlgorithmId = (int)sigAlgo,
				IsEnabled = true,
				IsDeletable = true,
			};

			Log.Information($"'{callPath}' 'system' creating new public key... " +
				$"{Environment.NewLine} algo: {pubKey.KeyAlgorithmId} format: {pubKey.KeyFormatId} " +
				$"{Environment.NewLine} sig: {pubKey.SigValue}" +
				$"{Environment.NewLine}{pubKey.KeyValue}");

			return (pubKey, privKey);
		}

		public static (PublicKey_EF, PrivateKey_EF) CreateKeyPair(IConfiguration conf, IUnitOfWork uow, Login_EF user,
			SshHostKeyAlgorithm keyAlgo, SignatureHashAlgorithm sigAlgo, int keySize, string keyPass, string comment)
		{
			var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
			var privId = Guid.NewGuid();
			var pubId = Guid.NewGuid();
			var privStream = new MemoryStream();
			var pubStream = new MemoryStream();
			var keyPair = SshPrivateKey.Generate(keyAlgo, keySize);

			keyPair.Save(privStream, keyPass, SshPrivateKeyFormat.Pkcs8);
			keyPair.SavePublicKey(pubStream, SshPublicKeyFormat.Pkcs8);

			var privKey = new PrivateKey_EF
			{
				Id = privId,
				UserId = user.UserId,
				PublicKeyId = pubId,
				KeyValue = Encoding.UTF8.GetString(privStream.ToArray()),
				KeyAlgorithmId = (int)keyPair.KeyAlgorithm,
				EncryptedPass = AES.EncryptString(keyPass, conf["Databases:AuroraSecret"]),
				KeyFormatId = (int)SshPrivateKeyFormat.Pkcs8,
				IsEnabled = true,
				IsDeletable = true,
			};

			Log.Information($"'{callPath}' '{user.UserName}' creating new private key... " +
				$"{Environment.NewLine} algo: {privKey.KeyAlgorithmId} format: {privKey.KeyFormatId} " +
				$"{Environment.NewLine}{privKey.KeyValue}");

			var pubKey = new PublicKey_EF
			{
				Id = pubId,
				UserId = user.UserId,
				PrivateKeyId = privId,
				KeyValue = Encoding.UTF8.GetString(pubStream.ToArray()),
				KeyAlgorithmId = (int)keyPair.KeyAlgorithm,
				KeyFormatId = (int)SshPublicKeyFormat.Pkcs8,
				SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
				SigAlgorithmId = (int)sigAlgo,
				Comment = comment,
				IsEnabled = true,
				IsDeletable = true,
			};

			Log.Information($"'{callPath}' '{user.UserName}' creating new public key... " +
				$"{Environment.NewLine} algo: {pubKey.KeyAlgorithmId} format: {pubKey.KeyFormatId} " +
				$"{Environment.NewLine} sig: {pubKey.SigValue}" +
				$"{Environment.NewLine}{pubKey.KeyValue}");

			return (pubKey, privKey);
		}

		public static ICollection<PrivateKey_EF> ChangePrivKeySecrets(ICollection<PrivateKey_EF> keys, 
			string secretCurrent, string secretNew)
		{
			var privKeys = new List<PrivateKey_EF>();

			foreach (var key in keys)
			{
				var plainText = AES.DecryptString(key.EncryptedPass, secretCurrent);
				var cipherText = AES.EncryptString(plainText, secretCurrent);

				if (key.EncryptedPass != cipherText)
					throw new UnauthorizedAccessException();

				var privBytes = Encoding.UTF8.GetBytes(key.KeyValue);
				var privStream = new MemoryStream();
				var privKey = new SshPrivateKey(privBytes, plainText);

				SshPrivateKeyFormat keyFormat;

				if (!Enum.TryParse<SshPrivateKeyFormat>(key.KeyFormatId.ToString(), true, out keyFormat))
					throw new InvalidCastException();

				privKey.Save(privStream, plainText, keyFormat);

				key.EncryptedPass = AES.EncryptString(plainText, secretNew);

				privKeys.Add(key);
			}

			return privKeys;
		}

		public static byte[] ExportPrivKey(PrivateKey_EF key, SshPrivateKeyFormat keyFormat, string keyPass)
		{
			var privBytes = Encoding.UTF8.GetBytes(key.KeyValue);
			var privStream = new MemoryStream();
			var privKey = new SshPrivateKey(privBytes, keyPass);
			privKey.Save(privStream, keyPass, keyFormat);

			return privStream.ToArray();
		}

		public static byte[] ExportPubKey(PublicKey_EF key, SshPublicKeyFormat keyFormat)
		{
			var pubBytes = Encoding.UTF8.GetBytes(key.KeyValue);
			var pubKeyInfo = new PublicKeyInfo();
			pubKeyInfo.Load(new MemoryStream(pubBytes));

			var pubStream = new MemoryStream();
			var pubKey = new SshPublicKey(pubKeyInfo);
			pubKey.SavePublicKey(pubStream, keyFormat);

			return pubStream.ToArray();
		}

		/*
		 * openssh uses base64 and special formatting for public keys like with "authorized_keys"
		 * https://man.openbsd.org/ssh-keygen
		 */
		public static StringBuilder ExportPubKeyBase64(Login_EF user, ICollection<PublicKey_EF> keys)
		{
			var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
			var sb = new StringBuilder();

			foreach (var key in keys)
			{
				var pubBytes = Encoding.UTF8.GetBytes(key.KeyValue);
				var pubKeyInfo = new PublicKeyInfo();
				pubKeyInfo.Load(new MemoryStream(pubBytes));

				var pubStream = new MemoryStream();
				var pubKey = new SshPublicKey(pubKeyInfo);
				pubKey.SavePublicKey(pubStream, SshPublicKeyFormat.Ssh2Base64);

				var algo = string.Empty;

				switch (pubKey.KeyAlgorithm)
				{
					case SshHostKeyAlgorithm.DSS:
						algo = "ssh-dsa";
						break;

					case SshHostKeyAlgorithm.RSA:
						algo = "ssh-rsa";
						break;

					case SshHostKeyAlgorithm.ECDsaNistP256:
					//algo = "ecdsa-sha2-nistp256";

					case SshHostKeyAlgorithm.ECDsaNistP384:
					//algo = "ecdsa-sha2-nistp384";

					case SshHostKeyAlgorithm.ECDsaNistP521:
					//algo = "ecdsa-sha2-nistp521";

					case SshHostKeyAlgorithm.ED25519:
					//algo = "ssh-ed25519";

					default:
						{
							Log.Warning($"'{callPath}' '{user.UserName}' algorithm {pubKey.KeyAlgorithm} not supported");
							continue;
						}
				}

				sb.AppendLine($"{algo} {Convert.ToBase64String(pubKey.GetPublicKey())} {key.Comment}");
			}

			return sb;
		}

		public static (PublicKey_EF, PrivateKey_EF) ImportKeyPair(IConfiguration conf, IUnitOfWork uow,
			SignatureHashAlgorithm sigAlgo, MemoryStream stream, string keyPass)
		{
			var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

			stream.Position = 0;

			PrivateKey_EF privKey = null, privKeyFound = null;
			PublicKey_EF pubKey = null, pubKeyFound = null;
			SshPrivateKey keyPair = null;

			if (string.IsNullOrEmpty(keyPass))
				keyPair = new SshPrivateKey(stream);
			else
				keyPair = new SshPrivateKey(stream, keyPass);

			keyPass = AlphaNumeric.CreateString(32);

			var privId = Guid.NewGuid();
			var pubId = Guid.NewGuid();
			var privStream = new MemoryStream();
			var pubStream = new MemoryStream();

			keyPair.Save(privStream, keyPass, SshPrivateKeyFormat.Pkcs8);
			keyPair.SavePublicKey(pubStream, SshPublicKeyFormat.Pkcs8);

			var privKeyValue = Encoding.UTF8.GetString(privStream.ToArray());
			var pubKeyValue = Encoding.UTF8.GetString(pubStream.ToArray());

			pubKeyFound = uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<PublicKey_EF>()
				.Where(x => x.UserId == null && x.KeyValue == pubKeyValue).ToLambda())
				.SingleOrDefault();

			/*
			 * a private key is stored encrypted. to find private key by value can only work if the password to encrypt
			 * on import is same as password used to encrypt the private key already stored. this will never happen.
			 * a band-aid below is find public key and then look for private key association.
			 */

			if (pubKeyFound != null)
				privKeyFound = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey_EF>()
					.Where(x => x.UserId == null && x.PublicKeyId == pubKeyFound.Id).ToLambda())
					.SingleOrDefault();

			if (privKeyFound == null
				&& pubKeyFound == null)
			{
				privKey = new PrivateKey_EF
				{
					Id = privId,
					PublicKeyId = pubId,
					KeyValue = privKeyValue,
					KeyAlgorithmId = (int)keyPair.KeyAlgorithm,
					EncryptedPass = AES.EncryptString(keyPass, conf["Databases:AuroraSecret"]),
					KeyFormatId = (int)SshPrivateKeyFormat.Pkcs8,
					IsEnabled = false,
					IsDeletable = true,
				};

				Log.Information($"'{callPath}' 'system' import private key... " +
					$"{Environment.NewLine} pass:'{keyPass}' " +
					$"{Environment.NewLine} algo:'{privKey.KeyAlgorithmId}' format:'{privKey.KeyFormatId}' " +
					$"{Environment.NewLine}{privKey.KeyValue}");

				pubKey = new PublicKey_EF
				{
					Id = pubId,
					PrivateKeyId = privId,
					KeyValue = pubKeyValue,
					KeyAlgorithmId = (int)keyPair.KeyAlgorithm,
					KeyFormatId = (int)SshPublicKeyFormat.Pkcs8,
					SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
					SigAlgorithmId = (int)sigAlgo,
					IsEnabled = false,
					IsDeletable = true,
				};

				Log.Information($"'{callPath}' 'system' import public key... " +
					$"{Environment.NewLine} algo:'{pubKey.KeyAlgorithmId}' format:'{pubKey.KeyFormatId}' " +
					$"{Environment.NewLine} sig:'{pubKey.SigValue}'" +
					$"{Environment.NewLine}{pubKey.KeyValue}");
			}
			else if (privKeyFound != null
				&& pubKeyFound == null)
			{
				privKey = privKeyFound;

				Log.Warning($"'{callPath}' 'system' skip import... " +
					$"{Environment.NewLine} *** private key with GUID {privKeyFound.Id} already exists ***" +
					$"{Environment.NewLine} algo:'{privKeyFound.KeyAlgorithmId}' format:'{privKeyFound.KeyFormatId}' " +
					$"{Environment.NewLine}{privKeyFound.KeyValue}");

				pubKey = new PublicKey_EF
				{
					Id = pubId,
					PrivateKeyId = privKeyFound.Id,
					KeyValue = pubKeyValue,
					KeyAlgorithmId = (int)keyPair.KeyAlgorithm,
					KeyFormatId = (int)SshPublicKeyFormat.Pkcs8,
					SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
					SigAlgorithmId = (int)sigAlgo,
					IsEnabled = false,
					IsDeletable = true,
				};

				Log.Information($"'{callPath}' 'system' import public key... " +
					$"{Environment.NewLine} algo:'{pubKey.KeyAlgorithmId}' format:'{pubKey.KeyFormatId}' " +
					$"{Environment.NewLine} sig:'{pubKey.SigValue}'" +
					$"{Environment.NewLine}{pubKey.KeyValue}");
			}
			else if (privKeyFound == null
				&& pubKeyFound != null)
			{
				pubKey = pubKeyFound;

				Log.Warning($"'{callPath}' 'system' skip import... " +
					$"{Environment.NewLine} *** public key with GUID {pubKeyFound.Id} already exists ***" +
					$"{Environment.NewLine} algo: {pubKeyFound.KeyAlgorithmId} format: {pubKeyFound.KeyFormatId} " +
					$"{Environment.NewLine} sig: {pubKeyFound.SigValue}" +
					$"{Environment.NewLine}{pubKeyFound.KeyValue}");

				privKey = new PrivateKey_EF
				{
					Id = privId,
					PublicKeyId = pubKeyFound.Id,
					KeyValue = privKeyValue,
					KeyAlgorithmId = (int)keyPair.KeyAlgorithm,
					EncryptedPass = AES.EncryptString(keyPass, conf["Databases:AuroraSecret"]),
					KeyFormatId = (int)SshPrivateKeyFormat.Pkcs8,
					IsEnabled = false,
					IsDeletable = true,
				};

				Log.Information($"'{callPath}' 'system' import private key... " +
					$"{Environment.NewLine} pass:'{keyPass}' " +
					$"{Environment.NewLine} algo:'{privKey.KeyAlgorithmId}' format:'{privKey.KeyFormatId}' " +
					$"{Environment.NewLine}{privKey.KeyValue}");
			}
			else
			{
				privKey = privKeyFound;
				pubKey = pubKeyFound;

				Log.Warning($"'{callPath}' 'system' skip import... " +
					$"{Environment.NewLine} *** private key with GUID {privKeyFound.Id} already exists ***" +
					$"{Environment.NewLine} algo:'{privKeyFound.KeyAlgorithmId}' format:'{privKeyFound.KeyFormatId}' " +
					$"{Environment.NewLine}{privKeyFound.KeyValue}");

				Log.Warning($"'{callPath}' 'system' skip import... " +
					$"{Environment.NewLine} *** public key with GUID {pubKeyFound.Id} already exists ***" +
					$"{Environment.NewLine} algo: {pubKeyFound.KeyAlgorithmId} format: {pubKeyFound.KeyFormatId} " +
					$"{Environment.NewLine} sig: {pubKeyFound.SigValue}" +
					$"{Environment.NewLine}{pubKeyFound.KeyValue}");
			}

			return (pubKey, privKey);
		}

		public static (PublicKey_EF, PrivateKey_EF) ImportKeyPair(IConfiguration conf, IUnitOfWork uow, Login_EF user,
			SignatureHashAlgorithm sigAlgo, MemoryStream stream, string keyPass, string comment)
		{
			var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

			stream.Position = 0;

			PrivateKey_EF privKey = null, privKeyFound = null;
			PublicKey_EF pubKey = null, pubKeyFound = null;
			SshPrivateKey keyPair = null;

			if (string.IsNullOrEmpty(keyPass))
				keyPair = new SshPrivateKey(stream);
			else
				keyPair = new SshPrivateKey(stream, keyPass);

			keyPass = AlphaNumeric.CreateString(32);

			var privId = Guid.NewGuid();
			var pubId = Guid.NewGuid();
			var privStream = new MemoryStream();
			var pubStream = new MemoryStream();

			keyPair.Save(privStream, keyPass, SshPrivateKeyFormat.Pkcs8);
			keyPair.SavePublicKey(pubStream, SshPublicKeyFormat.Pkcs8);

			var privKeyValue = Encoding.UTF8.GetString(privStream.ToArray());
			var pubKeyValue = Encoding.UTF8.GetString(pubStream.ToArray());

			pubKeyFound = uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<PublicKey_EF>()
				.Where(x => x.UserId == user.UserId && x.KeyValue == pubKeyValue).ToLambda())
				.SingleOrDefault();

			/*
			 * a private key is stored encrypted. to find private key by value can only work if the password to encrypt
			 * on import is same as password used to encrypt the private key already stored. this will never happen.
			 * a band-aid below is find public key and then look for private key association.
			 */

			if (pubKeyFound != null)
				privKeyFound = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey_EF>()
					.Where(x => x.UserId == user.UserId && x.PublicKeyId == pubKeyFound.Id).ToLambda())
					.SingleOrDefault();

			if (privKeyFound == null
				&& pubKeyFound == null)
			{
				privKey = new PrivateKey_EF
				{
					Id = privId,
					PublicKeyId = pubId,
					UserId = user.UserId,
					KeyValue = privKeyValue,
					KeyAlgorithmId = (int)keyPair.KeyAlgorithm,
					EncryptedPass = AES.EncryptString(keyPass, conf["Databases:AuroraSecret"]),
					KeyFormatId = (int)SshPrivateKeyFormat.Pkcs8,
					IsEnabled = false,
					IsDeletable = true,
				};

				Log.Information($"'{callPath}' '{user.UserName}' import private key... " +
					$"{Environment.NewLine} pass:'{keyPass}' " +
					$"{Environment.NewLine} algo:'{privKey.KeyAlgorithmId}' format:'{privKey.KeyFormatId}' " +
					$"{Environment.NewLine}{privKey.KeyValue}");

				pubKey = new PublicKey_EF
				{
					Id = pubId,
					PrivateKeyId = privId,
					UserId = user.UserId,
					KeyValue = pubKeyValue,
					KeyAlgorithmId = (int)keyPair.KeyAlgorithm,
					KeyFormatId = (int)SshPublicKeyFormat.Pkcs8,
					SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
					SigAlgorithmId = (int)sigAlgo,
					Comment = comment,
					IsEnabled = false,
					IsDeletable = true,
				};

				Log.Information($"'{callPath}' '{user.UserName}' import public key... " +
					$"{Environment.NewLine} algo:'{pubKey.KeyAlgorithmId}' format:'{pubKey.KeyFormatId}' " +
					$"{Environment.NewLine} sig:'{pubKey.SigValue}'" +
					$"{Environment.NewLine}{pubKey.KeyValue}");
			}
			else if (privKeyFound != null
				&& pubKeyFound == null)
			{
				privKey = privKeyFound;

				Log.Warning($"'{callPath}' '{user.UserName}' skip import... " +
					$"{Environment.NewLine} *** private key with GUID {privKeyFound.Id} already exists ***" +
					$"{Environment.NewLine} algo:'{privKeyFound.KeyAlgorithmId}' format:'{privKeyFound.KeyFormatId}' " +
					$"{Environment.NewLine}{privKeyFound.KeyValue}");

				pubKey = new PublicKey_EF
				{
					Id = pubId,
					PrivateKeyId = privKeyFound.Id,
					UserId = user.UserId,
					KeyValue = pubKeyValue,
					KeyAlgorithmId = (int)keyPair.KeyAlgorithm,
					KeyFormatId = (int)SshPublicKeyFormat.Pkcs8,
					SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
					SigAlgorithmId = (int)sigAlgo,
					Comment = comment,
					IsEnabled = false,
					IsDeletable = true,
				};

				Log.Information($"'{callPath}' '{user.UserName}' import public key... " +
					$"{Environment.NewLine} algo:'{pubKey.KeyAlgorithmId}' format:'{pubKey.KeyFormatId}' " +
					$"{Environment.NewLine} sig:'{pubKey.SigValue}'" +
					$"{Environment.NewLine}{pubKey.KeyValue}");
			}
			else if (privKeyFound == null
				&& pubKeyFound != null)
			{
				pubKey = pubKeyFound;

				Log.Warning($"'{callPath}' '{user.UserName}' skip import... " +
					$"{Environment.NewLine} *** public key with GUID {pubKeyFound.Id} already exists ***" +
					$"{Environment.NewLine} algo:'{pubKeyFound.KeyAlgorithmId}' format:'{pubKeyFound.KeyFormatId}' " +
					$"{Environment.NewLine} sig:'{pubKeyFound.SigValue}'" +
					$"{Environment.NewLine}{pubKeyFound.KeyValue}");

				privKey = new PrivateKey_EF
				{
					Id = privId,
					PublicKeyId = pubKeyFound.Id,
					UserId = user.UserId,
					KeyValue = privKeyValue,
					KeyAlgorithmId = (int)keyPair.KeyAlgorithm,
					EncryptedPass = AES.EncryptString(keyPass, conf["Databases:AuroraSecret"]),
					IsEnabled = false,
					IsDeletable = true,
				};

				Log.Information($"'{callPath}' '{user.UserName}' import private key... " +
					$"{Environment.NewLine} pass:'{keyPass}' " +
					$"{Environment.NewLine} algo:'{privKey.KeyAlgorithmId}' format:'{privKey.KeyFormatId}' " +
					$"{Environment.NewLine}{privKey.KeyValue}");
			}
			else
			{
				privKey = privKeyFound;
				pubKey = pubKeyFound;

				Log.Warning($"'{callPath}' 'system' skip import... " +
					$"{Environment.NewLine} *** private key with GUID {privKeyFound.Id} already exists ***" +
					$"{Environment.NewLine} algo:'{privKeyFound.KeyAlgorithmId}' format:'{privKeyFound.KeyFormatId}' " +
					$"{Environment.NewLine}{privKeyFound.KeyValue}");

				Log.Warning($"'{callPath}' 'system' skip import... " +
					$"{Environment.NewLine} *** public key with GUID {pubKeyFound.Id} already exists ***" +
					$"{Environment.NewLine} algo: {pubKeyFound.KeyAlgorithmId} format: {pubKeyFound.KeyFormatId} " +
					$"{Environment.NewLine} sig: {pubKeyFound.SigValue}" +
					$"{Environment.NewLine}{pubKeyFound.KeyValue}");
			}

			return (pubKey, privKey);
		}

		public static PublicKey_EF ImportPubKey(IUnitOfWork uow, Login_EF user,
			SignatureHashAlgorithm sigAlgo, string comment, MemoryStream stream)
		{
			var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

			stream.Position = 0;

			PublicKey_EF pubKey = null;
			var importedPubKey = new SshPublicKey(stream);
			var importedPubKeyStream = new MemoryStream();
			importedPubKey.SavePublicKey(importedPubKeyStream, SshPublicKeyFormat.Pkcs8);

			Log.Information($"'{callPath}' '{user.UserName}' show original public key... " +
				$"{Environment.NewLine} algo:'{importedPubKey.KeyAlgorithm}' " +
				$"{Environment.NewLine} sig:'{importedPubKey.Fingerprint.ToString(sigAlgo, false)}'" +
				$"{Environment.NewLine}{Encoding.UTF8.GetString(stream.ToArray())}");

			var pubKeyValue = Encoding.UTF8.GetString(importedPubKeyStream.ToArray());
			var pubKeyFound = uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<PublicKey_EF>()
				.Where(x => x.UserId == user.UserId && x.KeyValue == pubKeyValue).ToLambda())
				.SingleOrDefault();

			if (pubKeyFound == null)
			{
				pubKey = new PublicKey_EF
				{
					Id = Guid.NewGuid(),
					UserId = user.UserId,
					KeyValue = pubKeyValue,
					KeyAlgorithmId = (int)importedPubKey.KeyAlgorithm,
					KeyFormatId = (int)SshPublicKeyFormat.Pkcs8,
					SigValue = importedPubKey.Fingerprint.ToString(sigAlgo, false),
					SigAlgorithmId = (int)sigAlgo,
					Comment = comment,
					IsEnabled = false,
					IsDeletable = true,
				};

				Log.Information($"'{callPath}' '{user.UserName}' import public key... " +
					$"{Environment.NewLine} algo:'{pubKey.KeyAlgorithmId}' format:'{pubKey.KeyFormatId}' " +
					$"{Environment.NewLine} sig:'{pubKey.SigValue}'" +
					$"{Environment.NewLine}{pubKey.KeyValue}");
			}
			else
			{
				Log.Warning($"'{callPath}' '{user.UserName}' skip import... " +
					$"{Environment.NewLine} *** public key with GUID {pubKeyFound.Id} already exists ***" +
					$"{Environment.NewLine} algo:'{pubKeyFound.KeyAlgorithmId}' format:'{pubKeyFound.KeyFormatId}' " +
					$"{Environment.NewLine} sig:'{pubKeyFound.SigValue}'" +
					$"{Environment.NewLine}{pubKeyFound.KeyValue}");
			}

			return pubKey;
		}

		/*
		 * openssh uses base64 and special formatting for public keys like with "authorized_keys"
		 * https://man.openbsd.org/ssh-keygen
		 */
		public static ICollection<PublicKey_EF> ImportPubKeyBase64(IUnitOfWork uow, Login_EF user,
			SignatureHashAlgorithm sigAlgo, MemoryStream stream)
		{
			var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

			var pubKeys = new List<PublicKey_EF>();
			var pubKeyLines = new List<string>();

			stream.Position = 0;

			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				string line = String.Empty;

				while ((line = reader.ReadLine()) != null)
					pubKeyLines.Add(line);
			}

			foreach (var line in pubKeyLines)
			{
				var base64 = line.Split(' ');

				switch (base64[0])
				{
					case "ssh-dsa":
						break;

					case "ssh-rsa":
						break;

					case "ecdsa-sha2-nistp256":

					case "ecdsa-sha2-nistp384":

					case "ecdsa-sha2-nistp521":

					case "ssh-ed25519":

					default:
						{
							Log.Warning($"'{callPath}' '{user.UserName}' algorithm '{base64[0]}' not supported");
							continue;
						}
				}

				var importedPubKey = new SshPublicKey(Convert.FromBase64String(base64[1]));
				var importedPubKeyStream = new MemoryStream();
				importedPubKey.SavePublicKey(importedPubKeyStream, SshPublicKeyFormat.Pkcs8);

				Log.Information($"'{callPath}' '{user.UserName}' show original public key... " +
					$"{Environment.NewLine} algo:'{base64[0]}'" +
					$"{Environment.NewLine} comment:'{base64[2]}'" +
					$"{Environment.NewLine}{base64[1]}");

				var pubKeyValue = Encoding.UTF8.GetString(importedPubKeyStream.ToArray());
				var pubKeyFound = uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<PublicKey_EF>()
					.Where(x => x.UserId == user.UserId && x.KeyValue == pubKeyValue).ToLambda())
					.SingleOrDefault();

				if (pubKeyFound == null)
				{
					var pubKey = new PublicKey_EF
					{
						Id = Guid.NewGuid(),
						UserId = user.UserId,
						KeyValue = pubKeyValue,
						KeyAlgorithmId = (int)importedPubKey.KeyAlgorithm,
						KeyFormatId = (int)SshPublicKeyFormat.Pkcs8,
						SigValue = importedPubKey.Fingerprint.ToString(sigAlgo, false),
						SigAlgorithmId = (int)sigAlgo,
						Comment = base64[2],
						IsEnabled = false,
						IsDeletable = true,
					};

					pubKeys.Add(pubKey);

					Log.Information($"'{callPath}' '{user.UserName}' import public key... " +
						$"{Environment.NewLine} algo:'{pubKey.KeyAlgorithmId}' format:'{pubKey.KeyFormatId}' " +
						$"{Environment.NewLine} sig:'{pubKey.SigValue}'" +
						$"{Environment.NewLine}{pubKey.KeyValue}");
				}
				else
				{
					Log.Warning($"'{callPath}' '{user.UserName}' skip import..." +
						$"{Environment.NewLine} *** public key with GUID {pubKeyFound.Id} already exists ***" +
						$"{Environment.NewLine} algo:'{pubKeyFound.KeyAlgorithmId}' format:'{pubKeyFound.KeyFormatId}' " +
						$"{Environment.NewLine} sig:'{pubKeyFound.SigValue}'" +
						$"{Environment.NewLine}{pubKeyFound.KeyValue}");
				}
			}

			return pubKeys;
		}
	}
}
