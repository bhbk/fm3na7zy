using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Cryptography.Encryption;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Rebex.Net;
using Rebex.Security.Certificates;
using Rebex.Security.Cryptography.Pkcs;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bhbk.Lib.Aurora.Domain.Helpers
{
	public class KeyHelper
	{
		public static void CheckPrivKey(IConfiguration conf, IUnitOfWork uow,
			SshHostKeyAlgorithm keyAlgo, int privKeySize, string privKeyPass, SignatureHashAlgorithm sigAlgo)
		{
			var keyAlgoStr = keyAlgo.ToString();
			var privKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKeys>()
				.Where(x => x.KeyAlgo == keyAlgoStr && x.IdentityId == null && x.Immutable == true).ToLambda())
				.SingleOrDefault();

			if (privKey == null)
				CreatePrivKey(conf, uow, keyAlgo, privKeySize, privKeyPass, sigAlgo);
		}

		public static tbl_PrivateKeys CreatePrivKey(IConfiguration conf, IUnitOfWork uow,
			SshHostKeyAlgorithm keyAlgo, int privKeySize, string privKeyPass, SignatureHashAlgorithm sigAlgo)
		{
			var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
			var privId = Guid.NewGuid();
			var pubId = Guid.NewGuid();
			var privStream = new MemoryStream();
			var pubStream = new MemoryStream();
			var keyPair = SshPrivateKey.Generate(keyAlgo, privKeySize);

			keyPair.Save(privStream, privKeyPass, SshPrivateKeyFormat.Pkcs8);
			keyPair.SavePublicKey(pubStream, SshPublicKeyFormat.Pkcs8);

			var privKey = uow.PrivateKeys.Create(
				new tbl_PrivateKeys
				{
					Id = privId,
					PublicKeyId = pubId,
					IdentityId = null,
					KeyValue = Encoding.ASCII.GetString(privStream.ToArray()),
					KeyAlgo = keyPair.KeyAlgorithm.ToString(),
					KeyPass = AES.EncryptString(privKeyPass, conf["Databases:AuroraSecret"]),
					KeyFormat = SshPrivateKeyFormat.Pkcs8.ToString(),
					Enabled = true,
					Created = DateTime.Now,
					LastUpdated = null,
					Immutable = true,
				});

			Log.Information($"'{callPath}' 'system' private key algo {keyPair.KeyAlgorithm} sig {keyPair.Fingerprint.ToString(sigAlgo, false)}" +
				$"{Environment.NewLine}{privKey.KeyValue}");

			var pubKey = uow.PublicKeys.Create(
				new tbl_PublicKeys
				{
					Id = pubId,
					PrivateKeyId = privId,
					IdentityId = null,
					KeyValue = Encoding.ASCII.GetString(pubStream.ToArray()),
					KeyAlgo = keyPair.KeyAlgorithm.ToString(),
					KeyFormat = SshPublicKeyFormat.Pkcs8.ToString(),
					SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
					SigAlgo = sigAlgo.ToString(),
					Enabled = true,
					Created = DateTime.Now,
					LastUpdated = null,
					Immutable = true,
				});

			Log.Information($"'{callPath}' 'system' public key algo {keyPair.KeyAlgorithm} sig {keyPair.Fingerprint.ToString(sigAlgo, false)}" +
				$"{Environment.NewLine}{pubKey.KeyValue}");

			uow.Commit();

			return privKey;
		}

		public static tbl_PrivateKeys CreatePrivKey(IConfiguration conf, IUnitOfWork uow, tbl_Users user,
			SshHostKeyAlgorithm keyAlgo, int privKeySize, string privKeyPass, SignatureHashAlgorithm sigAlgo, string comment)
		{
			var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
			var privId = Guid.NewGuid();
			var pubId = Guid.NewGuid();
			var privStream = new MemoryStream();
			var pubStream = new MemoryStream();
			var keyPair = SshPrivateKey.Generate(keyAlgo, privKeySize);

			keyPair.Save(privStream, privKeyPass, SshPrivateKeyFormat.Pkcs8);
			keyPair.SavePublicKey(pubStream, SshPublicKeyFormat.Pkcs8);

			var privKey = uow.PrivateKeys.Create(
				new tbl_PrivateKeys
				{
					Id = privId,
					PublicKeyId = pubId,
					IdentityId = user.IdentityId,
					KeyValue = Encoding.ASCII.GetString(privStream.ToArray()),
					KeyAlgo = keyPair.KeyAlgorithm.ToString(),
					KeyPass = AES.EncryptString(privKeyPass, conf["Databases:AuroraSecret"]),
					KeyFormat = SshPrivateKeyFormat.Pkcs8.ToString(),
					Enabled = true,
					Created = DateTime.Now,
					LastUpdated = null,
					Immutable = false,
				});

			Log.Information($"'{callPath}' '{user.IdentityAlias}' private key algo {keyPair.KeyAlgorithm} sig {keyPair.Fingerprint.ToString(sigAlgo, false)}" +
				$"{Environment.NewLine}{privKey.KeyValue}");

			var pubKey = uow.PublicKeys.Create(
				new tbl_PublicKeys
				{
					Id = pubId,
					PrivateKeyId = privId,
					IdentityId = user.IdentityId,
					KeyValue = Encoding.ASCII.GetString(pubStream.ToArray()),
					KeyAlgo = keyPair.KeyAlgorithm.ToString(),
					KeyFormat = SshPublicKeyFormat.Pkcs8.ToString(),
					SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
					SigAlgo = sigAlgo.ToString(),
					Comment = comment,
					Enabled = true,
					Created = DateTime.Now,
					LastUpdated = null,
					Immutable = false,
				});

			Log.Information($"'{callPath}' '{user.IdentityAlias}' public key algo {keyPair.KeyAlgorithm} sig {keyPair.Fingerprint.ToString(sigAlgo, false)}" +
				$"{Environment.NewLine}{pubKey.KeyValue}");

			uow.Commit();

			return privKey;
		}

		public static ICollection<tbl_PrivateKeys> EditPrivKeySecrets(IUnitOfWork uow,
			ICollection<tbl_PrivateKeys> keys, string secretCurrent, string secretNew)
		{
			var privKeys = new List<tbl_PrivateKeys>();

			foreach (var key in keys)
			{
				var plainText = AES.DecryptString(key.KeyPass, secretCurrent);
				var cipherText = AES.EncryptString(plainText, secretCurrent);

				if (key.KeyPass != cipherText)
					throw new UnauthorizedAccessException();

				var privBytes = Encoding.ASCII.GetBytes(key.KeyValue);
				var privStream = new MemoryStream();
				var privKey = new SshPrivateKey(privBytes, plainText);

				SshPrivateKeyFormat keyFormat;

				if (!Enum.TryParse<SshPrivateKeyFormat>(key.KeyFormat, true, out keyFormat))
					throw new InvalidCastException();

				privKey.Save(privStream, plainText, keyFormat);

				key.KeyPass = AES.EncryptString(plainText, secretNew);
				key.LastUpdated = DateTime.Now;

				uow.PrivateKeys.Update(key);

				privKeys.Add(key);
			}

			uow.Commit();

			return privKeys;
		}

		public static byte[] ExportPrivKey(IConfiguration conf, tbl_PrivateKeys key, SshPrivateKeyFormat privKeyFormat, string privKeyPass)
		{
			var privBytes = Encoding.ASCII.GetBytes(key.KeyValue);
			var privStream = new MemoryStream();
			var privKey = new SshPrivateKey(privBytes, AES.DecryptString(key.KeyPass, conf["Databases:AuroraSecret"]));
			privKey.Save(privStream, privKeyPass, privKeyFormat);

			return privStream.ToArray();
		}

		public static byte[] ExportPubKey(tbl_PublicKeys key, SshPublicKeyFormat pubKeyFormat)
		{
			var pubBytes = Encoding.ASCII.GetBytes(key.KeyValue);
			var pubKeyInfo = new PublicKeyInfo();
			pubKeyInfo.Load(new MemoryStream(pubBytes));

			var pubStream = new MemoryStream();
			var pubKey = new SshPublicKey(pubKeyInfo);
			pubKey.SavePublicKey(pubStream, pubKeyFormat);

			return pubStream.ToArray();
		}

		/*
		 * openssh uses base64 and special formatting for public keys like with "authorized_keys"
		 * https://man.openbsd.org/ssh-keygen
		 */
		public static StringBuilder ExportPubKeyBase64(tbl_Users user, ICollection<tbl_PublicKeys> keys)
		{
			var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
			var sb = new StringBuilder();

			foreach (var key in keys)
			{
				var pubBytes = Encoding.ASCII.GetBytes(key.KeyValue);
				var pubKeyInfo = new PublicKeyInfo();
				pubKeyInfo.Load(new MemoryStream(pubBytes));

				var pubStream = new MemoryStream();
				var pubKey = new SshPublicKey(pubKeyInfo);
				pubKey.SavePublicKey(pubStream, SshPublicKeyFormat.Pkcs8);

				var algo = string.Empty;

				switch (pubKey.KeyAlgorithm)
				{
					case SshHostKeyAlgorithm.DSS:
						algo = "ssh-dsa";
						break;

					case SshHostKeyAlgorithm.RSA:
						algo = "ssh-rsa";
						break;

					//case SshHostKeyAlgorithm.ECDsaNistP256:
					//	algo = "ecdsa-sha2-nistp256";
					//	break;

					//case SshHostKeyAlgorithm.ECDsaNistP384:
					//	algo = "ecdsa-sha2-nistp384";
					//	break;

					//case SshHostKeyAlgorithm.ECDsaNistP521:
					//	algo = "ecdsa-sha2-nistp521";
					//	break;

					//case SshHostKeyAlgorithm.ED25519:
					//	algo = "ssh-ed25519";
					//	break;

					default:
						{
							Log.Warning($"'{callPath}' '{user.IdentityAlias}' algorithm {pubKey.KeyAlgorithm} not supported");
							continue;
						}
				}

				sb.AppendLine(new string($"{algo} {Convert.ToBase64String(pubKey.GetPublicKey())} {key.Comment}"));
			}

			return sb;
		}

		public static tbl_PrivateKeys ImportPrivKey(IConfiguration conf, IUnitOfWork uow,
			string privKeyPass, SignatureHashAlgorithm sigAlgo, FileInfo inputFile)
		{
			var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

			tbl_PrivateKeys pubKey = null;
			var keyPair = new SshPrivateKey(inputFile.FullName, privKeyPass);
			var privId = Guid.NewGuid();
			var pubId = Guid.NewGuid();
			var privStream = new MemoryStream();
			var pubStream = new MemoryStream();

			keyPair.Save(privStream, privKeyPass, SshPrivateKeyFormat.Pkcs8);
			keyPair.SavePublicKey(pubStream, SshPublicKeyFormat.Pkcs8);

			var privKeyValue = Encoding.ASCII.GetString(privStream.ToArray());
			var pubKeyValue = Encoding.ASCII.GetString(pubStream.ToArray());

			var privKeyFound = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKeys>()
				.Where(x => x.Id == null && x.KeyValue == privKeyValue).ToLambda())
				.SingleOrDefault();

			var pubKeyFound = uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
				.Where(x => x.Id == null && x.KeyValue == pubKeyValue).ToLambda())
				.SingleOrDefault();

			if (privKeyFound == null
				&& pubKeyFound == null)
			{
				uow.PrivateKeys.Create(
					new tbl_PrivateKeys
					{
						Id = privId,
						PublicKeyId = pubId,
						IdentityId = null,
						KeyValue = privKeyValue,
						KeyAlgo = keyPair.KeyAlgorithm.ToString(),
						KeyPass = AES.EncryptString(privKeyPass, conf["Databases:AuroraSecret"]),
						KeyFormat = SshPrivateKeyFormat.Pkcs8.ToString(),
						Enabled = true,
						Created = DateTime.Now,
						LastUpdated = null,
						Immutable = true,
					});

				Log.Information($"'{callPath}' 'system' private key algo {keyPair.KeyAlgorithm} sig {keyPair.Fingerprint.ToString(sigAlgo, false)}" +
					$"{Environment.NewLine}{privKeyValue}");

				uow.PublicKeys.Create(
					new tbl_PublicKeys
					{
						Id = pubId,
						PrivateKeyId = privId,
						IdentityId = null,
						KeyValue = pubKeyValue,
						KeyAlgo = keyPair.KeyAlgorithm.ToString(),
						KeyFormat = SshPublicKeyFormat.Pkcs8.ToString(),
						SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
						SigAlgo = sigAlgo.ToString(),
						Comment = null,
						Enabled = true,
						Created = DateTime.Now,
						LastUpdated = null,
						Immutable = true,
					});

				Log.Information($"'{callPath}' 'system' public key algo {keyPair.KeyAlgorithm} sig {keyPair.Fingerprint.ToString(sigAlgo, false)}" +
					$"{Environment.NewLine}{pubKeyValue}");
			}
			else if (privKeyFound != null
				&& pubKeyFound == null)
			{
				uow.PublicKeys.Create(
					new tbl_PublicKeys
					{
						Id = pubId,
						PrivateKeyId = privKeyFound.Id,
						IdentityId = null,
						KeyValue = pubKeyValue,
						KeyAlgo = keyPair.KeyAlgorithm.ToString(),
						KeyFormat = SshPublicKeyFormat.Pkcs8.ToString(),
						SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
						SigAlgo = sigAlgo.ToString(),
						Comment = null,
						Enabled = true,
						Created = DateTime.Now,
						LastUpdated = null,
						Immutable = true,
					});

				Log.Information($"'{callPath}' 'system' public key algo {keyPair.KeyAlgorithm} sig {keyPair.Fingerprint.ToString(sigAlgo, false)}" +
					$"{Environment.NewLine}{pubKeyValue}");
			}
			else if (privKeyFound == null
				&& pubKeyFound != null)
			{
				uow.PrivateKeys.Create(
					new tbl_PrivateKeys
					{
						Id = privId,
						PublicKeyId = pubKeyFound.Id,
						IdentityId = null,
						KeyValue = privKeyValue,
						KeyAlgo = keyPair.KeyAlgorithm.ToString(),
						KeyPass = AES.EncryptString(privKeyPass, conf["Databases:AuroraSecret"]),
						KeyFormat = SshPrivateKeyFormat.Pkcs8.ToString(),
						Enabled = true,
						Created = DateTime.Now,
						LastUpdated = null,
						Immutable = true,
					});

				Log.Information($"'{callPath}' 'system' private key algo {keyPair.KeyAlgorithm} sig {keyPair.Fingerprint.ToString(sigAlgo, false)}" +
					$"{Environment.NewLine}{privKeyValue}");
			}

			uow.Commit();

			return pubKey;
		}

		public static tbl_PrivateKeys ImportPrivKey(IConfiguration conf, IUnitOfWork uow, tbl_Users user,
			string privKeyPass, SignatureHashAlgorithm sigAlgo, string comment, FileInfo inputFile)
		{
			var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

			tbl_PrivateKeys pubKey = null;
			var keyPair = new SshPrivateKey(inputFile.FullName, privKeyPass);
			var privId = Guid.NewGuid();
			var pubId = Guid.NewGuid();
			var privStream = new MemoryStream();
			var pubStream = new MemoryStream();

			keyPair.Save(privStream, privKeyPass, SshPrivateKeyFormat.Pkcs8);
			keyPair.SavePublicKey(pubStream, SshPublicKeyFormat.Pkcs8);

			var privKeyValue = Encoding.ASCII.GetString(privStream.ToArray());
			var pubKeyValue = Encoding.ASCII.GetString(pubStream.ToArray());

			var privKeyFound = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKeys>()
				.Where(x => x.Id == user.IdentityId && x.KeyValue == privKeyValue).ToLambda())
				.SingleOrDefault();

			var pubKeyFound = uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
				.Where(x => x.Id == user.IdentityId && x.KeyValue == pubKeyValue).ToLambda())
				.SingleOrDefault();

			if (privKeyFound == null
				&& pubKeyFound == null)
			{
				uow.PrivateKeys.Create(
					new tbl_PrivateKeys
					{
						Id = privId,
						PublicKeyId = pubId,
						IdentityId = user.IdentityId,
						KeyValue = privKeyValue,
						KeyAlgo = keyPair.KeyAlgorithm.ToString(),
						KeyPass = AES.EncryptString(privKeyPass, conf["Databases:AuroraSecret"]),
						KeyFormat = SshPrivateKeyFormat.Pkcs8.ToString(),
						Enabled = true,
						Created = DateTime.Now,
						LastUpdated = null,
						Immutable = false,
					});

				Log.Information($"'{callPath}' '{user.IdentityAlias}' private key algo {keyPair.KeyAlgorithm} sig {keyPair.Fingerprint.ToString(sigAlgo, false)}" +
					$"{Environment.NewLine}{privKeyValue}");

				uow.PublicKeys.Create(
					new tbl_PublicKeys
					{
						Id = pubId,
						PrivateKeyId = privId,
						IdentityId = user.IdentityId,
						KeyValue = pubKeyValue,
						KeyAlgo = keyPair.KeyAlgorithm.ToString(),
						KeyFormat = SshPublicKeyFormat.Pkcs8.ToString(),
						SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
						SigAlgo = sigAlgo.ToString(),
						Comment = comment,
						Enabled = true,
						Created = DateTime.Now,
						LastUpdated = null,
						Immutable = false,
					});

				Log.Information($"'{callPath}' '{user.IdentityAlias}' public key algo {keyPair.KeyAlgorithm} sig {keyPair.Fingerprint.ToString(sigAlgo, false)}" +
					$"{Environment.NewLine}{pubKeyValue}");
			}
			else if (privKeyFound != null
				&& pubKeyFound == null)
			{
				uow.PublicKeys.Create(
					new tbl_PublicKeys
					{
						Id = pubId,
						PrivateKeyId = privKeyFound.Id,
						IdentityId = user.IdentityId,
						KeyValue = pubKeyValue,
						KeyAlgo = keyPair.KeyAlgorithm.ToString(),
						KeyFormat = SshPublicKeyFormat.Pkcs8.ToString(),
						SigValue = keyPair.Fingerprint.ToString(sigAlgo, false),
						SigAlgo = sigAlgo.ToString(),
						Comment = comment,
						Enabled = true,
						Created = DateTime.Now,
						LastUpdated = null,
						Immutable = false,
					});

				Log.Information($"'{callPath}' '{user.IdentityAlias}' public key algo {keyPair.KeyAlgorithm} sig {keyPair.Fingerprint.ToString(sigAlgo, false)}" +
					$"{Environment.NewLine}{pubKeyValue}");
			}
			else if (privKeyFound == null
				&& pubKeyFound != null)
			{
				uow.PrivateKeys.Create(
					new tbl_PrivateKeys
					{
						Id = privId,
						PublicKeyId = pubKeyFound.Id,
						IdentityId = user.IdentityId,
						KeyValue = privKeyValue,
						KeyAlgo = keyPair.Fingerprint.ToString(sigAlgo, false).ToString(),
						KeyPass = AES.EncryptString(privKeyPass, conf["Databases:AuroraSecret"]),
						Enabled = true,
						Created = DateTime.Now,
						LastUpdated = null,
						Immutable = false,
					});

				Log.Information($"'{callPath}' '{user.IdentityAlias}' private key algo {keyPair.KeyAlgorithm} sig {keyPair.Fingerprint.ToString(sigAlgo, false)}" +
					$"{Environment.NewLine}{privKeyValue}");
			}

			uow.Commit();

			return pubKey;
		}

		public static tbl_PublicKeys ImportPubKey(IUnitOfWork uow, tbl_Users user,
			SignatureHashAlgorithm sigAlgo, string hostname, FileInfo inputFile)
		{
			var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

			tbl_PublicKeys pubKeyEntity = null;
			var pubKey = new SshPublicKey(inputFile.FullName);
			var pubKeyStream = new MemoryStream();
			pubKey.SavePublicKey(pubKeyStream, SshPublicKeyFormat.Pkcs8);

			var pubKeyValue = Encoding.ASCII.GetString(pubKeyStream.ToArray());
			var pubKeyFound = uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
				.Where(x => x.Id == user.IdentityId && x.KeyValue == pubKeyValue).ToLambda());

			if (pubKeyFound == null)
			{
				pubKeyEntity = uow.PublicKeys.Create(
					new tbl_PublicKeys
					{
						Id = Guid.NewGuid(),
						IdentityId = user.IdentityId,
						KeyValue = pubKeyValue,
						KeyAlgo = pubKey.KeyAlgorithm.ToString(),
						KeyFormat = SshPublicKeyFormat.Pkcs8.ToString(),
						SigValue = pubKey.Fingerprint.ToString(sigAlgo, false),
						SigAlgo = sigAlgo.ToString(),
						Comment = hostname,
						Enabled = true,
						Created = DateTime.Now,
						LastUpdated = null,
						Immutable = false,
					});

				Log.Information($"'{callPath}' '{user.IdentityAlias}' public key algo {pubKey.KeyAlgorithm} sig {pubKey.Fingerprint.ToString(sigAlgo, false)}" +
					$"{Environment.NewLine}{pubKeyValue}");
			}
			else
			{
				Log.Warning($"'{callPath}' '{user.IdentityAlias}' skip public key algo {pubKey.KeyAlgorithm} sig {pubKey.Fingerprint.ToString(sigAlgo, false)}" +
					$"{Environment.NewLine}{pubKeyValue}");
			}

			uow.Commit();

			return pubKeyEntity;
		}

		/*
		 * openssh uses base64 and special formatting for public keys like with "authorized_keys"
		 * https://man.openbsd.org/ssh-keygen
		 */
		public static ICollection<tbl_PublicKeys> ImportPubKeyBase64(IUnitOfWork uow, tbl_Users user,
			SignatureHashAlgorithm sigAlgo, FileInfo inputFile)
		{
			var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
			var pubKeyLines = File.ReadAllLines(inputFile.FullName);
			var pubKeys = new List<tbl_PublicKeys>();

			foreach (var line in pubKeyLines)
			{
				var base64 = line.Split(" ");

				switch (base64[0])
				{
					case "ssh-dsa":
						break;

					case "ssh-rsa":
						break;

					//case "ecdsa-sha2-nistp256":
					//	break;

					//case "ecdsa-sha2-nistp384":
					//	break;

					//case "ecdsa-sha2-nistp521":
					//	break;

					//case "ssh-ed25519":
					//	break;

					default:
						{
							Log.Warning($"'{callPath}' '{user.IdentityAlias}' algorithm {base64[0]} not supported");
							continue;
						}
				}

				var pubKey = new SshPublicKey(Convert.FromBase64String(base64[1]));
				var pubKeyStream = new MemoryStream();
				pubKey.SavePublicKey(pubKeyStream, SshPublicKeyFormat.Pkcs8);

				var pubKeyValue = Encoding.ASCII.GetString(pubKeyStream.ToArray());

				if (!uow.PublicKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PublicKeys>()
					.Where(x => x.Id == user.IdentityId && x.KeyValue == pubKeyValue).ToLambda()).Any())
				{
					var newPubKey = uow.PublicKeys.Create(
						new tbl_PublicKeys
						{
							Id = Guid.NewGuid(),
							IdentityId = user.IdentityId,
							KeyValue = pubKeyValue,
							KeyAlgo = pubKey.KeyAlgorithm.ToString(),
							KeyFormat = SshPublicKeyFormat.Pkcs8.ToString(),
							SigValue = pubKey.Fingerprint.ToString(sigAlgo, false),
							SigAlgo = sigAlgo.ToString(),
							Comment = base64[2],
							Enabled = true,
							Created = DateTime.Now
						});

					pubKeys.Add(newPubKey);

					Log.Information($"'{callPath}' '{user.IdentityAlias}' public key algo {pubKey.KeyAlgorithm} sig {pubKey.Fingerprint.ToString(sigAlgo, false)}" +
						$"{Environment.NewLine}{pubKeyValue}");
				}
				else
				{
					Log.Warning($"'{callPath}' '{user.IdentityAlias}' skip public key algo {pubKey.KeyAlgorithm} sig {pubKey.Fingerprint.ToString(sigAlgo, false)}" +
						$"{Environment.NewLine}{pubKeyValue}");
				}
			}

			uow.Commit();

			return pubKeys;
		}
	}
}
