using Bhbk.Lib.Aurora.Data.EFCore.Infrastructure_DIRECT;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
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
            Log.Information(typeof(Daemon).Name + " stopped at: {time}", DateTimeOffset.Now);

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
