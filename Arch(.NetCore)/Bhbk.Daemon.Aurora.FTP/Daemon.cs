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
        private bool _disposed;

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
                    using (var scope = _factory.CreateScope())
                    {

                    };
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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
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
