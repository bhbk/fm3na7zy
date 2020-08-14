using Bhbk.Daemon.Aurora.SFTP.Factories;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Aurora.Domain.Primitives;
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

                        var license = uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<tbl_Settings>()
                            .Where(x => x.ConfigKey == "RebexLicense").ToLambda()).OrderBy(x => x.Created)
                            .Last();

                        Rebex.Licensing.Key = license.ConfigValue;

                        KeyHelper.CheckDaemonSshPrivKey(uow, SshHostKeyAlgorithm.DSS, 1024, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
                        KeyHelper.CheckDaemonSshPrivKey(uow, SshHostKeyAlgorithm.RSA, 2048, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
                        KeyHelper.CheckDaemonSshPrivKey(uow, SshHostKeyAlgorithm.ECDsaNistP521, 521, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);
                        KeyHelper.CheckDaemonSshPrivKey(uow, SshHostKeyAlgorithm.ED25519, 256, AlphaNumeric.CreateString(32), SignatureHashAlgorithm.SHA256);

                        var dsaStr = SshHostKeyAlgorithm.DSS.ToString();
                        var dsaPrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKeys>()
                            .Where(x => x.KeyAlgo == dsaStr && x.UserId == null).ToLambda()).OrderBy(x => x.Created)
                            .Single();

                        var dsaBytes = Encoding.ASCII.GetBytes(dsaPrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(dsaBytes, dsaPrivKey.KeyPass));

                        var rsaStr = SshHostKeyAlgorithm.RSA.ToString();
                        var rsaPrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKeys>()
                            .Where(x => x.KeyAlgo == rsaStr && x.UserId == null).ToLambda()).OrderBy(x => x.Created)
                            .Single();

                        var rsaBytes = Encoding.ASCII.GetBytes(rsaPrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(rsaBytes, rsaPrivKey.KeyPass));

                        var ecdsaStr = SshHostKeyAlgorithm.ECDsaNistP521.ToString();
                        var ecdsaPrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKeys>()
                            .Where(x => x.KeyAlgo == ecdsaStr && x.UserId == null).ToLambda()).OrderBy(x => x.Created)
                            .Single();

                        var ecdsaBytes = Encoding.ASCII.GetBytes(ecdsaPrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(ecdsaBytes, ecdsaPrivKey.KeyPass));

                        var ed25519Str = SshHostKeyAlgorithm.ED25519.ToString();
                        var ed25519PrivKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_PrivateKeys>()
                            .Where(x => x.KeyAlgo == ed25519Str && x.UserId == null).ToLambda()).OrderBy(x => x.Created)
                            .Single();

                        var ed25519Bytes = Encoding.ASCII.GetBytes(ed25519PrivKey.KeyValue);
                        _server.Keys.Add(new SshPrivateKey(ed25519Bytes, ed25519PrivKey.KeyPass));

                        _binding = conf.GetSection("Daemons:SftpService:Bindings").GetChildren().Select(x => x.Value);
                    }

                    foreach (var binding in _binding)
                    {
                        var pair = binding.Split("|");

                        _server.Bind(new IPEndPoint(IPAddress.Parse(pair[0]), int.Parse(pair[1])), FileServerProtocol.Sftp);
#if DEBUG
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

                    using (var scope = _factory.CreateScope())
                    {
                        var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                            .Where(x => x.UserName == ServerSession.Current.UserName).ToLambda())
                            .Single();

                        var admin = scope.ServiceProvider.GetRequiredService<IAdminService>();
                        var alert = scope.ServiceProvider.GetRequiredService<IAlertService>();

                        var identity = admin.User_GetV1(user.Id.ToString()).Result;

                        alert.Email_EnqueueV1(new EmailV1()
                        {
                            FromEmail = conf["Notifications:SmtpSenderAddress"],
                            FromDisplay = conf["Notifications:SmtpSenderDisplayName"],
                            ToId = identity.Id,
                            ToEmail = identity.Email,
                            ToDisplay = $"{identity.FirstName} {identity.LastName}",
                            Subject = "File Upload Notify",
                            HtmlContent = Templates.NotifyEmailOnFileUpload(conf["Daemons:SftpService:Dns"],
                                identity.UserName, identity.FirstName, identity.LastName, e.ResultNode.Path.StringPath, e.ResultNode.Length.ToString())
                        });

                        alert.Text_EnqueueV1(new TextV1()
                        {
                            FromPhoneNumber = conf["Notifications:SmsSenderNumber"],
                            ToId = identity.Id,
                            ToPhoneNumber = identity.PhoneNumber,
                            Body = conf["Notifications:SmsSenderDisplayName"] + Environment.NewLine
                                + Templates.NotifyTextOnFileUpload(conf["Daemons:SftpService:Dns"], identity.UserName,
                                e.ResultNode.Path.StringPath, e.ResultNode.Length.ToString())
                        });
                    }
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
                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                        .Where(x => x.UserName == e.UserName && x.Enabled).ToLambda(),
                            new List<Expression<Func<tbl_Users, object>>>()
                            {
                                x => x.tbl_Networks,
                                x => x.tbl_PublicKeys,
                            }).SingleOrDefault();

                    if (NetworkHelper.ValidateAddress(user.tbl_Networks.Where(x => x.Action == "Deny" && x.Enabled), e.ClientAddress))
                    {
                        Log.Warning($"'{callPath}' '{e.UserName}' is denied from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}'");

                        e.Reject();
                        return;
                    }

                    if (!NetworkHelper.ValidateAddress(user.tbl_Networks.Where(x => x.Action == "Allow" && x.Enabled), e.ClientAddress))
                    {
                        Log.Warning($"'{callPath}' '{e.UserName}' is not allowed from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}'");

                        e.Reject();
                        return;
                    }

                    Log.Information($"'{callPath}' '{e.UserName}' allowed from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}'");

                    if (user == null
                        || user.Enabled == false)
                    {
                        e.Reject();
                        return;
                    }

                    var keys = user.tbl_PublicKeys.Where(x => x.Enabled);

                    if (!user.AllowPassword)
                    {
                        if (keys.Count() > 0)
                        {
                            Log.Information($"'{callPath}' '{e.UserName}' allowed with public key");

                            e.Accept(AuthenticationMethods.PublicKey);
                            return;
                        }
                    }

                    if (user.AllowPassword)
                    {
                        if (keys.Count() > 0)
                        {
                            Log.Information($"'{callPath}' '{e.UserName}' allowed with public key or password");

                            e.Accept(AuthenticationMethods.PublicKey | AuthenticationMethods.Password);
                            return;
                        }
                        else
                        {
                            Log.Information($"'{callPath}' '{e.UserName}' allowed with password");

                            e.Accept(AuthenticationMethods.Password);
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
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var log = scope.ServiceProvider.GetRequiredService<ILogger>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                        .Where(x => x.UserName == e.UserName).ToLambda(),
                            new List<Expression<Func<tbl_Users, object>>>()
                            {
                                    x => x.tbl_PublicKeys,
                            }).SingleOrDefault();

                    var admin = scope.ServiceProvider.GetRequiredService<IAdminService>();
                    var sts = scope.ServiceProvider.GetRequiredService<IStsService>();

                    if (e.Key != null)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' in-progress with public key");

                        if (UserHelper.ValidatePubKey(user.tbl_PublicKeys.Where(x => x.Enabled).ToList(), e.Key))
                        {
                            if (!admin.User_VerifyV1(user.Id).Result)
                            {
                                Log.Warning($"'{callPath}' '{e.UserName}' failure with public key");

                                e.Reject();
                                return;
                            }

                            Log.Information($"'{callPath}' '{e.UserName}' success with public key");

                            var fs = FileSystemFactory.CreateFileSystem(_factory, log, user, e.Password);
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

                        Log.Warning($"'{callPath}' '{e.UserName}' failure with public key");

                        e.Reject();
                        return;
                    }

                    if (e.Password != null)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' in-progress with password");

                        try
                        {
                            var identity = admin.User_GetV1(user.Id.ToString()).Result;

                            var auth = sts.ResourceOwner_GrantV2(
                                new ResourceOwnerV2()
                                {
                                    issuer = conf["IdentityCredentials:IssuerName"],
                                    client = conf["IdentityCredentials:AudienceName"],
                                    grant_type = "password",
                                    user = identity.UserName,
                                    password = e.Password,
                                }).Result;
                        }
                        catch (HttpRequestException)
                        {
                            Log.Warning($"'{callPath}' '{e.UserName}' failure with password");

                            e.Reject();
                            return;
                        }

                        Log.Information($"'{callPath}' '{e.UserName}' success with password");

                        var fs = FileSystemFactory.CreateFileSystem(_factory, log, user, e.Password);
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
                }

                Log.Warning($"'{callPath}' '{e.UserName}' denied");

                e.Reject();
                return;
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

                Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file '/{e.FullPath}' bytes {e.BytesTransferred} to {e.Session.ClientEndPoint}");
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

        public void Dispose()
        {
            _server.Dispose();
        }
    }
}
