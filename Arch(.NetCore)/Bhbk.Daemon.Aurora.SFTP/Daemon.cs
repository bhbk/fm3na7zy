using Bhbk.Daemon.Aurora.SFTP.Factories;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Aurora.Domain.Primitives;
using Bhbk.Lib.Aurora.Domain.Primitives.Enums;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Bhbk.Lib.Cryptography.Encryption;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.Identity.Models.Sts;
using Bhbk.Lib.Identity.Services;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebex;
using Rebex.IO.FileSystem.Notifications;
using Rebex.Net;
using Rebex.Net.Servers;
using Rebex.Security.Certificates;
using Rebex.Security.Cryptography;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/*
 * https://www.rebex.net/file-server/features/easy-to-use-api.aspx
 */

namespace Bhbk.Daemon.Aurora.SFTP
{
    public class Daemon : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _factory;
        private readonly FileServer _server;
        private IEnumerable<string> _binding;
        private LogLevel _level;
        private bool _disposed;

        public Daemon(IServiceScopeFactory factory)
        {
            _factory = factory;
            _server = new FileServer();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    AsymmetricKeyAlgorithm.Register(Curve25519.Create);
                    AsymmetricKeyAlgorithm.Register(Ed25519.Create);
                    AsymmetricKeyAlgorithm.Register(EllipticCurveAlgorithm.Create);

                    using (var scope = _factory.CreateScope())
                    {
                        var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        if (!Enum.TryParse<LogLevel>(conf["Rebex:LogLevel"], true, out _level))
                            throw new InvalidCastException();

                        var license = uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<tbl_Setting>()
                            .Where(x => x.ConfigKey == "RebexLicense").ToLambda()).OrderBy(x => x.CreatedUtc)
                            .Last();

                        Rebex.Licensing.Key = license.ConfigValue;

                        KeyHelper.CheckPrivKey(conf, uow, SshHostKeyAlgorithm.DSS, 1024, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
                        KeyHelper.CheckPrivKey(conf, uow, SshHostKeyAlgorithm.RSA, 4096, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
                        KeyHelper.CheckPrivKey(conf, uow, SshHostKeyAlgorithm.ECDsaNistP256, 256, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
                        KeyHelper.CheckPrivKey(conf, uow, SshHostKeyAlgorithm.ECDsaNistP384, 384, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
                        KeyHelper.CheckPrivKey(conf, uow, SshHostKeyAlgorithm.ECDsaNistP521, 521, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
                        KeyHelper.CheckPrivKey(conf, uow, SshHostKeyAlgorithm.ED25519, 256, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);

                        var secret = conf["Databases:AuroraSecret"];

                        var dsaStr = SshHostKeyAlgorithm.DSS.ToString();
                        var dsaPrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKey>()
                            .Where(x => x.KeyAlgo == dsaStr && x.IdentityId == null).ToLambda()).OrderBy(x => x.CreatedUtc)
                            .Single();

                        var dsaBytes = Encoding.ASCII.GetBytes(dsaPrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(dsaBytes, AES.DecryptString(dsaPrivKey.KeyPass, secret)));

                        var rsaStr = SshHostKeyAlgorithm.RSA.ToString();
                        var rsaPrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKey>()
                            .Where(x => x.KeyAlgo == rsaStr && x.IdentityId == null).ToLambda()).OrderBy(x => x.CreatedUtc)
                            .Single();

                        var rsaBytes = Encoding.ASCII.GetBytes(rsaPrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(rsaBytes, AES.DecryptString(rsaPrivKey.KeyPass, secret)));

                        var ecdsa256Str = SshHostKeyAlgorithm.ECDsaNistP256.ToString();
                        var ecdsa256PrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKey>()
                            .Where(x => x.KeyAlgo == ecdsa256Str && x.IdentityId == null).ToLambda()).OrderBy(x => x.CreatedUtc)
                            .Single();

                        var ecdsa256Bytes = Encoding.ASCII.GetBytes(ecdsa256PrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(ecdsa256Bytes, AES.DecryptString(ecdsa256PrivKey.KeyPass, secret)));

                        var ecdsa384Str = SshHostKeyAlgorithm.ECDsaNistP384.ToString();
                        var ecdsa384PrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKey>()
                            .Where(x => x.KeyAlgo == ecdsa384Str && x.IdentityId == null).ToLambda()).OrderBy(x => x.CreatedUtc)
                            .Single();

                        var ecdsa384Bytes = Encoding.ASCII.GetBytes(ecdsa384PrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(ecdsa384Bytes, AES.DecryptString(ecdsa384PrivKey.KeyPass, secret)));

                        var ecdsa521Str = SshHostKeyAlgorithm.ECDsaNistP521.ToString();
                        var ecdsa521PrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKey>()
                            .Where(x => x.KeyAlgo == ecdsa521Str && x.IdentityId == null).ToLambda()).OrderBy(x => x.CreatedUtc)
                            .Single();

                        var ecdsa521Bytes = Encoding.ASCII.GetBytes(ecdsa521PrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(ecdsa521Bytes, AES.DecryptString(ecdsa521PrivKey.KeyPass, secret)));

                        var ed25519Str = SshHostKeyAlgorithm.ED25519.ToString();
                        var ed25519PrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKey>()
                            .Where(x => x.KeyAlgo == ed25519Str && x.IdentityId == null).ToLambda()).OrderBy(x => x.CreatedUtc)
                            .Single();

