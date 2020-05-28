using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.Daemon.Aurora.FTP
{
    public class Daemon : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _factory;
        private readonly IConfiguration _conf;
        private readonly int _delay;

        public Daemon(IServiceScopeFactory factory, IConfiguration conf)
        {
            _factory = factory;
            _conf = conf;
            _delay = int.Parse(_conf["Tasks:FtpWorker:PollingDelay"]);
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            while (!cancellationToken.IsCancellationRequested)
            {
#if DEBUG
                Log.Information($"'{callPath}' sleeping for {TimeSpan.FromSeconds(_delay)}");
#endif
                await Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken);
#if DEBUG
                Log.Information($"'{callPath}' running");
#endif

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
            try
            {

            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            await ExecuteAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
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

        }
    }
}
