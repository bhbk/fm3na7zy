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
    public class FtpService : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _factory;
        private readonly IConfiguration _conf;

        public FtpService(IServiceScopeFactory factory, IConfiguration conf)
        {
            _factory = factory;
            _conf = conf;
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
