using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Bhbk.Daemon.Aurora.SFTP.Jobs
{
    [DisallowConcurrentExecution]
    public class MOTDDownloadJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public MOTDDownloadJob(IServiceScopeFactory factory) => _factory = factory;

        public Task Execute(IJobExecutionContext context)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
#if DEBUG
            Log.Information($"'{callPath}' running");
#endif
            try
            {
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
#if DEBUG
            Log.Information($"'{callPath}' completed");
#endif
            return Task.CompletedTask;
        }
    }
}
