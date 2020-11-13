using Bhbk.Lib.Aurora.Data.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.Models_DIRECT;
using Bhbk.Lib.Common.Primitives;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Aurora.Tasks
{
    [DisallowConcurrentExecution]
    public class UnstructuredDataJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public UnstructuredDataJob(IServiceScopeFactory factory) => _factory = factory;

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

                    var staggerVerify = int.Parse(conf["Jobs:UnstructuredData:StaggerVerify"]);

                    var files = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserFile>()
                        .Where(x => x.LastVerifiedUtc < DateTime.UtcNow.AddSeconds(-staggerVerify)).ToLambda());

                    var problems = new List<tbl_UserFile>();

                    foreach (var file in files)
                    {
                        var filePath = new FileInfo(conf["Storage:UnstructuredDataPath"]
                            + Path.DirectorySeparatorChar + file.RealPath
                            + Path.DirectorySeparatorChar + file.RealFileName);

                        try
                        {
                            using (var sha256 = new SHA256Managed())
                            using (var fs = new FileStream(filePath.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                var hash = sha256.ComputeHash(fs);
                                var hashCheck = Strings.GetHexString(hash);

                                if (file.HashSHA256 != hashCheck)
                                {
                                    problems.Add(file);

                                    Log.Error($"{callPath} validation on {filePath} returned hash of '{hashCheck}' that does not" +
                                        $" match recorded hash of '{file.HashSHA256}'");

                                    continue;
                                }
                                else
                                    file.LastVerifiedUtc = DateTime.UtcNow;
                            }
                        }
                        catch (CryptographicException ex)
                        {
                            Log.Error($"{callPath} validation on {filePath} returned error {ex.ToString()}");
                        }
                        catch (IOException ex)
                        {
                            Log.Error($"{callPath} validation on {filePath} returned error {ex.ToString()}");
                        }
                    }

                    uow.Commit();

                    if (files.Any())
                    {
                        var msg = $"{callPath} completed. Performed validation on {files.Count().ToString()} files" +
                            $" and found {problems.Count()} problems.";

                        Log.Information(msg);
                    }
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
