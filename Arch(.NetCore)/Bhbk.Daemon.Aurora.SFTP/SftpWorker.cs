using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.Daemon.Aurora.SFTP
{
    public class SftpWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly IConfiguration _conf;
        private readonly int _delay;

        public SftpWorker(IServiceScopeFactory factory, IConfiguration conf)
        {
            _factory = factory;
            _conf = conf;
            _delay = int.Parse(_conf["Daemons:SftpWorker:PollingDelay"]);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            await Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
#if DEBUG
                        Log.Information($"'{callPath}' sleeping for {TimeSpan.FromSeconds(_delay)}");
#endif
                        Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken).Wait();
#if DEBUG
                        Log.Information($"'{callPath}' running");
#endif
                        using (var scope = _factory.CreateScope())
                        {

                        }

                        /*
                         * https://docs.microsoft.com/en-us/aspnet/core/performance/memory?view=aspnetcore-3.1
                         */
                        GC.Collect();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
            }, cancellationToken);
        }
    }
}
