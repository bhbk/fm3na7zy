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
using System.Security.Cryptography;
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
        private bool _disposed;

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

                        var license = uow.Settings.Get(QueryExpressionFactory.GetQueryExpression<E_Setting>()
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
                                var privKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<E_PrivateKey>()
                                    .Where(x => x.UserId == null && x.IsEnabled == true && x.KeyAlgo == hostKeyAlgo).ToLambda())
                                    .OrderBy(x => x.CreatedUtc)
                                    .LastOrDefault();

                                if (privKey != null)
                                {
                                    var keyBytes = Encoding.UTF8.GetBytes(privKey.KeyValue);
                                    _server.Keys.Add(new SshPrivateKey(keyBytes, AES.DecryptString(privKey.EncryptedPass, secret)));

                                    Log.Information($"'{callPath}' 'system' loading public/private key pair [algo] {privKey.KeyAlgo} [format] {privKey.KeyFormat}" +
                                        $"{Environment.NewLine}  [public key GUID]{privKey.PublicKeyId} [private key GUID] {privKey.Id}");
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

                        var entries = uow.Sessions.Get(QueryExpressionFactory.GetQueryExpression<E_Session>()
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
                    Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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
                                var user = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                                    .Where(x => x.UserName == session.UserName).ToLambda(),
                                        new List<Expression<Func<E_Login, object>>>()
                                        {
                                            x => x.Usage,
                                        })
                                    .SingleOrDefault();

                                /*
                                 * if connection disrupted decrement sessions in use for a user.
                                 */

                                if (user != null)
                                {
                                    user.Usage.SessionsInUse--;
                                    uow.Usages.Update(user.Usage);
                                }

                                uow.Sessions.Create(
                                    new E_Session
                                    {
                                        UserId = user?.UserId ?? null,
                                        CallPath = callPath,
                                        Details = $"duration:'{session.Duration}' reason:'node-restart'",
                                        LocalEndPoint = _localEndPoint,
                                        RemoteEndPoint = session.ClientEndPoint.ToString(),
                                        IsActive = false,
                                    });

                                var entries = uow.Sessions.Get(QueryExpressionFactory.GetQueryExpression<E_Session>()
                                    .Where(x => x.LocalEndPoint == _localEndPoint && x.IsActive == true).ToLambda());

                                foreach (var entry in entries)
                                    entry.IsActive = false;

                                uow.Sessions.Update(entries);
                                uow.Commit();

                                session.SendMessage($"'{session.UserName}' client:'{session.ClientEndPoint}' " +
                                    $"duration:'{session.Duration}' reason:'node-restart'");

                                session.Close();

                                Log.Warning($"'{callPath}' '{session.UserName}' client:'{session.ClientEndPoint}' " +
                                    $"duration:'{session.Duration}' reason:'node-restart'");
                            }
                        }

                        _server.Unbind();
                        _server.Stop();
                    }
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
                }
            }, cancellationToken);
        }

        private void System_CheckKeyPair(IConfiguration conf, IUnitOfWork uow,
            SshHostKeyAlgorithm keyAlgo, SignatureHashAlgorithm sigAlgo, int keySize)
        {
            var keyAlgoStr = keyAlgo.ToString();
            var privKey = uow.PrivateKeys.Get(QueryExpressionFactory.GetQueryExpression<E_PrivateKey>()
                .Where(x => x.UserId == null && x.IsEnabled == true && x.KeyAlgo == keyAlgoStr).ToLambda())
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

                    var networks = uow.Networks.Get(QueryExpressionFactory.GetQueryExpression<E_Network>()
                        .Where(x => x.UserId == null && x.IsEnabled).ToLambda())
                        .OrderBy(x => x.SequenceId);

                    /*
                     * does system have network filtering rule to allow session... this is global...
                     */

                    foreach (var network in networks)
                    {
                        var found = NetworkHelper.ValidateAddress(network, e.ClientAddress);

                        if (network.Action == NetworkActionType.Deny.ToString()
                            && found == true)
                        {
                            Log.Warning($"'{callPath}' client:'{e.ClientEndPoint}' denied:'network'");

                            uow.Sessions.Create(
                                new E_Session
                                {
                                    CallPath = callPath,
                                    Details = "denied:'network'",
                                    LocalEndPoint = _localEndPoint,
                                    RemoteEndPoint = e.ClientEndPoint.ToString(),
                                    IsActive = true,
                                });

                            uow.Commit();

                            e.Accept = false;
                            return;
                        }

                        if (network.Action == NetworkActionType.Allow.ToString()
                            && found == true)
                        {
                            Log.Information($"'{callPath}' client:'{e.ClientEndPoint}' allowed:'network'");

                            uow.Sessions.Create(
                                new E_Session
                                {
                                    CallPath = callPath,
                                    Details = "allowed:'network'",
                                    LocalEndPoint = _localEndPoint,
                                    RemoteEndPoint = e.ClientEndPoint.ToString(),
                                    IsActive = true,
                                });

                            uow.Commit();

                            e.Accept = true;
                            return;
                        }
                    }

                    Log.Warning($"'{callPath}' client:'{e.ClientEndPoint}' denied:'default'");

                    uow.Sessions.Create(
                        new E_Session
                        {
                            CallPath = callPath,
                            Details = "denied:'default'",
                            LocalEndPoint = _localEndPoint,
                            RemoteEndPoint = e.ClientEndPoint.ToString(),
                            IsActive = true,
                        });

                    uow.Commit();

                    e.Accept = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");

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
                    var user = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                        .Where(x => x.UserName == e.UserName && x.IsEnabled).ToLambda(),
                            new List<Expression<Func<E_Login, object>>>()
                            {
                                x => x.Networks,
                                x => x.PublicKeys,
                                x => x.Usage,
                            })
                        .SingleOrDefault();

                    /*
                     * does user exist and is user enabled...
                     */

                    if (user == null)
                    {
                        Log.Warning($"'{callPath}' user:'{e.UserName}' denied");

                        uow.Sessions.Create(
                            new E_Session
                            {
                                CallPath = callPath,
                                Details = $"user:'{e.UserName}' denied",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = e.ClientEndPoint.ToString(),
                                RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                IsActive = true,
                            });

                        uow.Commit();

                        e.Reject();
                        return;
                    }

                    /*
                     * does user have correct login type...
                     */

                    if (user.UserAuthType != UserAuthType.Identity.ToString())
                    {
                        Log.Warning($"'{callPath}' user:'{e.UserName}' denied:'login-type'");

                        uow.Sessions.Create(
                            new E_Session
                            {
                                CallPath = callPath,
                                Details = $"user:'{e.UserName}' denied:'login-type'",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = e.ClientEndPoint.ToString(),
                                RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                IsActive = true,
                            });

                        uow.Commit();

                        e.Reject();
                        return;
                    }

                    /*
                     * does user already have sessions going...
                     */

                    var sessions = _server.Sessions
                        .Where(x => x.UserName == e.UserName)
                        .Count();

                    if (sessions >= user.Usage.SessionMax)
                    {
                        Log.Warning($"'{callPath}' '{e.UserName}' session-maximum:'{user.Usage.SessionMax}' sessions-in-use:'{sessions}' denied");

                        uow.Sessions.Create(
                            new E_Session
                            {
                                UserId = user.UserId,
                                CallPath = callPath,
                                Details = $"session-maximum:'{user.Usage.SessionMax}' sessions-in-use:'{sessions}' denied",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = e.ClientEndPoint.ToString(),
                                RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                IsActive = true,
                            });

                        uow.Commit();

                        e.Reject();
                        return;
                    }

                    Log.Information($"'{callPath}' '{e.UserName}' session-maximum:'{user.Usage.SessionMax}' sessions-in-use:'{sessions}' allowed");

                    uow.Sessions.Create(
                        new E_Session
                        {
                            UserId = user.UserId,
                            CallPath = callPath,
                            Details = $"session-maximum:'{user.Usage.SessionMax}' sessions-in-use:'{sessions}' allowed",
                            LocalEndPoint = _localEndPoint,
                            RemoteEndPoint = e.ClientEndPoint.ToString(),
                            RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                            IsActive = true,
                        });

                    uow.Commit();

                    /*
                     * does user have network filtering rule to allow session...
                     */

                    var networks = uow.Networks.Get(QueryExpressionFactory.GetQueryExpression<E_Network>()
                        .Where(x => x.UserId == user.UserId && x.IsEnabled).ToLambda())
                        .OrderBy(x => x.SequenceId);

                    var allowed = false;

                    foreach (var network in networks)
                    {
                        var found = NetworkHelper.ValidateAddress(network, e.ClientAddress);

                        if (network.Action == NetworkActionType.Deny.ToString()
                            && found == true)
                        {
                            Log.Warning($"'{callPath}' '{e.UserName}' denied:'network'");

                            uow.Sessions.Create(
                                new E_Session
                                {
                                    UserId = user.UserId,
                                    CallPath = callPath,
                                    Details = $"denied:'network'",
                                    LocalEndPoint = _localEndPoint,
                                    RemoteEndPoint = e.ClientEndPoint.ToString(),
                                    RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                    IsActive = true,
                                });

                            uow.Commit();

                            e.Reject();
                            return;
                        }

                        if (network.Action == NetworkActionType.Allow.ToString()
                            && found == true)
                        {
                            Log.Information($"'{callPath}' '{e.UserName}' allowed:'network'");

                            uow.Sessions.Create(
                                new E_Session
                                {
                                    UserId = user.UserId,
                                    CallPath = callPath,
                                    Details = $"allowed:'network'",
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
                        Log.Warning($"'{callPath}' '{e.UserName}' denied:'network'");

                        uow.Sessions.Create(
                            new E_Session
                            {
                                UserId = user.UserId,
                                CallPath = callPath,
                                Details = $"denied:'network'",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = e.ClientEndPoint.ToString(),
                                RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                IsActive = true,
                            });

                        uow.Commit();

                        e.Reject();
                        return;
                    }

                    /*
                     * does system require password and public key authentication for user...
                     */

                    if (user.IsPublicKeyRequired
                        && user.IsPasswordRequired)
                    {
                        Log.Information($"'{callPath}' '{e.UserName}' required:'public-key and password'");

                        uow.Sessions.Create(
                            new E_Session
                            {
                                UserId = user.UserId,
                                CallPath = callPath,
                                Details = "required:'public-key and password'",
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
                        Log.Information($"'{callPath}' '{e.UserName}' required:'public-key'");

                        uow.Sessions.Create(
                            new E_Session
                            {
                                UserId = user.UserId,
                                CallPath = callPath,
                                Details = "required:'public-key'",
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
                        Log.Information($"'{callPath}' '{e.UserName}' required:'password'");

                        uow.Sessions.Create(
                            new E_Session
                            {
                                UserId = user.UserId,
                                CallPath = callPath,
                                Details = "required:'password'",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = e.ClientEndPoint.ToString(),
                                RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                IsActive = true,
                            });

                        uow.Commit();

                        e.Accept(AuthenticationMethods.Password);
                        return;
                    }

                    Log.Warning($"'{callPath}' '{e.UserName}' denied:'default'");

                    uow.Sessions.Create(
                        new E_Session
                        {
                            UserId = user.UserId,
                            CallPath = callPath,
                            Details = "denied:'default'",
                            LocalEndPoint = _localEndPoint,
                            RemoteEndPoint = e.ClientEndPoint.ToString(),
                            RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                            IsActive = true,
                        });

                    uow.Commit();

                    e.Reject();
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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

                    var user = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                        .Where(x => x.UserName == e.UserName && x.IsEnabled).ToLambda(),
                            new List<Expression<Func<E_Login, object>>>()
                            {
                                x => x.Mount,
                                x => x.PublicKeys,
                                x => x.Usage,
                            })
                        .Single();

                    if (e.Key != null)
                    {
                        /*
                         * does user have a valid public key...
                         */

                        if (!UserHelper.ValidatePubKey(user.PublicKeys.Where(x => x.IsEnabled).ToList(), e.Key))
                        {
                            Log.Warning($"'{callPath}' '{e.UserName}' denied:'public-key'");

                            uow.Sessions.Create(
                                new E_Session
                                {
                                    UserId = user.UserId,
                                    CallPath = callPath,
                                    Details = "denied:'pubkey-key'",
                                    LocalEndPoint = _localEndPoint,
                                    RemoteEndPoint = e.ClientEndPoint.ToString(),
                                    RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                    IsActive = true,
                                });

                            uow.Commit();

                            e.Reject();
                            return;
                        }

                        try
                        {
                            if (user.UserAuthType == UserAuthType.Identity.ToString())
                            {
                                /*
                                 * is identity user and password valid...
                                 */

                                var identityUser = admin.User_GetV1(user.UserId.ToString())
                                    .AsTask().Result;

                                if (identityUser.IsLockedOut
                                    || !identityUser.PasswordConfirmed
                                    || !identityUser.EmailConfirmed)
                                {
                                    Log.Warning($"'{callPath}' '{e.UserName}' identity-user:'{identityUser.Email}' denied:'invalid'");

                                    uow.Sessions.Create(
                                        new E_Session
                                        {
                                            UserId = user.UserId,
                                            CallPath = callPath,
                                            Details = $"identity-user:'{identityUser.Email}' denied:'invalid'",
                                            LocalEndPoint = _localEndPoint,
                                            RemoteEndPoint = e.ClientEndPoint.ToString(),
                                            RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                            IsActive = true,
                                        });

                                    uow.Commit();

                                    throw new UnauthorizedAccessException();
                                }

                                /*
                                 * is identity user in required role(s)...
                                 */

                                var identityRoles = admin.User_GetRolesV1(user.UserId.ToString())
                                    .AsTask().Result;

                                if (!identityRoles.Where(x => x.Name == DefaultConstants.RoleForDaemonUsers_Aurora).Any())
                                {
                                    Log.Warning($"'{callPath}' '{e.UserName}' identity-user:'{identityUser.Email}'" +
                                        $" missing-role:'{DefaultConstants.RoleForDaemonUsers_Aurora}'");

                                    uow.Sessions.Create(
                                        new E_Session
                                        {
                                            UserId = user.UserId,
                                            CallPath = callPath,
                                            Details = $"identity-user:'{identityUser.Email}' missing-role:'{DefaultConstants.RoleForDaemonUsers_Aurora}'",
                                            LocalEndPoint = _localEndPoint,
                                            RemoteEndPoint = e.ClientEndPoint.ToString(),
                                            RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                            IsActive = true,
                                        });

                                    uow.Commit();

                                    throw new UnauthorizedAccessException();
                                }
                            }
                            else if (user.UserAuthType == UserAuthType.Local.ToString())
                            {
                                /*
                                 * no need for additional checks if local user and public key valid...
                                 */
                            }
                            else
                                throw new NotImplementedException();
                        }
                        catch (Exception ex)
                            when (ex is CryptographicException || ex is HttpRequestException || ex is UnauthorizedAccessException)
                        {
                            Log.Warning($"'{callPath}'" +
                                $"{Environment.NewLine} {ex.Message}" +
                                $"{Environment.NewLine} {ex.InnerException}");

                            Log.Warning($"'{callPath}' '{e.UserName}' denied:'public-key'");

                            uow.Sessions.Create(
                                new E_Session
                                {
                                    UserId = user.UserId,
                                    CallPath = callPath,
                                    Details = $"denied:'public-key'",
                                    LocalEndPoint = _localEndPoint,
                                    RemoteEndPoint = e.ClientEndPoint.ToString(),
                                    RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                    IsActive = true,
                                });

                            uow.Commit();

                            e.Reject();
                            return;
                        }

                        /*
                         * authentication with public key is successful...
                         */

                        Log.Information($"'{callPath}' '{e.UserName}' accepted:'public-key'");

                        uow.Sessions.Create(
                            new E_Session
                            {
                                UserId = user.UserId,
                                CallPath = callPath,
                                Details = "accepted:'public-key'",
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
                                && !user.Mount.AmbassadorId.HasValue)
                            {
                                Log.Error($"'{callPath}' '{e.UserName}' {FileSystemProviderType.SMB} filesystem mount denied:'missing-credential'");

                                uow.Sessions.Create(
                                    new E_Session
                                    {
                                        UserId = user.UserId,
                                        CallPath = callPath,
                                        Details = $"{FileSystemProviderType.SMB} filesystem mount denied:'missing-credential'",
                                        LocalEndPoint = _localEndPoint,
                                        RemoteEndPoint = e.ClientEndPoint.ToString(),
                                        RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                        IsActive = true,
                                    });

                                uow.Commit();

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

                            /*
                             * persist session count for user so cli has data-point...
                             */

                            user.Usage.SessionsInUse = (short)_server.Sessions
                                .Where(x => x.UserName == e.UserName)
                                .Count();

                            /*
                             * add session that is about to be created...
                             */

                            user.Usage.SessionsInUse++;

                            uow.Usages.Update(user.Usage);
                            uow.Commit();

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
                            if (user.UserAuthType == UserAuthType.Identity.ToString())
                            {
                                /*
                                 * is identity user and password valid...
                                 */

                                var identityUser = admin.User_GetV1(user.UserId.ToString())
                                .AsTask().Result;

                                var identityGrant = sts.ResourceOwner_GrantV2(
                                    new ResourceOwnerV2()
                                    {
                                        issuer = conf["IdentityCredential:IssuerName"],
                                        client = string.Empty,
                                        grant_type = "password",
                                        user = identityUser.UserName,
                                        password = e.Password,
                                    })
                                    .AsTask().Result;

                                /*
                                 * is identity user in required role(s)...
                                 */

                                var jwt = new JwtSecurityToken(identityGrant.access_token);

                                if (!jwt.Claims.Any(x => x.Type == ClaimTypes.Role && x.Value == DefaultConstants.RoleForDaemonUsers_Aurora))
                                {
                                    Log.Warning($"'{callPath}' '{e.UserName}' identity-user:'{identityUser.Email}' " +
                                        $"missing-role:'{DefaultConstants.RoleForDaemonUsers_Aurora}'");

                                    uow.Sessions.Create(
                                        new E_Session
                                        {
                                            UserId = user.UserId,
                                            CallPath = callPath,
                                            Details = $"identity-user:'{identityUser.Email}' missing-role:'{DefaultConstants.RoleForDaemonUsers_Aurora}'",
                                            LocalEndPoint = _localEndPoint,
                                            RemoteEndPoint = e.ClientEndPoint.ToString(),
                                            RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                            IsActive = true,
                                        });

                                    uow.Commit();

                                    throw new UnauthorizedAccessException();
                                }
                            }
                            else if (user.UserAuthType == UserAuthType.Local.ToString())
                            {
                                /*
                                 * is local user and password valid...
                                 */

                                var secret = conf["Databases:AuroraSecret"];
                                var decryptedPass = AES.DecryptString(user.EncryptedPass, secret);

                                if (decryptedPass != e.Password)
                                    throw new UnauthorizedAccessException();
                            }
                            else
                                throw new NotImplementedException();
                        }
                        catch (Exception ex)
                            when (ex is CryptographicException || ex is HttpRequestException || ex is UnauthorizedAccessException)
                        {
                            Log.Warning($"'{callPath}'" +
                                $"{Environment.NewLine} {ex.Message}" +
                                $"{Environment.NewLine} {ex.InnerException}");

                            Log.Warning($"'{callPath}' '{e.UserName}' denied:'password'");

                            uow.Sessions.Create(
                                new E_Session
                                {
                                    UserId = user.UserId,
                                    CallPath = callPath,
                                    Details = $"denied:'password'",
                                    LocalEndPoint = _localEndPoint,
                                    RemoteEndPoint = e.ClientEndPoint.ToString(),
                                    RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                                    IsActive = true,
                                });

                            uow.Commit();

                            e.Reject();
                            return;
                        }

                        /*
                         * authentication with password is successful...
                         */

                        Log.Information($"'{callPath}' '{e.UserName}' accepted:'password'");

                        uow.Sessions.Create(
                            new E_Session
                            {
                                UserId = user.UserId,
                                CallPath = callPath,
                                Details = "accepted:'password'",
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

                            /*
                             * persist session count for user so cli has data-point...
                             */

                            user.Usage.SessionsInUse = (short)_server.Sessions
                                .Where(x => x.UserName == e.UserName)
                                .Count();

                            /*
                             * add session that is about to be created...
                             */

                            user.Usage.SessionsInUse++;

                            uow.Usages.Update(user.Usage);
                            uow.Commit();

                            e.Accept(fsUser);
                            return;
                        }

                        /*
                         * authenticate partially if another kind of credential has not been provided yet.
                         */

                        e.AcceptPartially();
                        return;
                    }

                    /*
                     * no public key or password provided.
                     */

                    Log.Warning($"'{callPath}' '{e.UserName}' denied:'default'");

                    uow.Sessions.Create(
                        new E_Session
                        {
                            UserId = user.UserId,
                            CallPath = callPath,
                            Details = "denied:'default'",
                            LocalEndPoint = _localEndPoint,
                            RemoteEndPoint = e.ClientEndPoint.ToString(),
                            RemoteSoftwareIdentifier = e.ClientSoftwareIdentifier,
                            IsActive = true,
                        });

                    uow.Commit();

                    e.Reject();
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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

                    var user = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                        .Where(x => x.UserName == ServerSession.Current.UserName).ToLambda(),
                            new List<Expression<Func<E_Login, object>>>()
                            {
                                x => x.Usage,
                            })
                        .SingleOrDefault();

                    /*
                     * if connection attempt did not succeed in connect phase, pre-authentication phase and authentication 
                     * stages there would be no need to decrement sessions in use for a user.
                     */

                    if (user != null)
                    {
                        user.Usage.SessionsInUse--;
                        uow.Usages.Update(user.Usage);
                    }

                    uow.Sessions.Create(
                        new E_Session
                        {
                            UserId = user?.UserId ?? null,
                            CallPath = callPath,
                            Details = $"duration:'{e.Session.Duration}'",
                            LocalEndPoint = _localEndPoint,
                            RemoteEndPoint = ServerSession.Current.ClientEndPoint.ToString(),
                            IsActive = false,
                        });

                    var entries = uow.Sessions.Get(QueryExpressionFactory.GetQueryExpression<E_Session>()
                        .Where(x => x.LocalEndPoint == _localEndPoint && x.RemoteEndPoint == remote && x.IsActive == true).ToLambda());

                    foreach (var entry in entries)
                        entry.IsActive = false;

                    uow.Sessions.Update(entries);
                    uow.Commit();

                    Log.Information($"'{callPath}'{(string.IsNullOrEmpty(user?.UserName) ? null : " '" + user.UserName + "'")}" +
                        $" client:'{e.Session.ClientEndPoint}' duration:'{e.Session.Duration}'");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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

                    var user = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                        .Where(x => x.UserName == ServerSession.Current.UserName).ToLambda(),
                            new List<Expression<Func<E_Login, object>>>()
                            {
                                x => x.Alerts,
                            })
                        .Single();

                    /*
                     * add event to state...
                     */

                    uow.Sessions.Create(
                        new E_Session
                        {
                            UserId = user.UserId,
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

                    Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'/{e.FullPath}'" +
                        $" size:'{e.BytesTransferred / 1048576f}MB'");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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

                    var user = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                        .Where(x => x.UserName == ServerSession.Current.UserName).ToLambda(),
                            new List<Expression<Func<E_Login, object>>>()
                            {
                                x => x.Alerts,
                            })
                        .Single();

                    /*
                     * add event to state...
                     */

                    uow.Sessions.Create(
                        new E_Session
                        {
                            UserId = user.UserId,
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

                    Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'/{e.FullPath}'" +
                        $" size:'{e.BytesTransferred / 1048576f}MB'");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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
                        var user = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                            .Where(x => x.UserName == ServerSession.Current.UserName).ToLambda())
                            .Single();

                        /*
                         * add event to state...
                         */

                        uow.Sessions.Create(
                            new E_Session
                            {
                                UserId = user.UserId,
                                CallPath = callPath,
                                Details = $"file:'{e.Node.Path.StringPath}' size:'{e.Node.Length / 1048576f}MB'",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = ServerSession.Current.ClientEndPoint.ToString(),
                                IsActive = true,
                            });

                        uow.Commit();

                        Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'{e.Node.Path.StringPath}'" +
                            $" size:'{e.Node.Length / 1048576f}MB'");
                    }
                }
            }
            catch (Exception ex)
            {
                e.CancelOperation();

                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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

                        var user = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                            .Where(x => x.UserName == ServerSession.Current.UserName).ToLambda(),
                                new List<Expression<Func<E_Login, object>>>()
                                {
                                    x => x.Alerts,
                                })
                            .Single();

                        /*
                         * add event to state...
                         */

                        uow.Sessions.Create(
                            new E_Session
                            {
                                UserId = user.UserId,
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

                        Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'{e.ResultNode.Path.StringPath}'" +
                            $" size:'{e.ResultNode.Length / 1048576f}MB'");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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
                        var user = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                            .Where(x => x.UserName == ServerSession.Current.UserName).ToLambda())
                            .Single();

                        /*
                         * add event to state...
                         */

                        uow.Sessions.Create(
                            new E_Session
                            {
                                UserId = user.UserId,
                                CallPath = callPath,
                                Details = $"file:'{e.Node.Path.StringPath}' size:'{e.Node.Length / 1048576f}MB'",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = ServerSession.Current.ClientEndPoint.ToString(),
                                IsActive = true,
                            });

                        uow.Commit();

                        Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'{e.Node.Path.StringPath}'" +
                            $" size:'{e.Node.Length / 1048576f}MB'");
                    }
                }
            }
            catch (Exception ex)
            {
                e.CancelOperation();

                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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

                        var user = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                            .Where(x => x.UserName == ServerSession.Current.UserName).ToLambda(),
                                new List<Expression<Func<E_Login, object>>>()
                                {
                                    x => x.Alerts,
                                })
                            .Single();

                        /*
                         * add event to state...
                         */

                        uow.Sessions.Create(
                            new E_Session
                            {
                                UserId = user.UserId,
                                CallPath = callPath,
                                Details = $"file:'{e.Node.Path.StringPath}' size:'{e.Node.Length / 1048576f}MB'",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = ServerSession.Current.ClientEndPoint.ToString(),
                                IsActive = true,
                            });

                        uow.Commit();

                        Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'{e.Node.Path.StringPath}'" +
                            $" size:'{e.Node.Length / 1048576f}MB'");
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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
                        var user = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                            .Where(x => x.UserName == ServerSession.Current.UserName).ToLambda())
                            .Single();

                        /*
                         * add event to state...
                         */

                        uow.Sessions.Create(
                            new E_Session
                            {
                                UserId = user.UserId,
                                CallPath = callPath,
                                Details = $"file:'{e.Node.Path.StringPath}' size:'{e.Node.Length / 1048576f}MB'",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = ServerSession.Current.ClientEndPoint.ToString(),
                                IsActive = true,
                            });

                        uow.Commit();

                        Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'{e.Node.Path.StringPath}'" +
                            $" size:'{e.Node.Length / 1048576f}MB'");
                    }
                }
            }
            catch (Exception ex)
            {
                e.CancelOperation();

                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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

                        var user = uow.Logins.Get(QueryExpressionFactory.GetQueryExpression<E_Login>()
                            .Where(x => x.UserName == ServerSession.Current.UserName).ToLambda(),
                                new List<Expression<Func<E_Login, object>>>()
                                {
                                    x => x.Alerts,
                                })
                            .Single();

                        /*
                         * add event to state...
                         */

                        uow.Sessions.Create(
                            new E_Session
                            {
                                UserId = user.UserId,
                                CallPath = callPath,
                                Details = $"file:'{e.ResultNode.Path.StringPath}' size:'{e.ResultNode.Length / 1048576f}MB'",
                                LocalEndPoint = _localEndPoint,
                                RemoteEndPoint = ServerSession.Current.ClientEndPoint.ToString(),
                                IsActive = true,
                            });

                        uow.Commit();

                        Log.Information($"'{callPath}' '{ServerSession.Current.UserName}' file:'{e.ResultNode.Path.StringPath}'" +
                            $" size:'{e.ResultNode.Length / 1048576f}MB'");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
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
