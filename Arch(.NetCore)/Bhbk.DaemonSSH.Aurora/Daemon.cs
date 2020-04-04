using Bhbk.DaemonSSH.Aurora.FileSystems;
using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Cryptography.Hashing;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebex;
using Rebex.IO.FileSystem;
using Rebex.Net;
using Rebex.Net.Servers;
using Rebex.Security.Cryptography;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/*
 * https://www.rebex.net/file-server/features/easy-to-use-api.aspx
 */

namespace Bhbk.DaemonSSH.Aurora
{
    public class Daemon : IHostedService, IDisposable
    {
        private LogLevel _level;
        private readonly IServiceScopeFactory _factory;
        private readonly IConfiguration _conf;
        private readonly FileServer _server;
        private readonly int _delay;

        public Daemon(IServiceScopeFactory factory, IConfiguration conf)
        {
            _level = LogLevel.Info;
            _factory = factory;
            _conf = conf;
            _delay = int.Parse(_conf["Tasks:SshWorker:PollingDelay"]);
            _server = new FileServer();
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken);

                //Log.Information(typeof(Daemon).Name + " worker running at: {time}", DateTimeOffset.Now);

                try
                {
                    using (var scope = _factory.CreateScope())
                    {

                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }

                GC.Collect();
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Information(typeof(Daemon).Name + " started at: {time}", DateTimeOffset.Now);

            AsymmetricKeyAlgorithm.Register(Curve25519.Create);
            AsymmetricKeyAlgorithm.Register(Ed25519.Create);
            AsymmetricKeyAlgorithm.Register(EllipticCurveAlgorithm.Create);

            try
            {
                Load_LicenseKeys();
                Load_SystemKeys();

                _server.LogWriter = new ConsoleLogWriter(_level);
                _server.Settings.AllowedAuthenticationMethods = AuthenticationMethods.PublicKey | AuthenticationMethods.Password;
                _server.Authentication += Event_Authentication;
                _server.Connecting += Event_Connecting;
                _server.FileDownloaded += Event_FileDownloaded;
                _server.FileUploaded += Event_FileUploaded;
                _server.PathAccessAuthorization += Event_PathAccessAuthorization;
                _server.PreAuthentication += Event_PreAuthentication;
                _server.ShellCommand += Event_ShellCommand;

                _server.Bind(new IPEndPoint(IPAddress.Parse(_conf["Daemons:SshServer:Host"]), int.Parse(_conf["Daemons:SshServer:Port"])), FileServerProtocol.Sftp);
                _server.Bind(new IPEndPoint(IPAddress.Parse(_conf["Daemons:SshServer:Host"]), int.Parse(_conf["Daemons:SshServer:Port"])), FileServerProtocol.Shell);
                _server.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            await ExecuteAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information(typeof(Daemon).Name + " stopped at: {time}", DateTimeOffset.Now);

            try
            {
                if (_server.IsRunning)
                {
                    _server.Unbind(new IPEndPoint(IPAddress.Parse(_conf["Daemons:SshServer:Host"]), int.Parse(_conf["Daemons:SshServer:Port"])));
                    _server.Stop();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void Event_Authentication(object sender, AuthenticationEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#authentication
             */

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var user = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                        .Where(x => x.UserName == e.UserName).ToLambda(),
                            new List<Expression<Func<tbl_Users, object>>>()
                            {
                                x => x.tbl_UserFolders,
                                x => x.tbl_UserFiles,
                                x => x.tbl_UserPasswords,
                                x => x.tbl_UserPublicKeys
                            }).SingleOrDefault();

                    var keys = user.tbl_UserPublicKeys.Where(x => x.Enabled);
                    var pass = user.tbl_UserPasswords;

                    if (e.Key != null)
                    {
                        Log.Information($"Authenticate in-progress for '{e.UserName}' at '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with public key.");

                        if (keys.Where(x => x.KeyValueBase64 == Convert.ToBase64String(e.Key.GetPublicKey(), Base64FormattingOptions.None)).Any())
                        {
                            Log.Information($"Authenticate success for '{e.UserName}' at '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with public key.");

                            var path = new FileInfo(_conf["Storage:LocalBasePath"]
                                + Path.DirectorySeparatorChar + "data"
                                + Path.DirectorySeparatorChar + user.UserName);

                            if (!Directory.Exists(path.FullName))
                                Directory.CreateDirectory(path.FullName);

                            var localfs = new LocalFileSystemProvider(path.FullName, FileSystemType.ReadWrite);
                            var fsuser = new FileServerUser(e.UserName, e.Password);
                            fsuser.SetFileSystem(localfs);

                            e.Accept(fsuser);
                            return;
                        }
                        else
                        {
                            Log.Error($"Authenticate failure for '{e.UserName}' at '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with public key.");

                            e.Reject();
                            return;
                        }
                    }
                    else if (e.Password != null)
                    {
                        Log.Information($"Authenticate in-progress for '{e.UserName}' at '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with password.");

                        if (PBKDF2.Validate(pass.PasswordHashPBKDF2, e.Password))
                        {
                            Log.Information($"Authenticate success for '{e.UserName}' at '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with password.");

                            var path = new FileInfo(_conf["Storage:LocalBasePath"]
                                + Path.DirectorySeparatorChar + "data"
                                + Path.DirectorySeparatorChar + user.UserName);

                            if (!Directory.Exists(path.FullName))
                                Directory.CreateDirectory(path.FullName);

                            //var localfs = new LocalFileSystemProvider(path.FullName, FileSystemType.ReadWrite);
                            //var fsuser = new FileServerUser(e.UserName, e.Password);
                            //fsuser.SetFileSystem(localfs);

                            //e.Accept(fsuser);

                            //var memoryfs = new MemoryFSProvider();
                            //var fsuser = new FileServerUser(e.UserName, e.Password);
                            //fsuser.SetFileSystem(memoryfs);

                            //e.Accept(fsuser);

                            var databasefs = new DatabaseFileSystemProvider(_factory, _conf, user);
                            var fsuser = new FileServerUser(e.UserName, e.Password);
                            fsuser.SetFileSystem(databasefs);

                            e.Accept(fsuser);
                            return;
                        }
                        else
                        {
                            Log.Error($"Authenticate failure for '{e.UserName}' at '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with password.");

                            e.Reject();
                            return;
                        }
                    }

                    Log.Error($"Authenticate not possible for '{e.UserName}' at '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}'.");

                    e.Reject();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void Event_Connecting(object sender, ConnectingEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#connecting
             */

            try
            {
                e.Accept = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void Event_FileDownloaded(object sender, FileTransferredEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#upload-download-events
             */

            try
            {
                Log.Information($"User '{e.User.Name}' downloaded file '{e.FullPath}', bytes transferred: {e.BytesTransferred}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void Event_FileUploaded(object sender, FileTransferredEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#upload-download-events
             */

            try
            {
                Log.Information($"User '{e.User.Name}' uploaded file '{e.FullPath}', bytes transferred: {e.BytesTransferred}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void Event_PathAccessAuthorization(object sender, PathAccessAuthorizationEventArgs e)
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

        private void Event_PreAuthentication(object sender, PreAuthenticationEventArgs e)
        {
            /*
             * https://www.rebex.net/file-server/features/events.aspx#pre-authentication
             */

            try
            {
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
                        Log.Information($"Pre-authenticate allowed for '{e.UserName}' at '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with password or public key.");

                        e.Accept(AuthenticationMethods.PublicKey | AuthenticationMethods.Password);
                        return;
                    }

                    if (pass == null
                        && keys.Count() > 0)
                    {
                        Log.Information($"Pre-authenticate allowed for '{e.UserName}' at '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with public key.");

                        e.Accept(AuthenticationMethods.PublicKey);
                        return;
                    }

                    if (pass != null 
                        && pass.Enabled
                        && keys.Count() == 0)
                    {
                        Log.Information($"Pre-authenticate allowed for '{e.UserName}' at '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}' with password.");

                        e.Accept(AuthenticationMethods.Password);
                        return;
                    }

                    Log.Error($"Pre-authenticate not allowed for '{e.UserName}' at '{e.ClientEndPoint}' running '{e.ClientSoftwareIdentifier}'.");

                    e.Reject();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void Event_ShellCommand(object sender, ShellCommandEventArgs e)
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

        private void Load_LicenseKeys()
        {
            /*
             * https://www.rebex.net/support/trial/
             */

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    Rebex.Licensing.Key = uow.Settings.Get(x => x.ConfigKey == "RebexLicense")
                        .OrderBy(x => x.Created).Last().ConfigValue;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private void Load_SystemKeys()
        {
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    /*
                    * https://en.wikipedia.org/wiki/Digital_Signature_Algorithm
                    */
                    var dsaKey = uow.SystemKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_SystemKeys>()
                        .Where(x => x.KeyValueAlgo == "DSA").ToLambda())
                        .OrderBy(x => x.Created).LastOrDefault();

                    if (dsaKey == null)
                    {
                        var stream = new MemoryStream();
                        var key = SshPrivateKey.Generate(SshHostKeyAlgorithm.DSS, 1024);
                        var pass = AlphaNumeric.CreateString(32);

                        key.Save(stream, pass, SshPrivateKeyFormat.Pkcs8);

                        dsaKey = uow.SystemKeys.Create(
                            new tbl_SystemKeys
                            {
                                Id = Guid.NewGuid(),
                                KeyValueBase64 = Encoding.ASCII.GetString(stream.ToArray()),
                                KeyValueAlgo = "DSA",
                                KeyValuePass = pass,
                                KeyValueFormat = SshPrivateKeyFormat.Pkcs8.ToString().ToUpper(),
                                Enabled = true,
                                Created = DateTime.Now,
                                Immutable = true
                            });
                        uow.Commit();
                    }

                    var dsaBytes = Encoding.ASCII.GetBytes(dsaKey.KeyValueBase64);
                    _server.Keys.Add(new SshPrivateKey(dsaBytes, dsaKey.KeyValuePass));

                    /*
                     * https://en.wikipedia.org/wiki/RSA_(cryptosystem)
                     */
                    var rsaKey = uow.SystemKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_SystemKeys>()
                        .Where(x => x.KeyValueAlgo == "RSA").ToLambda())
                        .OrderBy(x => x.Created).LastOrDefault();

                    if (rsaKey == null)
                    {
                        var stream = new MemoryStream();
                        var key = SshPrivateKey.Generate(SshHostKeyAlgorithm.RSA, 2048);
                        var pass = AlphaNumeric.CreateString(32);

                        key.Save(stream, pass, SshPrivateKeyFormat.Pkcs8);

                        rsaKey = uow.SystemKeys.Create(
                            new tbl_SystemKeys
                            {
                                Id = Guid.NewGuid(),
                                KeyValueBase64 = Encoding.ASCII.GetString(stream.ToArray()),
                                KeyValueAlgo = "RSA",
                                KeyValuePass = pass,
                                KeyValueFormat = SshPrivateKeyFormat.Pkcs8.ToString().ToUpper(),
                                Enabled = true,
                                Created = DateTime.Now,
                                Immutable = true
                            });
                        uow.Commit();
                    }

                    var rsaBytes = Encoding.ASCII.GetBytes(rsaKey.KeyValueBase64);
                    _server.Keys.Add(new SshPrivateKey(rsaBytes, rsaKey.KeyValuePass));

                    /*
                     * https://en.wikipedia.org/wiki/Elliptic_Curve_Digital_Signature_Algorithm
                     */
                    var ecdsaKey = uow.SystemKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_SystemKeys>()
                        .Where(x => x.KeyValueAlgo == "ECDSA").ToLambda())
                        .OrderBy(x => x.Created).LastOrDefault();

                    if (ecdsaKey == null)
                    {
                        var stream = new MemoryStream();
                        var key = SshPrivateKey.Generate(SshHostKeyAlgorithm.ECDsaNistP521, 521);
                        var pass = AlphaNumeric.CreateString(32);

                        key.Save(stream, pass, SshPrivateKeyFormat.Pkcs8);

                        ecdsaKey = uow.SystemKeys.Create(
                            new tbl_SystemKeys
                            {
                                Id = Guid.NewGuid(),
                                KeyValueBase64 = Encoding.ASCII.GetString(stream.ToArray()),
                                KeyValueAlgo = "ECDSA",
                                KeyValuePass = pass,
                                KeyValueFormat = SshPrivateKeyFormat.Pkcs8.ToString().ToUpper(),
                                Enabled = true,
                                Created = DateTime.Now,
                                Immutable = true
                            });
                        uow.Commit();
                    }

                    var ecdsaBytes = Encoding.ASCII.GetBytes(ecdsaKey.KeyValueBase64);
                    _server.Keys.Add(new SshPrivateKey(ecdsaBytes, ecdsaKey.KeyValuePass));

                    /*
                     * https://en.wikipedia.org/wiki/Curve25519
                     */
                    var ed25519Key = uow.SystemKeys.Get(QueryExpressionFactory.GetQueryExpression<tbl_SystemKeys>()
                        .Where(x => x.KeyValueAlgo == "ED25519").ToLambda())
                        .OrderBy(x => x.Created).LastOrDefault();

                    if (ed25519Key == null)
                    {
                        var stream = new MemoryStream();
                        var key = SshPrivateKey.Generate(SshHostKeyAlgorithm.ED25519, 256);
                        var pass = AlphaNumeric.CreateString(32);

                        key.Save(stream, pass, SshPrivateKeyFormat.Pkcs8);

                        ed25519Key = uow.SystemKeys.Create(
                            new tbl_SystemKeys
                            {
                                Id = Guid.NewGuid(),
                                KeyValueBase64 = Encoding.ASCII.GetString(stream.ToArray()),
                                KeyValueAlgo = "ED25519",
                                KeyValuePass = pass,
                                KeyValueFormat = SshPrivateKeyFormat.Pkcs8.ToString().ToUpper(),
                                Enabled = true,
                                Created = DateTime.Now,
                                Immutable = true
                            });
                        uow.Commit();
                    }

                    var ed25519Bytes = Encoding.ASCII.GetBytes(ed25519Key.KeyValueBase64);
                    _server.Keys.Add(new SshPrivateKey(ed25519Bytes, ed25519Key.KeyValuePass));
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
