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
            Log.Information($"'{callPath}' running");

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
            Log.Information($"'{callPath}' completed");
            Log.Information($"'{callPath}' will run again at {context.NextFireTimeUtc.Value.LocalDateTime}");

            return Task.CompletedTask;
        }
    }
}
