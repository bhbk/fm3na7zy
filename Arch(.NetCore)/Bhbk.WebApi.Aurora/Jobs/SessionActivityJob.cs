using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWork;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Aurora.Tasks
{
    [DisallowConcurrentExecution]
    public class SessionActivityJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public SessionActivityJob(IServiceScopeFactory factory) => _factory = factory;

        public Task Execute(IJobExecutionContext context)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            Log.Information($"'{callPath}' running");

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var keepDuration = int.Parse(conf["Jobs:SessionActivity:KeepDuration"]);
                    var deleteBeforeDate = DateTime.UtcNow.AddSeconds(-keepDuration);

                    var entries = uow.Sessions.Get(QueryExpressionFactory.GetQueryExpression<Session>()
                        .Where(x => x.CreatedUtc < deleteBeforeDate).ToLambda());

                    if (entries.Any())
                    {
                        uow.Sessions.Delete(entries);
                        uow.Commit();

                        Log.Information($"'{callPath}' deleted {entries.Count()} session entries");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
            }
            finally
            {
                GC.Collect();
            }

            Log.Information($"'{callPath}' completed");
            Log.Information($"'{callPath}' will run again at {context.NextFireTimeUtc.Value.LocalDateTime}");

            return Task.CompletedTask;
        }
    }
}
