using Bhbk.Daemon.Aurora.SFTP.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Aurora.Domain.Primitives;
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
using System.Data;
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

                        var license = uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<Setting>()
                            .Where(x => x.ConfigKey == "RebexLicense").ToLambda()).OrderBy(x => x.CreatedUtc)
                            .Last();

                        Rebex.Licensing.Key = license.ConfigValue;

                        KeyHelper.CheckKeyPair(conf, uow, SshHostKeyAlgorithm.DSS, 1024, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
                        KeyHelper.CheckKeyPair(conf, uow, SshHostKeyAlgorithm.RSA, 4096, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
                        KeyHelper.CheckKeyPair(conf, uow, SshHostKeyAlgorithm.ECDsaNistP256, 256, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
                        KeyHelper.CheckKeyPair(conf, uow, SshHostKeyAlgorithm.ECDsaNistP384, 384, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
                        KeyHelper.CheckKeyPair(conf, uow, SshHostKeyAlgorithm.ECDsaNistP521, 521, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
                        KeyHelper.CheckKeyPair(conf, uow, SshHostKeyAlgorithm.ED25519, 256, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);

                        var secret = conf["Databases:AuroraSecret"];

                        var dsaStr = SshHostKeyAlgorithm.DSS.ToString();
                        var dsaPrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey>()
                            .Where(x => x.IdentityId == null && x.KeyAlgo == dsaStr).ToLambda())
                            .OrderBy(x => x.CreatedUtc)
                            .Last();

                        var dsaBytes = Encoding.ASCII.GetBytes(dsaPrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(dsaBytes, AES.DecryptString(dsaPrivKey.KeyPass, secret)));

                        var rsaStr = SshHostKeyAlgorithm.RSA.ToString();
                        var rsaPrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey>()
                            .Where(x => x.IdentityId == null && x.KeyAlgo == rsaStr).ToLambda())
                            .OrderBy(x => x.CreatedUtc)
                            .Last();

                        var rsaBytes = Encoding.ASCII.GetBytes(rsaPrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(rsaBytes, AES.DecryptString(rsaPrivKey.KeyPass, secret)));

                        var ecdsa256Str = SshHostKeyAlgorithm.ECDsaNistP256.ToString();
                        var ecdsa256PrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey>()
                            .Where(x => x.IdentityId == null && x.KeyAlgo == ecdsa256Str).ToLambda())
                            .OrderBy(x => x.CreatedUtc)
                            .Last();

                        var ecdsa256Bytes = Encoding.ASCII.GetBytes(ecdsa256PrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(ecdsa256Bytes, AES.DecryptString(ecdsa256PrivKey.KeyPass, secret)));

                        var ecdsa384Str = SshHostKeyAlgorithm.ECDsaNistP384.ToString();
                        var ecdsa384PrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey>()
                            .Where(x => x.IdentityId == null && x.KeyAlgo == ecdsa384Str).ToLambda())
                            .OrderBy(x => x.CreatedUtc)
                            .Last();

                        var ecdsa384Bytes = Encoding.ASCII.GetBytes(ecdsa384PrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(ecdsa384Bytes, AES.DecryptString(ecdsa384PrivKey.KeyPass, secret)));

                        var ecdsa521Str = SshHostKeyAlgorithm.ECDsaNistP521.ToString();
                        var ecdsa521PrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey>()
                            .Where(x => x.IdentityId == null && x.KeyAlgo == ecdsa521Str).ToLambda())
                            .OrderBy(x => x.CreatedUtc)
                            .Last();

                        var ecdsa521Bytes = Encoding.ASCII.GetBytes(ecdsa521PrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(ecdsa521Bytes, AES.DecryptString(ecdsa521PrivKey.KeyPass, secret)));

                        var ed25519Str = SshHostKeyAlgorithm.ED25519.ToString();
                        var ed25519PrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey>()
                            .Where(x => x.IdentityId == null && x.KeyAlgo == ed25519Str).ToLambda())
                            .OrderBy(x => x.CreatedUtc)
                            .Last();

                        var ed25519Bytes = Encoding.ASCII.GetBytes(ed25519PrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(ed25519Bytes, AES.DecryptString(ed25519PrivKey.KeyPass, secret)));

                        _binding = conf.GetSection("Daemons:SftpService:Bindings").GetChildren().Select(x => x.Value);
                    }

                    foreach (var binding in _binding)
                    {
                        var pair = binding.Split("|");

                        _server.Bind(new IPEndPoint(IPAddress.Parse(pair[0]), int.Parse(pair[1])), FileServerProtocol.Sftp);
                        _server.Bind(new IPEndPoint(IPAddress.Parse(pair[0]), int.Parse(pair[1])), FileServerProtocol.Shell);
#if !RELEASE
                        _server.Bind(new IPEndPoint(IPAddress.Parse(pair[0]), int.Parse(pair[1])), FileServerProtocol.Tunneling);
#endif
                    }

                    _server.LogWriter = new ConsoleLogWriter(_level);
                    _server.Settings.MaxAuthenticationAttempts = 3;
                    _server.Settings.AllowedAuthenticationMethods = AuthenticationMethods.PublicKey | AuthenticationMethods.Password;
                    _server.Settings.SshParameters.EncryptionAlgorithms = SshEncryptionAlgorithm.Any;
                    _server.Settings.SshParameters.EncryptionModes = SshEncryptionMode.Any;
                    _server.Settings.SshParameters.KeyExchangeAlgorithms = SshKeyExchangeAlgorithm.Any;
                    _server.Settings.SshParameters.HostKeyAlgorithms = SshHostKeyAlgorithm.Any;
                    _server.Settings.SshParameters.MacAlgorithms = SshMacAlgorithm.Any;
                    _server.Connecting += System_Connecting;
                    _server.Disconnected += System_Disconnected;
                    _server.PreAuthentication += User_PreAuthentication;
                    _server.Authentication += User_Authentication;
                    _server.FileDownloaded += User_FileDownloaded;
                    _server.FileUploaded += User_FileUploaded;
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

        private void System_Connecting(object sender, ConnectingEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#connecting
             */

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var networks = uow.Networks.Get(QueryExpressionFactory.GetQueryExpression<Network>()
                        .Where(x => x.IdentityId == null && x.IsEnabled).ToLambda())
                        .OrderBy(x => x.SequenceId);

                    foreach (var network in networks)
                    {
                        var found = NetworkHelper.ValidateAddress(network, e.ClientAddress);

                        if (network.Action == NetworkActionType.Allow.ToString()
                            && found == true)
                        {
                            Log.Information($"'{callPath}' system allow from '{e.ClientEndPoint}'");

                            e.Accept = true;
                            return;
                        }

                        if (network.Action == NetworkActionType.Deny.ToString()
                            && found == true)
                        {
                            Log.Warning($"'{callPath}' system deny from '{e.ClientEndPoint}'");

                            e.Accept = false;
                            return;
                        }
                    }
                }

                Log.Warning($"'{callPath}' system deny (default) from {e.ClientEndPoint}");

                e.Accept = false;
                return;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());

                e.Accept = false;
                return;
            }
        }

        private void System_Disconnected(object sender, DisconnectedEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#disconnected
             */

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                Log.Information($"'{callPath}' from {e.Session.ClientEndPoint} after {e.Session.Duration}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void User_PreAuthentication(object sender, PreAuthenticationEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#pre-authentication
             */

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                        .Where(x => x.IdentityAlias == e.UserName && x.IsEnabled).ToLambda(),
                            new List<Expression<Func<User, object>>>()
                            {
                                x => x.Networks,
                                x => x.PublicKeys,
                            }).SingleOrDefault();

                    if (user == null
                        || user.IsEnabled == false)
                    {
                        Log.Warning($"'{callPath}' '{e.UserName}' not found or not enabled");

                        e.Reject();
                        return;
                    }

                    var networks = uow.Networks.Get(QueryExpressionFactory.GetQueryExpression<Network>()
                        .Where(x => x.IdentityId == user.IdentityId && x.IsEnabled).ToLambda())
                        .OrderBy(x => x.SequenceId);

                    var allowed = false;

                    foreach (var network in networks)
                    {
                        var found = NetworkHelper.ValidateAddress(network, e.ClientAddress);

                        if (network.Action == NetworkActionType.Allow.ToString()
                            && found == true)
                        {
                            Log.Information($"'{callPath}' '{e.UserName}' allowed from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}'");

                            allowed = true;
                            break;
                        }

                        if (network.Action == NetworkActionType.Deny.ToString()
                            && found == true)
                        {
                            Log.Warning($"'{callPath}' '{e.UserName}' is denied from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}'");

                            e.Reject();
                            return;
                        }
                    }

                    if (allowed == false)
                    {
                        Log.Warning($"'{callPath}' '{e.UserName}' is denied (default) from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}'");

                        e.Reject();
                        return;
                    }

                    if (user.IsPublicKeyRequired
                        && user.IsPasswordRequired)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' allowed with public key and password");

                        e.Accept(AuthenticationMethods.PublicKey | AuthenticationMethods.Password);
                        return;
                    }

                    if (user.IsPublicKeyRequired
                        && !user.IsPasswordRequired)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' allowed with public key");

                        e.Accept(AuthenticationMethods.PublicKey);
                        return;
                    }

                    if (!user.IsPublicKeyRequired
                        && user.IsPasswordRequired)
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

        private void User_Authentication(object sender, AuthenticationEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#authentication
             */

            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var log = scope.ServiceProvider.GetRequiredService<ILogger>();
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var admin = scope.ServiceProvider.GetRequiredService<IAdminService>();
                    var sts = scope.ServiceProvider.GetRequiredService<IStsService>();

                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                        .Where(x => x.IdentityAlias == e.UserName && x.IsEnabled).ToLambda(),
                            new List<Expression<Func<User, object>>>()
                            {
                                x => x.Mount,
                                x => x.PublicKeys,
                            }).SingleOrDefault();

                    if (e.Key != null)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' in-progress with public key");

                        if (!UserHelper.ValidatePubKey(user.PublicKeys.Where(x => x.IsEnabled).ToList(), e.Key)
                            || !admin.User_VerifyV1(user.IdentityId).AsTask().Result)
                        {
                            Log.Warning($"'{callPath}' '{e.UserName}' failure with public key");

                            e.Reject();
                            return;
                        }

                        Log.Information($"'{callPath}' '{e.UserName}' success with public key");

                        if (e.PartiallyAccepted
                            || !user.IsPasswordRequired)
                        {
                            /*
                             * an smb mount will not succeed without a user password or ambassador credential.
                             */

                            if (user.FileSystemType == FileSystemProviderType.SMB.ToString()
                                && !user.Mount.CredentialId.HasValue)
                            {
                                Log.Warning($"'{callPath}' '{e.UserName}' failure with no user or ambassador password" +
                                    $" available to mount {FileSystemProviderType.SMB} filesystem");

                                e.Reject();
                                return;
                            }

                            var fs = FileSystemFactory.CreateFileSystem(_factory, log, user, e.UserName, e.Password);

                            var fsNotify = fs.GetFileSystemNotifier();
                            fsNotify.CreatePreview += UserNotify_CreatePreview;
                            fsNotify.CreateCompleted += UserNotify_CreateCompleted;
                            fsNotify.DeletePreview += UserNotify_DeletePreview;
                            fsNotify.DeleteCompleted += UserNotify_DeleteCompleted;

                            var fsUser = new FileServerUser(e.UserName, e.Password, ShellType.Scp);
                            fsUser.SetFileSystem(fs);

                            e.Accept(fsUser);
                            return;
                        }

                        /*
                         * authenticate partially if another kind of credential has not been provided yet.
                         */

                        e.AcceptPartially();
                        return;
                    }

                    if (e.Password != null)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' in-progress with password");

                        try
                        {
                            var identity = admin.User_GetV1(user.IdentityId.ToString()).AsTask().Result;

                            _ = sts.ResourceOwner_GrantV2(
                                new ResourceOwnerV2()
                                {
                                    issuer = conf["IdentityCredential:IssuerName"],
                                    client = conf["IdentityCredential:AudienceName"],
                                    grant_type = "password",
                                    user = identity.UserName,
                                    password = e.Password,
                                }).AsTask().Result;
                        }
                        catch (HttpRequestException ex)
                        {
                            Log.Warning($"'{callPath}' '{e.UserName}' failure with password");

                            Log.Warning($"'{callPath}'" +
                                $"{Environment.NewLine} {ex.Message}" +
                                $"{Environment.NewLine} {ex.InnerException}");

                            e.Reject();
                            return;
                        }

                        Log.Information($"'{callPath}' '{e.UserName}' success with password");

                        if (e.PartiallyAccepted
                            || !user.IsPublicKeyRequired)
                        {
                            var fs = FileSystemFactory.CreateFileSystem(_factory, log, user, e.UserName, e.Password);

                            var fsNotify = fs.GetFileSystemNotifier();
                            fsNotify.CreatePreview += UserNotify_CreatePreview;
                            fsNotify.CreateCompleted += UserNotify_CreateCompleted;
                            fsNotify.DeletePreview += UserNotify_DeletePreview;
                            fsNotify.DeleteCompleted += UserNotify_DeleteCompleted;

                            var fsUser = new FileServerUser(e.UserName, e.Password, ShellType.Scp);
                            fsUser.SetFileSystem(fs);

                            e.Accept(fsUser);
                            return;
                        }

                        /*
                         * authenticate partially if another kind of credential has not been provided yet.
                         */

                        e.AcceptPartially();
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

        private void User_FileDownloaded(object sender, FileTransferredEventArgs e)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var admin = scope.ServiceProvider.GetRequiredService<IAdminService>();
                    var alert = scope.ServiceProvider.GetRequiredService<IAlertService>();

                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                        .Where(x => x.IdentityAlias == ServerSession.Current.UserName).ToLambda())
                        .Single();

                    foreach (var email in uow.UserAlerts.Get(QueryExpressionFactory.GetQueryExpression<UserAlert>()
                        .Where(x => x.IdentityId == user.IdentityId && x.ToEmailAddress != null && x.IsEnabled == true && x.OnDownload == true).ToLambda()))
                    {
                        _ = alert.Enqueue_EmailV1(new EmailV1()
                        {
                            FromEmail = conf["Notifications:EmailFromAddress"],
                            FromDisplay = conf["Notifications:EmailFromDisplayName"],
                            ToEmail = email.ToEmailAddress,
                            ToDisplay = $"{email.ToFirstName} {email.ToLastName}",
                            Subject = "File Download Alert",
                            Body = Templates.NotifyEmailOnFileDownload(conf["Daemons:SftpService:Dns"], ServerSession.Current.UserName,
                                email.ToFirstName, email.ToLastName, "/" + e.FullPath, e.BytesTransferred.ToString(), e.Session.ClientEndPoint.ToString())
                        }).AsTask().Result;
                    }

                    foreach (var text in uow.UserAlerts.Get(QueryExpressionFactory.GetQueryExpression<UserAlert>()
                        .Where(x => x.IdentityId == user.IdentityId && x.ToPhoneNumber != null && x.IsEnabled == true && x.OnDownload == true).ToLambda()))
                    {
                        _ = alert.Enqueue_TextV1(new TextV1()
                        {
                            FromPhoneNumber = conf["Notifications:TextFromPhoneNumber"],
                            ToPhoneNumber = text.ToPhoneNumber,
                            Body = Templates.NotifyTextOnFileDownload(conf["Daemons:SftpService:Dns"], ServerSession.Current.UserName,
                                text.ToFirstName, text.ToLastName, "/" + e.FullPath, e.BytesTransferred.ToString(), e.Session.ClientEndPoint.ToString())
                        }).AsTask().Result;
                    }
                }

                Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file '/{e.FullPath}' bytes {e.BytesTransferred} from {e.Session.ClientEndPoint}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void User_FileUploaded(object sender, FileTransferredEventArgs e)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var admin = scope.ServiceProvider.GetRequiredService<IAdminService>();
                    var alert = scope.ServiceProvider.GetRequiredService<IAlertService>();

                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                        .Where(x => x.IdentityAlias == ServerSession.Current.UserName).ToLambda())
                        .Single();

                    foreach (var email in uow.UserAlerts.Get(QueryExpressionFactory.GetQueryExpression<UserAlert>()
                        .Where(x => x.IdentityId == user.IdentityId && x.ToEmailAddress != null && x.IsEnabled == true && x.OnUpload == true).ToLambda()))
                    {
                        _ = alert.Enqueue_EmailV1(new EmailV1()
                        {
                            FromEmail = conf["Notifications:EmailFromAddress"],
                            FromDisplay = conf["Notifications:EmailFromDisplayName"],
                            ToEmail = email.ToEmailAddress,
                            ToDisplay = $"{email.ToFirstName} {email.ToLastName}",
                            Subject = "File Upload Alert",
                            Body = Templates.NotifyEmailOnFileUpload(conf["Daemons:SftpService:Dns"], ServerSession.Current.UserName,
                                email.ToFirstName, email.ToLastName, "/" + e.FullPath, e.BytesTransferred.ToString(), e.Session.ClientEndPoint.ToString())
                        }).AsTask().Result;
                    }

                    foreach (var text in uow.UserAlerts.Get(QueryExpressionFactory.GetQueryExpression<UserAlert>()
                            .Where(x => x.IdentityId == user.IdentityId && x.ToPhoneNumber != null && x.IsEnabled == true && x.OnUpload == true).ToLambda()))
                    {
                        _ = alert.Enqueue_TextV1(new TextV1()
                        {
                            FromPhoneNumber = conf["Notifications:TextFromPhoneNumber"],
                            ToPhoneNumber = text.ToPhoneNumber,
                            Body = Templates.NotifyTextOnFileUpload(conf["Daemons:SftpService:Dns"], ServerSession.Current.UserName,
                                text.ToFirstName, text.ToLastName, "/" + e.FullPath, e.BytesTransferred.ToString(), e.Session.ClientEndPoint.ToString())
                        }).AsTask().Result;
                    }
                }

                Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file '/{e.FullPath}' bytes {e.BytesTransferred} from {e.Session.ClientEndPoint}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void UserNotify_CreatePreview(object sender, PreviewSingleNodeOperationEventArgs e)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {

            }
            catch (Exception ex)
            {
                e.CancelOperation();

                Log.Error(ex.ToString());
                throw;
            }
        }

        private void UserNotify_CreateCompleted(object sender, SingleNodeOperationEventArgs e)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {

            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void UserNotify_DeletePreview(object sender, PreviewSingleNodeOperationEventArgs e)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {

            }
            catch (Exception ex)
            {
                e.CancelOperation();

                Log.Error(ex.ToString());
                throw;
            }
        }

        private void UserNotify_DeleteCompleted(object sender, SingleNodeOperationEventArgs e)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                if (e.ResultNode.IsFile)
                {
                    using (var scope = _factory.CreateScope())
                    {
                        var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var admin = scope.ServiceProvider.GetRequiredService<IAdminService>();
                        var alert = scope.ServiceProvider.GetRequiredService<IAlertService>();

                        var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                            .Where(x => x.IdentityAlias == ServerSession.Current.UserName).ToLambda())
                            .Single();

                        foreach (var email in uow.UserAlerts.Get(QueryExpressionFactory.GetQueryExpression<UserAlert>()
                            .Where(x => x.IdentityId == user.IdentityId && x.ToEmailAddress != null && x.IsEnabled == true && x.OnDelete == true).ToLambda()))
                        {
                            _ = alert.Enqueue_EmailV1(new EmailV1()
                            {
                                FromEmail = conf["Notifications:EmailFromAddress"],
                                FromDisplay = conf["Notifications:EmailFromDisplayName"],
                                ToEmail = email.ToEmailAddress,
                                ToDisplay = $"{email.ToFirstName} {email.ToLastName}",
                                Subject = "File Delete Alert",
                                Body = Templates.NotifyEmailOnFileDelete(conf["Daemons:SftpService:Dns"], ServerSession.Current.UserName,
                                    email.ToFirstName, email.ToLastName, e.ResultNode.Path.StringPath)
                            }).AsTask().Result;
                        }

                        foreach (var text in uow.UserAlerts.Get(QueryExpressionFactory.GetQueryExpression<UserAlert>()
                            .Where(x => x.IdentityId == user.IdentityId && x.ToPhoneNumber != null && x.IsEnabled == true && x.OnDelete == true).ToLambda()))
                        {
                            _ = alert.Enqueue_TextV1(new TextV1()
                            {
                                FromPhoneNumber = conf["Notifications:TextFromPhoneNumber"],
                                ToPhoneNumber = text.ToPhoneNumber,
                                Body = Templates.NotifyTextOnFileDelete(conf["Daemons:SftpService:Dns"], ServerSession.Current.UserName,
                                    text.ToFirstName, text.ToLastName, e.ResultNode.Path.StringPath)
                            }).AsTask().Result;
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

        public void Dispose()
        {
            _server.Dispose();
        }
    }
}
