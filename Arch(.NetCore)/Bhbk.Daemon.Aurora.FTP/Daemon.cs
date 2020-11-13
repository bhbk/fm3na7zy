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

        public Daemon(IServiceScopeFactory factory)
        {
            _factory = factory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                try
                {

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

                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }, cancellationToken);
        }

        public void Dispose()
        {

        }
    }
}
