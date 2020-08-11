using Bhbk.Daemon.Aurora.SFTP.Factories;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Aurora.Domain.Primitives;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.Identity.Models.Alert;
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
using Rebex.Security.Cryptography;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/*
 * https://www.rebex.net/file-server/features/easy-to-use-api.aspx
 */

namespace Bhbk.Daemon.Aurora.SFTP
{
    public class SftpService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _factory;
        private readonly FileServer _server;
        private IEnumerable<string> _binding;
        private LogLevel _level;

        public SftpService(IServiceScopeFactory factory)
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

                        _binding = conf.GetSection("Daemons:SftpService:Bindings").GetChildren().Select(x => x.Value);

                        /*
                         * https://www.rebex.net/support/trial/
                         */
                        var key = uow.SysSettings.Get(x => x.ConfigKey == "RebexLicense")
                            .OrderBy(x => x.Created).Last();

                        Rebex.Licensing.Key = key.ConfigValue;

                        /*
                        * https://en.wikipedia.org/wiki/Digital_Signature_Algorithm
                        */
                        var dsaKey = uow.SysPrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_SysPrivateKeys>()
                            .Where(x => x.KeyValueAlgo == SshHostKeyAlgorithm.DSS.ToString()).ToLambda())
                            .OrderBy(x => x.Created).LastOrDefault();

                        if (dsaKey == null)
                        {
                            dsaKey = KeyHelper.GenerateSshPrivateKey(uow,
                                SshHostKeyAlgorithm.DSS, 1024, AlphaNumeric.CreateString(32), SshPrivateKeyFormat.Pkcs8);

                            uow.Commit();
                        }

                        var dsaBytes = Encoding.ASCII.GetBytes(dsaKey.KeyValueBase64);
                        _server.Keys.Add(new SshPrivateKey(dsaBytes, dsaKey.KeyValuePass));

                        /*
                         * https://en.wikipedia.org/wiki/RSA_(cryptosystem)
                         */
                        var rsaKey = uow.SysPrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_SysPrivateKeys>()
                            .Where(x => x.KeyValueAlgo == SshHostKeyAlgorithm.RSA.ToString()).ToLambda())
                            .OrderBy(x => x.Created).LastOrDefault();

                        if (rsaKey == null)
                        {
                            rsaKey = KeyHelper.GenerateSshPrivateKey(uow,
                                SshHostKeyAlgorithm.RSA, 2048, AlphaNumeric.CreateString(32), SshPrivateKeyFormat.Pkcs8);

                            uow.Commit();
                        }

                        var rsaBytes = Encoding.ASCII.GetBytes(rsaKey.KeyValueBase64);
                        _server.Keys.Add(new SshPrivateKey(rsaBytes, rsaKey.KeyValuePass));

                        /*
                         * https://en.wikipedia.org/wiki/Elliptic_Curve_Digital_Signature_Algorithm
                         */
                        var ecdsaKey = uow.SysPrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_SysPrivateKeys>()
                            .Where(x => x.KeyValueAlgo == SshHostKeyAlgorithm.ECDsaNistP521.ToString()).ToLambda())
                            .OrderBy(x => x.Created).LastOrDefault();

                        if (ecdsaKey == null)
                        {
                            ecdsaKey = KeyHelper.GenerateSshPrivateKey(uow,
                                SshHostKeyAlgorithm.ECDsaNistP521, 521, AlphaNumeric.CreateString(32), SshPrivateKeyFormat.Pkcs8);

                            uow.Commit();
                        }

                        var ecdsaBytes = Encoding.ASCII.GetBytes(ecdsaKey.KeyValueBase64);
                        _server.Keys.Add(new SshPrivateKey(ecdsaBytes, ecdsaKey.KeyValuePass));

                        /*
                         * https://en.wikipedia.org/wiki/Curve25519
                         */
                        var ed25519Key = uow.SysPrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_SysPrivateKeys>()
                            .Where(x => x.KeyValueAlgo == SshHostKeyAlgorithm.ED25519.ToString()).ToLambda())
                            .OrderBy(x => x.Created).LastOrDefault();

                        if (ed25519Key == null)
                        {
                            ed25519Key = KeyHelper.GenerateSshPrivateKey(uow,
                                SshHostKeyAlgorithm.ED25519, 256, AlphaNumeric.CreateString(32), SshPrivateKeyFormat.Pkcs8);

                            uow.Commit();
                        }

                        var ed25519Bytes = Encoding.ASCII.GetBytes(ed25519Key.KeyValueBase64);
                        _server.Keys.Add(new SshPrivateKey(ed25519Bytes, ed25519Key.KeyValuePass));
                    }

                    _server.LogWriter = new ConsoleLogWriter(_level);
                    _server.Settings.AllowedAuthenticationMethods = AuthenticationMethods.PublicKey | AuthenticationMethods.Password;
                    _server.Authentication += FsUser_Authentication;
                    _server.Connecting += FsUser_Connecting;
                    _server.Disconnected += FsUser_Disconnected;
                    _server.FileDownloaded += FsUser_FileDownloaded;
                    _server.FileUploaded += FsUser_FileUploaded;
                    _server.PathAccessAuthorization += FsUser_PathAccessAuthorization;
                    _server.PreAuthentication += FsUser_PreAuthentication;
                    _server.ShellCommand += FsUser_ShellCommand;

                    foreach (var binding in _binding)
                    {
                        var pair = binding.Split("|");

                        _server.Bind(new IPEndPoint(IPAddress.Parse(pair[0]), int.Parse(pair[1])), FileServerProtocol.Sftp);
#if DEBUG
                        _server.Bind(new IPEndPoint(IPAddress.Parse(pair[0]), int.Parse(pair[1])), FileServerProtocol.Shell);
#endif
                    }

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

        private void FsNotify_GetContentSurrogate(object sender, GetContentEventArgs e)
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

        private void FsUser_Authentication(object sender, AuthenticationEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#authentication
             */
            try
            {
                var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

                if (e.Key != null)
                {
                    Log.Information($"'{callPath}' '{e.UserName}' in-progress from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with public key");

                    using (var scope = _factory.CreateScope())
                    {
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                            .Where(x => x.UserName == e.UserName).ToLambda(),
                                new List<Expression<Func<tbl_Users, object>>>()
                                {
                                    x => x.tbl_UserFolders,
                                    x => x.tbl_UserFiles,
                                    x => x.tbl_UserPublicKeys,
                                }).SingleOrDefault();

                        var keys = user.tbl_UserPublicKeys.Where(x => x.Enabled);

                        if (keys.Where(x => x.KeyValueBase64 == Convert.ToBase64String(e.Key.GetPublicKey(), Base64FormattingOptions.None)).Any())
                        {
                            Log.Information($"'{callPath}' '{e.UserName}' success from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with public key");

                            var fs = FileSystemFactory.CreateFileSystem(_factory, user, logger);
                            var fsUser = new FileServerUser(e.UserName, e.Password);
                            fsUser.SetFileSystem(fs);

                            var fsNotify = fs.GetFileSystemNotifier();
                            fsNotify.GetContentSurrogate += FsNotify_GetContentSurrogate;
                            fsNotify.CreatePreview += FsNotify_CreatePreview;
                            fsNotify.CreateCompleted += FsNotify_CreateCompleted;
                            fsNotify.DeletePreview += FsNotify_DeletePreview;
                            fsNotify.DeleteCompleted += FsNotify_DeleteCompleted;

                            e.Accept(fsUser);
                            return;
                        }
                        else
                        {
                            Log.Error($"'{callPath}' '{e.UserName}' failure from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with public key");

                            e.Reject();
                            return;
                        }
                    }
                }
                else if (e.Password != null)
                {
                    Log.Information($"'{callPath}' '{e.UserName}' in-progress from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with password");

                    using (var scope = _factory.CreateScope())
                    {
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                            .Where(x => x.UserName == e.UserName).ToLambda(),
                                new List<Expression<Func<tbl_Users, object>>>()
                                {
                                    x => x.tbl_UserFolders,
                                    x => x.tbl_UserFiles,
                                    x => x.tbl_UserPasswords,
                                }).SingleOrDefault();

                        var pass = user.tbl_UserPasswords;

                        if (PBKDF2.Validate(pass.HashPBKDF2, e.Password))
                        {
                            Log.Information($"'{callPath}' '{e.UserName}' success from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with password");

                            var fs = FileSystemFactory.CreateFileSystem(_factory, user, logger);
                            var fsUser = new FileServerUser(e.UserName, e.Password);
                            fsUser.SetFileSystem(fs);

                            var fsNotify = fs.GetFileSystemNotifier();
                            fsNotify.GetContentSurrogate += FsNotify_GetContentSurrogate;
                            fsNotify.CreatePreview += FsNotify_CreatePreview;
                            fsNotify.CreateCompleted += FsNotify_CreateCompleted;
                            fsNotify.DeletePreview += FsNotify_DeletePreview;
                            fsNotify.DeleteCompleted += FsNotify_DeleteCompleted;

                            e.Accept(fsUser);
                            return;
                        }
                        else
                        {
                            Log.Error($"'{callPath}' '{e.UserName}' failure from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with password");

                            e.Reject();
                            return;
                        }
                    }
                }

                Log.Error($"'{callPath}' '{e.UserName}' not possible from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}'");

                e.Reject();
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

        private void FsUser_Disconnected(object sender, DisconnectedEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#disconnected
             */
            try
            {
                var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

                Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' from {e.Session.ClientEndPoint} after {e.Session.Duration}");
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

                using (var scope = _factory.CreateScope())
                {
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                        .Where(x => x.UserName == ServerSession.Current.UserName).ToLambda()).Single();

                    if (user.IdentityId.HasValue)
                    {
                        var admin = scope.ServiceProvider.GetRequiredService<IAdminService>();
                        var result = admin.User_GetV1(user.IdentityId.Value.ToString()).Result;

                        var alert = scope.ServiceProvider.GetRequiredService<IAlertService>();

                        alert.Email_EnqueueV1(new EmailV1()
                        {
                            FromEmail = conf["Notifications:SmtpSenderAddress"],
                            FromDisplay = conf["Notifications:SmtpSenderDisplayName"],
                            ToId = result.Id,
                            ToEmail = result.Email,
                            ToDisplay = $"{result.FirstName} {result.LastName}",
                            Subject = "File Upload Notify",
                            HtmlContent = Templates.NotifyEmailOnFileUpload(conf["Daemons:SftpService:Dns"], result.UserName, result.FirstName, result.LastName,
                                e.FullPath, e.BytesTransferred.ToString())
                        });

                        alert.Text_EnqueueV1(new TextV1()
                        {
                            FromPhoneNumber = conf["Notifications:SmsSenderNumber"],
                            ToId = result.Id,
                            ToPhoneNumber = result.PhoneNumber,
                            Body = conf["Notifications:SmsSenderDisplayName"] + Environment.NewLine
                                + Templates.NotifyTextOnFileUpload(conf["Daemons:SftpService:Dns"], result.UserName, e.FullPath, e.BytesTransferred.ToString())
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void FsUser_PathAccessAuthorization(object sender, PathAccessAuthorizationEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#path-access-authorization
             */
            try
            {
                e.Allow();
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
                                x => x.tbl_UserPasswords,
                                x => x.tbl_UserPublicKeys
                            }).SingleOrDefault();

                    if (user == null
                        || user.Enabled == false)
                    {
                        e.Reject();
                        return;
                    }

                    var keys = user.tbl_UserPublicKeys.Where(x => x.Enabled);
                    var pass = user.tbl_UserPasswords;

                    if (pass != null
                        && pass.Enabled
                        && keys.Count() > 0)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' allowed from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with password or public key");

                        e.Accept(AuthenticationMethods.PublicKey | AuthenticationMethods.Password);
                        return;
                    }

                    if (pass == null
                        && keys.Count() > 0)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' allowed from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with public key");

                        e.Accept(AuthenticationMethods.PublicKey);
                        return;
                    }

                    if (pass != null
                        && pass.Enabled
                        && keys.Count() == 0)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' allowed from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with password");

                        e.Accept(AuthenticationMethods.Password);
                        return;
                    }

                    Log.Error($"'{callPath}' '{e.UserName}' not allowed from '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}'");

                    e.Reject();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void FsUser_ShellCommand(object sender, ShellCommandEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#shell-command
             */
            try
            {

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