                        var ed25519Bytes = Encoding.ASCII.GetBytes(ed25519PrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(ed25519Bytes, AES.DecryptString(ed25519PrivKey.KeyPass, secret)));

                        _binding = conf.GetSection("Daemons:SftpService:Bindings").GetChildren().Select(x => x.Value);
                    }

                    foreach (var binding in _binding)
                    {
                        var pair = binding.Split("|");

                        _server.Bind(new IPEndPoint(IPAddress.Parse(pair[0]), int.Parse(pair[1])), FileServerProtocol.Sftp);
#if !RELEASE
                        _server.Bind(new IPEndPoint(IPAddress.Parse(pair[0]), int.Parse(pair[1])), FileServerProtocol.Shell);
#endif
                    }

                    _server.LogWriter = new ConsoleLogWriter(_level);
                    _server.Settings.AllowedAuthenticationMethods = AuthenticationMethods.PublicKey | AuthenticationMethods.Password;
                    _server.Settings.SshParameters.EncryptionAlgorithms = SshEncryptionAlgorithm.Any;
                    _server.Settings.SshParameters.EncryptionModes = SshEncryptionMode.Any;
                    _server.Settings.SshParameters.KeyExchangeAlgorithms = SshKeyExchangeAlgorithm.Any;
                    _server.Settings.SshParameters.HostKeyAlgorithms = SshHostKeyAlgorithm.Any;
                    _server.Settings.SshParameters.MacAlgorithms = SshMacAlgorithm.Any;
                    _server.Authentication += FsUser_Authentication;
                    _server.Connecting += FsUser_Connecting;
                    _server.Disconnected += FsUser_Disconnected;
                    _server.FileDownloaded += FsUser_FileDownloaded;
                    _server.FileUploaded += FsUser_FileUploaded;
                    _server.PreAuthentication += FsUser_PreAuthentication;
                    _server.Start();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (_server.IsRunning)
                    {
                        foreach (var binding in _binding)
                        {
                            var pair = binding.Split("|");

                            _server.Unbind(new IPEndPoint(IPAddress.Parse(pair[0]), int.Parse(pair[1])));
                        }

                        _server.Stop();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }, cancellationToken);
        }

        private void FsNotify_CreatePreview(object sender, PreviewSingleNodeOperationEventArgs e)
        {
            try
            {
                if (e.Node.IsFile)
                {
                    var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

                    Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file '{e.Node.Path.StringPath}'");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void FsNotify_CreateCompleted(object sender, SingleNodeOperationEventArgs e)
        {
            try
            {
                if (e.ResultNode.IsFile)
                {
                    var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

                    Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file '{e.ResultNode.Path.StringPath}'");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void FsNotify_DeletePreview(object sender, PreviewSingleNodeOperationEventArgs e)
        {
            try
            {
                if (e.Node.IsFile)
                {
                    var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

                    Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file '{e.Node.Path.StringPath}'");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void FsNotify_DeleteCompleted(object sender, SingleNodeOperationEventArgs e)
        {
            try
            {
                if (e.ResultNode.IsFile)
                {
                    var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

                    using (var scope = _factory.CreateScope())
                    {
                        var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var admin = scope.ServiceProvider.GetRequiredService<IAdminService>();
                        var alert = scope.ServiceProvider.GetRequiredService<IAlertService>();

                        var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                            .Where(x => x.IdentityAlias == ServerSession.Current.UserName).ToLambda())
                            .Single();

                        foreach (var email in uow.UserAlerts.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserAlert>()
                            .Where(x => x.IdentityId == user.IdentityId && !string.IsNullOrWhiteSpace(x.ToEmailAddress) && x.OnDelete == true).ToLambda()))
                        {
                            alert.Enqueue_EmailV1(new EmailV1()
                            {
                                FromEmail = conf["Notifications:EmailFromAddress"],
                                FromDisplay = conf["Notifications:EmailFromDisplayName"],
                                ToEmail = email.ToEmailAddress,
                                ToDisplay = $"{email.ToFirstName} {email.ToLastName}",
                                Subject = "File Delete Alert",
                                Body = Templates.NotifyEmailOnFileDelete(conf["Daemons:SftpService:Dns"], ServerSession.Current.UserName,
                                    email.ToFirstName, email.ToLastName, e.ResultNode.Path.StringPath)
                            }).AsTask();
                        }

                        foreach (var text in uow.UserAlerts.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserAlert>()
                            .Where(x => x.IdentityId == user.IdentityId && !string.IsNullOrWhiteSpace(x.ToPhoneNumber) && x.OnDelete == true).ToLambda()))
                        {
                            alert.Enqueue_TextV1(new TextV1()
                            {
                                FromPhoneNumber = conf["Notifications:TextFromPhoneNumber"],
                                ToPhoneNumber = text.ToPhoneNumber,
                                Body = Templates.NotifyTextOnFileDelete(conf["Daemons:SftpService:Dns"], ServerSession.Current.UserName,
                                    text.ToFirstName, text.ToLastName, e.ResultNode.Path.StringPath)
                            }).AsTask();
                        }
                    }

                    Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file '{e.ResultNode.Path.StringPath}'");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void FsUser_PreAuthentication(object sender, PreAuthenticationEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#pre-authentication
             */
            try
            {
                var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                        .Where(x => x.IdentityAlias == e.UserName && x.IsEnabled).ToLambda(),
                            new List<Expression<Func<tbl_User, object>>>()
                            {
                                x => x.tbl_Networks,
                                x => x.tbl_PublicKeys,
                            }).SingleOrDefault();

                    if (user == null
                        || user.IsEnabled == false)
                    {
                        Log.Warning($"'{callPath}' '{e.UserName}' not found or not enabled");

                        e.Reject();
                        return;
                    }

                    var action = NetworkAction.Deny.ToString();
                    if (NetworkHelper.ValidateAddress(user.tbl_Networks.Where(x => x.Action == action && x.IsEnabled), e.ClientAddress))
                    {
                        Log.Warning($"'{callPath}' '{e.UserName}' is denied from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}'");

                        e.Reject();
                        return;
                    }

                    action = NetworkAction.Allow.ToString();
                    if (!NetworkHelper.ValidateAddress(user.tbl_Networks.Where(x => x.Action == action && x.IsEnabled), e.ClientAddress))
                    {
                        Log.Warning($"'{callPath}' '{e.UserName}' is not allowed from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}'");

                        e.Reject();
                        return;
                    }

                    Log.Information($"'{callPath}' '{e.UserName}' allowed from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}'");

                    if (user.RequirePublicKey
                        && user.RequirePassword)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' allowed with public key and password");

                        e.Accept(AuthenticationMethods.PublicKey | AuthenticationMethods.Password);
                        return;
                    }

                    if (user.RequirePublicKey
                        && !user.RequirePassword)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' allowed with public key");

                        e.Accept(AuthenticationMethods.PublicKey);
                        return;
                    }

                    if (!user.RequirePublicKey
                        && user.RequirePassword)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' allowed with password");

                        e.Accept(AuthenticationMethods.Password);
                        return;
                    }

                    Log.Warning($"'{callPath}' '{e.UserName}' denied");

                    e.Reject();
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void FsUser_Authentication(object sender, AuthenticationEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#authentication
             */
            try
            {
                var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

                using (var scope = _factory.CreateScope())
                {
                    var log = scope.ServiceProvider.GetRequiredService<ILogger>();
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var admin = scope.ServiceProvider.GetRequiredService<IAdminService>();
                    var sts = scope.ServiceProvider.GetRequiredService<IStsService>();

                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                        .Where(x => x.IdentityAlias == e.UserName && x.IsEnabled).ToLambda(),
                            new List<Expression<Func<tbl_User, object>>>()
                            {
                                x => x.tbl_PublicKeys,
                                x => x.tbl_UserMount,
                            }).SingleOrDefault();

                    if (e.Key != null)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' in-progress with public key");

                        if (UserHelper.ValidatePubKey(user.tbl_PublicKeys.Where(x => x.IsEnabled).ToList(), e.Key)
                            && admin.User_VerifyV1(user.IdentityId).AsTask().Result)
                        {
                            Log.Information($"'{callPath}' '{e.UserName}' success with public key");

                            if (e.PartiallyAccepted
                                || !user.RequirePassword)
                            {
                                /*
                                 * an smb mount will not succeed without a user password or ambassador credential.
                                 */
                                if (user.FileSystemType == FileSystemTypes.SMB.ToString()
                                    && !user.tbl_UserMount.CredentialId.HasValue)
                                {
                                    Log.Warning($"'{callPath}' '{e.UserName}' failure no credential to create {FileSystemTypes.SMB} filesystem");

                                    e.Reject();
                                    return;
                                }

                                var fs = FileSystemFactory.CreateFileSystem(_factory, log, user, e.UserName, e.Password);
                                var fsUser = new FileServerUser(e.UserName, e.Password);
                                fsUser.SetFileSystem(fs);

                                var fsNotify = fs.GetFileSystemNotifier();
                                fsNotify.CreatePreview += FsNotify_CreatePreview;
                                fsNotify.CreateCompleted += FsNotify_CreateCompleted;
                                fsNotify.DeletePreview += FsNotify_DeletePreview;
                                fsNotify.DeleteCompleted += FsNotify_DeleteCompleted;

                                e.Accept(fsUser);
                                return;
                            }
                            else
                            {
                                /*
                                 * authenticate partially if another kind of credential has not been provided yet.
                                 */
                                e.AcceptPartially();
                                return;
                            }
                        }
                        else
                        {
                            Log.Warning($"'{callPath}' '{e.UserName}' failure with public key");

                            e.Reject();
                            return;
                        }
                    }

                    if (e.Password != null)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' in-progress with password");

                        try
                        {
                            var identity = admin.User_GetV1(user.IdentityId.ToString()).AsTask().Result;

                            var auth = sts.ResourceOwner_GrantV2(
                                new ResourceOwnerV2()
                                {
                                    issuer = conf["IdentityCredentials:IssuerName"],
                                    client = conf["IdentityCredentials:AudienceName"],
                                    grant_type = "password",
                                    user = identity.UserName,
                                    password = e.Password,
                                }).AsTask().Result;

                            Log.Information($"'{callPath}' '{e.UserName}' success with password");

                            if (e.PartiallyAccepted
                                || !user.RequirePublicKey)
                            {
                                var fs = FileSystemFactory.CreateFileSystem(_factory, log, user, e.UserName, e.Password);
                                var fsUser = new FileServerUser(e.UserName, e.Password);
                                fsUser.SetFileSystem(fs);

                                var fsNotify = fs.GetFileSystemNotifier();
                                fsNotify.CreatePreview += FsNotify_CreatePreview;
                                fsNotify.CreateCompleted += FsNotify_CreateCompleted;
                                fsNotify.DeletePreview += FsNotify_DeletePreview;
                                fsNotify.DeleteCompleted += FsNotify_DeleteCompleted;

                                e.Accept(fsUser);
                                return;
                            }
                            else
                            {
                                /*
                                 * authenticate partially if another kind of credential has not been provided yet.
                                 */
                                e.AcceptPartially();
                                return;
                            }
                        }
                        catch (HttpRequestException)
                        {
                            Log.Warning($"'{callPath}' '{e.UserName}' failure with password");

                            e.Reject();
                            return;
                        }
                    }

                    Log.Warning($"'{callPath}' '{e.UserName}' denied");

                    e.Reject();
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void FsUser_Connecting(object sender, ConnectingEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#connecting
             */
            try
            {
                var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

                Log.Information($"'{callPath}' from {e.ClientEndPoint}");

                e.Accept = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void FsUser_FileDownloaded(object sender, FileTransferredEventArgs e)
        {
            try
            {
                var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

                using (var scope = _factory.CreateScope())
                {
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var admin = scope.ServiceProvider.GetRequiredService<IAdminService>();
                    var alert = scope.ServiceProvider.GetRequiredService<IAlertService>();

                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                        .Where(x => x.IdentityAlias == ServerSession.Current.UserName).ToLambda())
                        .Single();

                    foreach (var email in uow.UserAlerts.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserAlert>()
                        .Where(x => x.IdentityId == user.IdentityId && !string.IsNullOrWhiteSpace(x.ToEmailAddress) && x.OnDownload == true).ToLambda()))
                    {
                        alert.Enqueue_EmailV1(new EmailV1()
                        {
                            FromEmail = conf["Notifications:EmailFromAddress"],
                            FromDisplay = conf["Notifications:EmailFromDisplayName"],
                            ToEmail = email.ToEmailAddress,
                            ToDisplay = $"{email.ToFirstName} {email.ToLastName}",
                            Subject = "File Download Alert",
                            Body = Templates.NotifyEmailOnFileDownload(conf["Daemons:SftpService:Dns"], ServerSession.Current.UserName,
                                email.ToFirstName, email.ToLastName, "/" + e.FullPath, e.BytesTransferred.ToString(), e.Session.ClientEndPoint.ToString())
                        }).AsTask();
                    }

                    foreach (var text in uow.UserAlerts.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserAlert>()
                        .Where(x => x.IdentityId == user.IdentityId && !string.IsNullOrWhiteSpace(x.ToPhoneNumber) && x.OnDownload == true).ToLambda()))
                    {
                        alert.Enqueue_TextV1(new TextV1()
                        {
                            FromPhoneNumber = conf["Notifications:TextFromPhoneNumber"],
                            ToPhoneNumber = text.ToPhoneNumber,
                            Body = Templates.NotifyTextOnFileDownload(conf["Daemons:SftpService:Dns"], ServerSession.Current.UserName,
                                text.ToFirstName, text.ToLastName, "/" + e.FullPath, e.BytesTransferred.ToString(), e.Session.ClientEndPoint.ToString())
                        }).AsTask();
                    }
                }

                Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file '/{e.FullPath}' bytes {e.BytesTransferred} from {e.Session.ClientEndPoint}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void FsUser_FileUploaded(object sender, FileTransferredEventArgs e)
        {
            try
            {
                var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

                using (var scope = _factory.CreateScope())
                {
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var admin = scope.ServiceProvider.GetRequiredService<IAdminService>();
                    var alert = scope.ServiceProvider.GetRequiredService<IAlertService>();

                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_User>()
                        .Where(x => x.IdentityAlias == ServerSession.Current.UserName).ToLambda())
                        .Single();

                    foreach (var email in uow.UserAlerts.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserAlert>()
                        .Where(x => x.IdentityId == user.IdentityId && !string.IsNullOrWhiteSpace(x.ToEmailAddress) && x.OnUpload == true).ToLambda()))
                    {
                        alert.Enqueue_EmailV1(new EmailV1()
                        {
                            FromEmail = conf["Notifications:EmailFromAddress"],
                            FromDisplay = conf["Notifications:EmailFromDisplayName"],
                            ToEmail = email.ToEmailAddress,
                            ToDisplay = $"{email.ToFirstName} {email.ToLastName}",
                            Subject = "File Upload Alert",
                            Body = Templates.NotifyEmailOnFileUpload(conf["Daemons:SftpService:Dns"], ServerSession.Current.UserName,
                                email.ToFirstName, email.ToLastName, "/" + e.FullPath, e.BytesTransferred.ToString(), e.Session.ClientEndPoint.ToString())
                        }).AsTask();
                    }

                    foreach (var text in uow.UserAlerts.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserAlert>()
                        .Where(x => x.IdentityId == user.IdentityId && !string.IsNullOrWhiteSpace(x.ToPhoneNumber) && x.OnUpload == true).ToLambda()))
                    {
                        alert.Enqueue_TextV1(new TextV1()
                        {
                            FromPhoneNumber = conf["Notifications:TextFromPhoneNumber"],
                            ToPhoneNumber = text.ToPhoneNumber,
                            Body = Templates.NotifyTextOnFileUpload(conf["Daemons:SftpService:Dns"], ServerSession.Current.UserName,
                                text.ToFirstName, text.ToLastName, "/" + e.FullPath, e.BytesTransferred.ToString(), e.Session.ClientEndPoint.ToString())
                        }).AsTask();
                    }
                }

                Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file '/{e.FullPath}' bytes {e.BytesTransferred} from {e.Session.ClientEndPoint}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void FsUser_Disconnected(object sender, DisconnectedEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#disconnected
             */
            try
            {
                var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

                Log.Information($"'{callPath}' from {e.Session.ClientEndPoint} after {e.Session.Duration}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _server.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Daemon()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
