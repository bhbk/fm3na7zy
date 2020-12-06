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
using System.Reflection;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Aurora.Tasks
{
    [DisallowConcurrentExecution]
    public class SessionCleanupJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public SessionCleanupJob(IServiceScopeFactory factory) => _factory = factory;

        public Task Execute(IJobExecutionContext context)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            try
            {
                Log.Information($"'{callPath}' running");

                using (var scope = _factory.CreateScope())
                {
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var keepDuration = int.Parse(conf["Jobs:SessionCleanup:KeepDuration"]);
                    var deleteBeforeDate = DateTime.Now.AddSeconds(-keepDuration);

                    uow.Sessions.Delete(QueryExpressionFactory.GetQueryExpression<Session>()
                        .Where(x => x.CreatedUtc < deleteBeforeDate).ToLambda());
                    uow.Commit();
                }

                Log.Information($"'{callPath}' completed");
                Log.Information($"'{callPath}' will run again at {context.NextFireTimeUtc.Value.LocalDateTime}");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            finally
            {
                GC.Collect();
            }

            return Task.CompletedTask;
        }
    }
}
