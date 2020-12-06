using Bhbk.Daemon.Aurora.SFTP.Factories;
using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.Aurora.Domain.Templates;
using Bhbk.Lib.Aurora.Primitives;
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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
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
        private LogLevel _level;
        private readonly IServiceScopeFactory _factory;
        private readonly FileServer _server;
        private readonly IPAddress _ip;
        private readonly string _host;
        private string _localEndPoint;
        private IEnumerable<string> _bindingAddresses;
        private int _bindingPort;

        public Daemon(IServiceScopeFactory factory)
        {
            _factory = factory;
            _server = new FileServer();
            _ip = NetworkHelper.GetIPAddresses(Dns.GetHostName())
                .First();
            _host = Dns.GetHostName();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var callPath = "Daemon.StartAsync"; //reflection does not resolve callpath in async...

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

                        var keyType = ConfigType.RebexLicense.ToString();

                        var license = uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<Setting>()
                            .Where(x => x.ConfigKey == keyType).ToLambda())
                            .OrderBy(x => x.CreatedUtc)
                            .Last();

                        Rebex.Licensing.Key = license.ConfigValue;

                        /*
                         * does daemon have public/private key pair(s) needed for clients to connect... create them if not...
                         */

                        System_CheckKeyPair(conf, uow, SshHostKeyAlgorithm.DSS, SignatureHashAlgorithm.SHA256, 1024);
                        System_CheckKeyPair(conf, uow, SshHostKeyAlgorithm.RSA, SignatureHashAlgorithm.SHA256, 4096);
                        System_CheckKeyPair(conf, uow, SshHostKeyAlgorithm.ECDsaNistP256, SignatureHashAlgorithm.SHA256, 256);
                        System_CheckKeyPair(conf, uow, SshHostKeyAlgorithm.ECDsaNistP384, SignatureHashAlgorithm.SHA256, 384);
                        System_CheckKeyPair(conf, uow, SshHostKeyAlgorithm.ECDsaNistP521, SignatureHashAlgorithm.SHA256, 521);
                        System_CheckKeyPair(conf, uow, SshHostKeyAlgorithm.ED25519, SignatureHashAlgorithm.SHA256, 256);

                        var secret = conf["Databases:AuroraSecret"];

                        var hostKeyAlgos = conf["Daemons:Sftp:HostKeyAlgorithms"].Split(',')
                            .Select(x => x.Trim());

                        foreach (var hostKeyAlgo in hostKeyAlgos)
                        {
                            SshHostKeyAlgorithm hostKeyAlgosSupported;

                            if (Enum.TryParse(hostKeyAlgo, out hostKeyAlgosSupported))
                            {
                                var privKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey>()
                                    .Where(x => x.IdentityId == null && x.IsEnabled == true && x.KeyAlgo == hostKeyAlgo).ToLambda())
                                    .OrderBy(x => x.CreatedUtc)
                                    .LastOrDefault();

                                if (privKey != null)
                                {
                                    var keyBytes = Encoding.UTF8.GetBytes(privKey.KeyValue);
                                    _server.Keys.Add(new SshPrivateKey(keyBytes, AES.DecryptString(privKey.KeyPass, secret)));

                                    Log.Information($"'{callPath}' 'system' loading public/private key pair [algo] {privKey.KeyAlgo} [format] {privKey.KeyFormat}" +
                                        $"{Environment.NewLine} [public key GUID]{privKey.PublicKeyId} [private key GUID] {privKey.Id}" +
                                        $"{Environment.NewLine}");
                                }
                                else
                                    Log.Error($"'{callPath}' 'system' public/private key pair [algo] {hostKeyAlgo} not found");
                            }
                            else
                                Log.Error($"'{callPath}' 'system' public/private key pair [algo] {hostKeyAlgo} not supported");
                        }

                        var hostAddresses = new StringBuilder();
                        hostAddresses.Append($"'{callPath}' 'system' [hostname] '{_host.ToUpper()}' has address(es) ");

                        foreach (var ip in NetworkHelper.GetIPAddresses(_host))
                            hostAddresses.Append($"{ip} ");

                        Log.Information($"{hostAddresses}");

                        _bindingAddresses = conf.GetSection("Daemons:Sftp:BindingAddresses").GetChildren()
                            .Select(x => x.Value);

                        _bindingPort = Int32.Parse(conf.GetSection("Daemons:Sftp:BindingPorts").GetChildren()
                            .Select(x => x.Value).First());

                        _localEndPoint = $"{_ip}:{_bindingPort}";

                        /*
                         * clear out orphan session entries from un-graceful shutdown of daemon...
                         */

                        var entries = uow.Sessions.Get(QueryExpressionFactory.GetQueryExpression<Session>()
                            .Where(x => x.LocalEndPoint == _localEndPoint && x.IsActive == true).ToLambda());

                        foreach (var entry in entries)
                            entry.IsActive = false;

                        uow.Sessions.Update(entries);
                        uow.Commit();

                        /*
                         * daemon can bind to multiple ip addresses and ports...
                         */

                        foreach (var bindingAddress in _bindingAddresses)
                        {
                            _server.Bind(new IPEndPoint(IPAddress.Parse(bindingAddress), _bindingPort), FileServerProtocol.Sftp);
                            _server.Bind(new IPEndPoint(IPAddress.Parse(bindingAddress), _bindingPort), FileServerProtocol.Shell);

                            Log.Information($"'{callPath}' 'system' [ip-binding] {bindingAddress} [port-binding] {_bindingPort}");
                        }

                        /*
                         * daemon needs to have attitude adjusted to needs/wants...
                         */

                        _server.LogWriter = new ConsoleLogWriter(_level);
                        _server.Settings.MaxAuthenticationAttempts = Int32.Parse(conf["Daemons:Sftp:MaxAuthAttempts"]);
                        _server.Settings.AllowedAuthenticationMethods = AuthenticationMethods.PublicKey | AuthenticationMethods.Password;
                        _server.Settings.SshParameters.EncryptionAlgorithms = SshEncryptionAlgorithm.Any;
                        _server.Settings.SshParameters.EncryptionModes = SshEncryptionMode.Any;
                        _server.Settings.SshParameters.KeyExchangeAlgorithms = SshKeyExchangeAlgorithm.Any;
                        _server.Settings.SshParameters.HostKeyAlgorithms = SshHostKeyAlgorithm.Any;
                        _server.Settings.SshParameters.MacAlgorithms = SshMacAlgorithm.Any;

                        /*
                         * daemon hooks certain events that are needed...
                         */

                        _server.Connecting += System_Connect;
                        _server.PreAuthentication += System_PreAuthentication;
                        _server.Authentication += System_Authentication;
                        _server.Disconnected += System_Disconnect;
                        _server.FileDownloaded += System_FileDownloaded;
                        _server.FileUploaded += System_FileUploaded;
                        _server.Start();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }, cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            /*
             * method only called when polite shutdown request sent...
             * examples are ctrl-c at console or "shutdown" from services mmc.
             */

            var callPath = "Daemon.StopAsync"; //reflection does not resolve callpath in async...

            await Task.Run(() =>
            {
                try
                {
                    if (_server.IsRunning)
                    {
                        using (var scope = _factory.CreateScope())
                        {
                            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                            foreach (var session in _server.Sessions)
                            {
                                var remote = session.ClientEndPoint.ToString();

                                var entries = uow.Sessions.Get(QueryExpressionFactory.GetQueryExpression<Session>()
                                    .Where(x => x.LocalEndPoint == _localEndPoint && x.IsActive == true).ToLambda());

                                foreach (var entry in entries)
                                    entry.IsActive = false;

                                uow.Sessions.Update(entries);
                                uow.Commit();

                                Log.Warning($"'{callPath}' '{session.UserName}' client:'{session.ClientEndPoint}' " +
                                    $"duration:'{session.Duration}' force disconnect, server restart");

                                session.SendMessage($"'{session.UserName}' client:'{session.ClientEndPoint}' " +
                                    $"duration:'{session.Duration}' force disconnect, server restart");
                            }
                        }

                        _server.Unbind();
                        _server.Stop();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }, cancellationToken);
        }

        private void System_CheckKeyPair(IConfiguration conf, IUnitOfWork uow,
            SshHostKeyAlgorithm keyAlgo, SignatureHashAlgorithm sigAlgo, int keySize)
        {
            var keyAlgoStr = keyAlgo.ToString();
            var privKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<PrivateKey>()
                .Where(x => x.IdentityId == null && x.IsEnabled == true && x.KeyAlgo == keyAlgoStr).ToLambda())
                .OrderBy(x => x.CreatedUtc)
                .LastOrDefault();

            if (privKey == null)
            {
                var keyPair = KeyHelper.CreateKeyPair(conf, uow, keyAlgo, sigAlgo, keySize, AlphaNumeric.CreateString(32));

                if (keyPair.Item1 != null)
                {
                    keyPair.Item1.IsEnabled = true;

                    uow.PublicKeys.Create(keyPair.Item1);
                    uow.Commit();
                }

                if (keyPair.Item2 != null)
                {
                    keyPair.Item2.IsEnabled = true;

                    uow.PrivateKeys.Create(keyPair.Item2);
                    uow.Commit();
                }
            }
        }

        private void System_Connect(object sender, ConnectingEventArgs e)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var networks = uow.Networks.Get(QueryExpressionFactory.GetQueryExpression<Network>()
                        .Where(x => x.IdentityId == null && x.IsEnabled).ToLambda())
                        .OrderBy(x => x.SequenceId);

                    /*
                     * does system have network filtering rule to allow session... this is global...
                     */

                    foreach (var network in networks)
                    {
                        var found = NetworkHelper.ValidateAddress(network, e.ClientAddress);

                        if (network.Action == NetworkActionType.Allow.ToString()
                            && found == true)
                        {
                            uow.Sessions.Create(
                                new Session
                                {
                                    CallPath = callPath,
                                    Details = "allowed",
                                    LocalEndPoint = _localEndPoint,
                                    RemoteEndPoint = e.ClientEndPoint.ToString(),
                                    IsActive = true,
                                });

                            uow.Commit();

                            Log.Information($"'{callPath}' client:'{e.ClientEndPoint}' allowed");

                            e.Accept = true;
                            return;
                        }

                        if (network.Action == NetworkActionType.Deny.ToString()
                            && found == true)
                        {
                            Log.Warning($"'{callPath}' client:'{e.ClientEndPoint}' denied");

                            e.Accept = false;
                            return;
                        }
                    }

                    Log.Warning($"'{callPath}' client:'{e.ClientEndPoint}' denied");

                    e.Accept = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());

                e.Accept = false;
                return;
            }
        }

        private void System_PreAuthentication(object sender, PreAuthenticationEventArgs e)
        {
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
                            })
                        .SingleOrDefault();

                    /*
                     * does user exist and is user enabled...
                     */

                    if (user == null)
                    {
                        Log.Warning($"'{callPath}' '{e.UserName}' client:'{e.ClientEndPoint}' software:'{e.ClientSoftwareIdentifier}' denied");

                        e.Reject();
                        return;
                    }

                    /*
                     * does user already have sessions going...
                     */

                    var sessions = _server.Sessions
                        .Where(x => x.UserName == e.UserName)
                        .Count();

                    if (sessions >= user.SessionMax)
                    {
                        Log.Warning($"'{callPath}' '{e.UserName}' session maximum {user.SessionMax} and session(s) in use {sessions}");

                        e.Reject();
                        return;
                    }

                    Log.Information($"'{callPath}' '{e.UserName}' session maximum {user.SessionMax} and session(s) in use {sessions}");

                    /*
                     * does user have network filtering rule to allow session...
                     */

                    var networks = uow.Networks.Get(QueryExpressionFactory.GetQueryExpression<Network>()
                        .Where(x => x.IdentityId == user.IdentityId && x.IsEnabled).ToLambda())
                        .OrderBy(x => x.SequenceId);

                    var allowed = false;

                    foreach (var network in networks)
                    {
                        var found = NetworkHelper.ValidateAddress(network, e.ClientAddress);

                        if (network.Action == NetworkActionType.Deny.ToString()
                            && found == true)
                        {
                            Log.Warning($"'{callPath}' '{e.UserName}' client:'{e.ClientEndPoint}' software:'{e.ClientSoftwareIdentifier}' denied");

                            e.Reject();
                            return;
                        }

                        if (network.Action == NetworkActionType.Allow.ToString()
                            && found == true)
                        {
                            Log.Information($"'{callPath}' '{e.UserName}' client:'{e.ClientEndPoint}' software:'{e.ClientSoftwareIdentifier}' allowed");

                            uow.Sessions.Create(
                                new Session
                                {
                                    IdentityId = user.IdentityId,
                                    CallPath = callPath,
                                    Details = "allowed",
                                    LocalEndPoint = _localEndPoint,
                                    RemoteEndPoint = e.ClientEndPoint.ToString(),
                                    RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                    IsActive = true,
                                });

                            uow.Commit();

                            allowed = true;
                            break;
                        }
                    }

                    if (allowed == false)
                    {
                        Log.Warning($"'{callPath}' '{e.UserName}' client:'{e.ClientEndPoint}' software:'{e.ClientSoftwareIdentifier}' denied");

                        e.Reject();
                        return;
                    }

                    /*
                     * does system require password and public key authentication for user...
                     */

                    if (user.IsPublicKeyRequired
                        && user.IsPasswordRequired)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' public key and password required");

                        uow.Sessions.Create(
                            new Session
                            {
                                IdentityId = user.IdentityId,
                                CallPath = callPath,
                                Details = "public key and password required",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = e.ClientEndPoint.ToString(),
                                RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                IsActive = true,
                            });

                        uow.Commit();

                        e.Accept(AuthenticationMethods.PublicKey | AuthenticationMethods.Password);
                        return;
                    }

                    /*
                     * does system require only password authentication for user...
                     */

                    if (user.IsPublicKeyRequired
                        && !user.IsPasswordRequired)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' public key required");

                        uow.Sessions.Create(
                            new Session
                            {
                                IdentityId = user.IdentityId,
                                CallPath = callPath,
                                Details = "public key required",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = e.ClientEndPoint.ToString(),
                                RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                IsActive = true,
                            });

                        uow.Commit();

                        e.Accept(AuthenticationMethods.PublicKey);
                        return;
                    }

                    /*
                     * does system require only public key authentication for user...
                     */

                    if (!user.IsPublicKeyRequired
                        && user.IsPasswordRequired)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' password required");

                        uow.Sessions.Create(
                            new Session
                            {
                                IdentityId = user.IdentityId,
                                CallPath = callPath,
                                Details = "password required",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = e.ClientEndPoint.ToString(),
                                RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                IsActive = true,
                            });

                        uow.Commit();

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

        private void System_Authentication(object sender, AuthenticationEventArgs e)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
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
                            })
                        .Single();

                    if (e.Key != null)
                    {
                        /*
                         * does user have a valid public key...
                         */

                        if (!UserHelper.ValidatePubKey(user.PublicKeys.Where(x => x.IsEnabled).ToList(), e.Key))
                        {
                            Log.Warning($"'{callPath}' '{e.UserName}' public key not accepted");

                            e.Reject();
                            return;
                        }

                        try
                        {
                            /*
                             * does user have valid account...
                             */

                            var identityUser = admin.User_GetV1(user.IdentityId.ToString())
                                .AsTask().Result;

                            if (identityUser.IsLockedOut
                                || !identityUser.PasswordConfirmed
                                || !identityUser.EmailConfirmed)
                            {
                                Log.Warning($"'{callPath}' '{e.UserName}' backed by user '{identityUser.Email}' locked or not confirmed");

                                throw new UnauthorizedAccessException();
                            }

                            /*
                             * does user have valid role...
                             */

                            var identityRoles = admin.User_GetRolesV1(user.IdentityId.ToString())
                                .AsTask().Result;

                            if (!identityRoles.Where(x => x.Name == DefaultConstants.RoleForDaemonUsers_Aurora).Any())
                            {
                                Log.Warning($"'{callPath}' '{e.UserName}' backed by user '{identityUser.Email}' missing " +
                                    $"'{DefaultConstants.RoleForDaemonUsers_Aurora}' role");

                                throw new UnauthorizedAccessException();
                            }
                        }
                        catch (Exception ex)
                            when (ex is HttpRequestException || ex is UnauthorizedAccessException)
                        {
                            Log.Warning($"'{callPath}' '{e.UserName}' public key not accepted");

                            Log.Warning($"'{callPath}'" +
                                $"{Environment.NewLine} {ex.Message}" +
                                $"{Environment.NewLine} {ex.InnerException}");

                            e.Reject();
                            return;
                        }

                        /*
                         * authentication with public key is successful...
                         */

                        Log.Information($"'{callPath}' '{e.UserName}' public key accepted");

                        uow.Sessions.Create(
                            new Session
                            {
                                IdentityId = user.IdentityId,
                                CallPath = callPath,
                                Details = "public key accepted",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = e.ClientEndPoint.ToString(),
                                RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                IsActive = true,
                            });

                        uow.Commit();

                        if (e.PartiallyAccepted
                            || !user.IsPasswordRequired)
                        {
                            /*
                             * an smb mount will not succeed without a user password or ambassador credential.
                             */

                            if (user.FileSystemType.ToLower() == FileSystemProviderType.SMB.ToString().ToLower()
                                && !user.Mount.CredentialId.HasValue)
                            {
                                Log.Warning($"'{callPath}' '{e.UserName}' no credential to mount {FileSystemProviderType.SMB} filesystem");

                                e.Reject();
                                return;
                            }

                            var fs = FileSystemFactory.CreateFileSystem(_factory, logger, user, e.UserName, e.Password);

                            var fsNotify = fs.GetFileSystemNotifier();
                            fsNotify.DeletePreview += User_DeletePreview;
                            fsNotify.DeleteCompleted += User_DeleteCompleted;
                            fsNotify.GetContentPreview += User_GetContentPreview;
                            fsNotify.GetContentCompleted += User_GetContentCompleted;
                            fsNotify.SaveContentPreview += User_SaveContentPreview;
                            fsNotify.SaveContentCompleted += User_SaveContentCompleted;

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
                        try
                        {
                            /*
                             * does user have valid account...
                             */

                            var identityUser = admin.User_GetV1(user.IdentityId.ToString())
                                .AsTask().Result;

                            var identityGrant = sts.ResourceOwner_GrantV2(
                                new ResourceOwnerV2()
                                {
                                    issuer = conf["IdentityCredential:IssuerName"],
                                    client = conf["IdentityCredential:AudienceName"],
                                    grant_type = "password",
                                    user = identityUser.UserName,
                                    password = e.Password,
                                })
                                .AsTask().Result;

                            /*
                             * does user have valid role...
                             */

                            var jwt = new JwtSecurityToken(identityGrant.access_token);

                            if (!jwt.Claims.Any(x => x.Type == ClaimTypes.Role && x.Value == DefaultConstants.RoleForDaemonUsers_Aurora))
                            {
                                Log.Warning($"'{callPath}' '{e.UserName}' backed by user '{identityUser.Email}' missing " +
                                    $"'{DefaultConstants.RoleForDaemonUsers_Aurora}' role");

                                throw new UnauthorizedAccessException();
                            }
                        }
                        catch (Exception ex)
                            when (ex is HttpRequestException || ex is UnauthorizedAccessException)
                        {
                            Log.Warning($"'{callPath}' '{e.UserName}' password not accepted");

                            Log.Warning($"'{callPath}'" +
                                $"{Environment.NewLine} {ex.Message}" +
                                $"{Environment.NewLine} {ex.InnerException}");

                            e.Reject();
                            return;
                        }

                        /*
                         * authentication with password is successful...
                         */

                        Log.Information($"'{callPath}' '{e.UserName}' password accepted");

                        uow.Sessions.Create(
                            new Session
                            {
                                IdentityId = user.IdentityId,
                                CallPath = callPath,
                                Details = "password accepted",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = e.ClientEndPoint.ToString(),
                                RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                IsActive = true,
                            });

                        uow.Commit();

                        if (e.PartiallyAccepted
                            || !user.IsPublicKeyRequired)
                        {
                            var fs = FileSystemFactory.CreateFileSystem(_factory, logger, user, e.UserName, e.Password);

                            var fsNotify = fs.GetFileSystemNotifier();
                            fsNotify.DeletePreview += User_DeletePreview;
                            fsNotify.DeleteCompleted += User_DeleteCompleted;
                            fsNotify.GetContentPreview += User_GetContentPreview;
                            fsNotify.GetContentCompleted += User_GetContentCompleted;
                            fsNotify.SaveContentPreview += User_SaveContentPreview;
                            fsNotify.SaveContentCompleted += User_SaveContentCompleted;

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

        private void System_Disconnect(object sender, DisconnectedEventArgs e)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                var remote = e.Session.ClientEndPoint.ToString();

                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                        .Where(x => x.IdentityAlias == ServerSession.Current.UserName).ToLambda())
                        .SingleOrDefault();

                    /*
                     * if connection attempt did not succeed in connect phase, pre-authentication phase and authentication phase 
                     * there would be no need to decrement sessions in use for a user.
                     */

                    if (user != null)
                    {
                        user.SessionsInUse--;
                        uow.Users.Update(user);
                    }

                    uow.Sessions.Create(
                        new Session
                        {
                            IdentityId = user?.IdentityId ?? null,
                            CallPath = callPath,
                            Details = $"duration:'{e.Session.Duration}'",
                            LocalEndPoint = _localEndPoint,
                            RemoteEndPoint = ServerSession.Current.ClientEndPoint.ToString(),
                            IsActive = true,
                        });
                    uow.Commit();

                    var entries = uow.Sessions.Get(QueryExpressionFactory.GetQueryExpression<Session>()
                        .Where(x => x.LocalEndPoint == _localEndPoint && x.RemoteEndPoint == remote && x.IsActive == true).ToLambda());

                    foreach (var entry in entries)
                        entry.IsActive = false;

                    uow.Sessions.Update(entries);
                    uow.Commit();

                    Log.Information($"'{callPath}'{(string.IsNullOrEmpty(user?.IdentityAlias) ? null : " '" + user.IdentityAlias + "'")}" +
                        $" client:'{e.Session.ClientEndPoint}' duration:'{e.Session.Duration}'");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void System_FileDownloaded(object sender, FileTransferredEventArgs e)
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
                        .Where(x => x.IdentityAlias == ServerSession.Current.UserName).ToLambda(),
                            new List<Expression<Func<User, object>>>()
                            {
                                    x => x.Alerts,
                            })
                        .Single();

                    /*
                     * add event to state...
                     */

                    uow.Sessions.Create(
                        new Session
                        {
                            IdentityId = user.IdentityId,
                            CallPath = callPath,
                            Details = $"file:'/{e.FullPath}' size:'{e.BytesTransferred / 1048576f}MB'",
                            LocalEndPoint = _localEndPoint,
                            RemoteEndPoint = ServerSession.Current.ClientEndPoint.ToString(),
                            IsActive = true,
                        });

                    uow.Commit();

                    /*
                     * send notifications...
                     */

                    foreach (var email in user.Alerts
                        .Where(x => x.ToEmailAddress != null && x.IsEnabled == true && x.OnDownload == true))
                    {
                        _ = alert.Enqueue_EmailV1(
                            new EmailV1()
                            {
                                FromEmail = conf["Notifications:EmailFromAddress"],
                                FromDisplay = conf["Notifications:EmailFromDisplayName"],
                                ToEmail = email.ToEmailAddress,
                                ToDisplay = $"{email.ToDisplayName}",
                                Subject = "File Download Alert",
                                Body = EmailTemplate.NotifyOnFileDownload(conf["Daemons:Sftp:Dns"], ServerSession.Current.UserName,
                                    email.ToDisplayName, "/" + e.FullPath, e.BytesTransferred.ToString(),
                                    ServerSession.Current.ClientEndPoint.ToString()),
                            }).AsTask().Result;
                    }

                    foreach (var text in user.Alerts
                        .Where(x => x.ToPhoneNumber != null && x.IsEnabled == true && x.OnDownload == true))
                    {
                        _ = alert.Enqueue_TextV1(
                            new TextV1()
                            {
                                FromPhoneNumber = conf["Notifications:TextFromPhoneNumber"],
                                ToPhoneNumber = text.ToPhoneNumber,
                                Body = TextTemplate.NotifyOnFileDownload(conf["Daemons:Sftp:Dns"], ServerSession.Current.UserName,
                                    text.ToDisplayName, "/" + e.FullPath, e.BytesTransferred.ToString(),
                                    ServerSession.Current.ClientEndPoint.ToString()),
                            }).AsTask().Result;
                    }

                    Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'/{e.FullPath}' size:'{e.BytesTransferred / 1048576f}MB' " +
                        $"client:'{ServerSession.Current.ClientEndPoint}'");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void System_FileUploaded(object sender, FileTransferredEventArgs e)
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
                        .Where(x => x.IdentityAlias == ServerSession.Current.UserName).ToLambda(),
                            new List<Expression<Func<User, object>>>()
                            {
                                    x => x.Alerts,
                            })
                        .Single();

                    /*
                     * add event to state...
                     */

                    uow.Sessions.Create(
                        new Session
                        {
                            IdentityId = user.IdentityId,
                            CallPath = callPath,
                            Details = $"file:'/{e.FullPath}' size:'{e.BytesTransferred / 1048576f}MB'",
                            LocalEndPoint = _localEndPoint,
                            RemoteEndPoint = ServerSession.Current.ClientEndPoint.ToString(),
                            IsActive = true,
                        });

                    uow.Commit();

                    /*
                     * send notifications...
                     */

                    foreach (var email in user.Alerts
                        .Where(x => x.ToEmailAddress != null && x.IsEnabled == true && x.OnUpload == true))
                    {
                        _ = alert.Enqueue_EmailV1(
                            new EmailV1()
                            {
                                FromEmail = conf["Notifications:EmailFromAddress"],
                                FromDisplay = conf["Notifications:EmailFromDisplayName"],
                                ToEmail = email.ToEmailAddress,
                                ToDisplay = $"{email.ToDisplayName}",
                                Subject = "File Upload Alert",
                                Body = EmailTemplate.NotifyOnFileUpload(conf["Daemons:Sftp:Dns"], ServerSession.Current.UserName,
                                    email.ToDisplayName, "/" + e.FullPath, e.BytesTransferred.ToString(),
                                    ServerSession.Current.ClientEndPoint.ToString())
                            }).AsTask().Result;
                    }

                    foreach (var text in user.Alerts
                            .Where(x => x.ToPhoneNumber != null && x.IsEnabled == true && x.OnUpload == true))
                    {
                        _ = alert.Enqueue_TextV1(
                            new TextV1()
                            {
                                FromPhoneNumber = conf["Notifications:TextFromPhoneNumber"],
                                ToPhoneNumber = text.ToPhoneNumber,
                                Body = TextTemplate.NotifyOnFileUpload(conf["Daemons:Sftp:Dns"], ServerSession.Current.UserName,
                                    text.ToDisplayName, "/" + e.FullPath, e.BytesTransferred.ToString(),
                                    ServerSession.Current.ClientEndPoint.ToString())
                            }).AsTask().Result;
                    }

                    Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'/{e.FullPath}' size:'{e.BytesTransferred / 1048576f}MB' " +
                        $"client:'{ServerSession.Current.ClientEndPoint}'");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void User_DeletePreview(object sender, PreviewSingleNodeOperationEventArgs e)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                if (e.Node.IsFile)
                {
                    using (var scope = _factory.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                            .Where(x => x.IdentityAlias == ServerSession.Current.UserName).ToLambda())
                            .Single();

                        /*
                         * add event to state...
                         */

                        uow.Sessions.Create(
                            new Session
                            {
                                IdentityId = user.IdentityId,
                                CallPath = callPath,
                                Details = $"file:'{e.Node.Path.StringPath}' size:'{e.Node.Length / 1048576f}MB'",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = ServerSession.Current.ClientEndPoint.ToString(),
                                IsActive = true,
                            });

                        uow.Commit();

                        Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'{e.Node.Path.StringPath}' size:'{e.Node.Length / 1048576f}MB' " +
                            $"client:'{ServerSession.Current.ClientEndPoint}'");
                    }
                }
            }
            catch (Exception ex)
            {
                e.CancelOperation();

                Log.Error(ex.ToString());
            }
        }

        private void User_DeleteCompleted(object sender, SingleNodeOperationEventArgs e)
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
                            .Where(x => x.IdentityAlias == ServerSession.Current.UserName).ToLambda(),
                                new List<Expression<Func<User, object>>>()
                                {
                                    x => x.Alerts,
                                })
                            .Single();

                        /*
                         * add event to state...
                         */

                        uow.Sessions.Create(
                            new Session
                            {
                                IdentityId = user.IdentityId,
                                CallPath = callPath,
                                Details = $"file:'{e.ResultNode.Path.StringPath}' size:'{e.ResultNode.Length / 1048576f}MB'",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = ServerSession.Current.ClientEndPoint.ToString(),
                                IsActive = true,
                            });

                        uow.Commit();

                        /*
                         * send notifications...
                         */

                        foreach (var email in user.Alerts
                            .Where(x => x.ToEmailAddress != null && x.IsEnabled == true && x.OnDelete == true))
                        {
                            _ = alert.Enqueue_EmailV1(
                                new EmailV1()
                                {
                                    FromEmail = conf["Notifications:EmailFromAddress"],
                                    FromDisplay = conf["Notifications:EmailFromDisplayName"],
                                    ToEmail = email.ToEmailAddress,
                                    ToDisplay = $"{email.ToDisplayName}",
                                    Subject = "File Delete Alert",
                                    Body = EmailTemplate.NotifyOnFileDelete(conf["Daemons:Sftp:Dns"], ServerSession.Current.UserName,
                                        email.ToDisplayName, e.ResultNode.Path.StringPath)
                                }).AsTask().Result;
                        }

                        foreach (var text in user.Alerts
                            .Where(x => x.ToPhoneNumber != null && x.IsEnabled == true && x.OnDelete == true))
                        {
                            _ = alert.Enqueue_TextV1(
                                new TextV1()
                                {
                                    FromPhoneNumber = conf["Notifications:TextFromPhoneNumber"],
                                    ToPhoneNumber = text.ToPhoneNumber,
                                    Body = TextTemplate.NotifyOnFileDelete(conf["Daemons:Sftp:Dns"], ServerSession.Current.UserName,
                                        text.ToDisplayName, e.ResultNode.Path.StringPath)
                                }).AsTask().Result;
                        }

                        Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'{e.ResultNode.Path.StringPath}' size:'{e.ResultNode.Length / 1048576f}MB' " +
                            $"client:'{ServerSession.Current.ClientEndPoint}'");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void User_GetContentPreview(object sender, PreviewGetContentEventArgs e)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                if (e.Node.IsFile
                    && e.Node.Length > 0)
                {
                    using (var scope = _factory.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                            .Where(x => x.IdentityAlias == ServerSession.Current.UserName).ToLambda())
                            .Single();

                        /*
                         * add event to state...
                         */

                        uow.Sessions.Create(
                            new Session
                            {
                                IdentityId = user.IdentityId,
                                CallPath = callPath,
                                Details = $"file:'{e.Node.Path.StringPath}' size:'{e.Node.Length / 1048576f}MB'",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = ServerSession.Current.ClientEndPoint.ToString(),
                                IsActive = true,
                            });

                        uow.Commit();

                        Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'{e.Node.Path.StringPath}' size:'{e.Node.Length / 1048576f}MB' " +
                            $"client:'{ServerSession.Current.ClientEndPoint}'");
                    }
                }
            }
            catch (Exception ex)
            {
                e.CancelOperation();

                Log.Error(ex.ToString());
            }
        }

        private void User_GetContentCompleted(object sender, GetContentEventArgs e)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                if (e.Node.IsFile
                    && e.Node.Length > 0)
                {
                    using (var scope = _factory.CreateScope())
                    {
                        var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                            .Where(x => x.IdentityAlias == ServerSession.Current.UserName).ToLambda(),
                                new List<Expression<Func<User, object>>>()
                                {
                                    x => x.Alerts,
                                })
                            .Single();

                        /*
                         * add event to state...
                         */

                        uow.Sessions.Create(
                            new Session
                            {
                                IdentityId = user.IdentityId,
                                CallPath = callPath,
                                Details = $"file:'{e.Node.Path.StringPath}' size:'{e.Node.Length / 1048576f}MB'",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = ServerSession.Current.ClientEndPoint.ToString(),
                                IsActive = true,
                            });

                        uow.Commit();

                        Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'{e.Node.Path.StringPath}' size:'{e.Node.Length / 1048576f}MB' " +
                            $"client:'{ServerSession.Current.ClientEndPoint}'");
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void User_SaveContentPreview(object sender, PreviewSaveContentEventArgs e)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                if (e.Node.IsFile)
                {
                    using (var scope = _factory.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                            .Where(x => x.IdentityAlias == ServerSession.Current.UserName).ToLambda())
                            .Single();

                        /*
                         * add event to state...
                         */

                        uow.Sessions.Create(
                            new Session
                            {
                                IdentityId = user.IdentityId,
                                CallPath = callPath,
                                Details = $"file:'{e.Node.Path.StringPath}' size:'{e.Node.Length / 1048576f}MB'",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = ServerSession.Current.ClientEndPoint.ToString(),
                                IsActive = true,
                            });

                        uow.Commit();

                        Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'{e.Node.Path.StringPath}' size:'{e.Node.Length / 1048576f}MB' " +
                            $"client:'{ServerSession.Current.ClientEndPoint}'");
                    }
                }
            }
            catch (Exception ex)
            {
                e.CancelOperation();

                Log.Error(ex.ToString());
            }
        }

        private void User_SaveContentCompleted(object sender, SingleNodeOperationEventArgs e)
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

                        var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<User>()
                            .Where(x => x.IdentityAlias == ServerSession.Current.UserName).ToLambda(),
                                new List<Expression<Func<User, object>>>()
                                {
                                    x => x.Alerts,
                                })
                            .Single();

                        /*
                         * add event to state...
                         */

                        uow.Sessions.Create(
                            new Session
                            {
                                IdentityId = user.IdentityId,
                                CallPath = callPath,
                                Details = $"file:'{e.ResultNode.Path.StringPath}' size:'{e.ResultNode.Length / 1048576f}MB'",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = ServerSession.Current.ClientEndPoint.ToString(),
                                IsActive = true,
                            });

                        uow.Commit();

                        Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'{e.ResultNode.Path.StringPath}' size:'{e.ResultNode.Length / 1048576f}MB' " +
                            $"client:'{ServerSession.Current.ClientEndPoint}'");
                    }
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
