using Bhbk.Lib.Aurora.Data_EF6.Models;
using Bhbk.Lib.Aurora.Data_EF6.UnitOfWorks;
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
using System.Net;
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
                    var verifyAfterDate = DateTime.UtcNow.AddSeconds(-staggerVerify);

                    var files = uow.Files.Get(QueryExpressionFactory.GetQueryExpression<File_EF>()
                        .Where(x => x.LastVerifiedUtc < verifyAfterDate).ToLambda());

                    var problems = new List<File_EF>();

                    foreach (var file in files)
                    {
                        var filePath = new FileInfo(conf["Storage:UnstructuredData"]
                            + Path.DirectorySeparatorChar + file.RealPath
                            + Path.DirectorySeparatorChar + file.RealFileName);

                        try
                        {
                            using (var sha256 = new SHA256Managed())
                            using (var fs = new FileStream(filePath.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                var hash = sha256.ComputeHash(fs);
                                var hashCheck = Strings.GetHexString(hash);

                                if (file.HashValue != hashCheck)
                                {
                                    problems.Add(file);

                                    Log.Error($"'{callPath}' failed on {Dns.GetHostName().ToUpper()} for {filePath}" +
                                        $" recorded hash '{file.HashTypeId}' does not match returned hash '{hashCheck}'");

                                    continue;
                                }
                                else
                                    file.LastVerifiedUtc = DateTime.UtcNow;
                            }

                            uow.Files.Update(file);
                            uow.Commit();
                        }
                        catch (Exception ex)
                            when (ex is CryptographicException || ex is IOException)
                        {
                            problems.Add(file);

                            Log.Error(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()} for {filePath}");
                        }
                    }

                    if (files.Any())
                        Log.Information($"'{callPath}' validation on {files.Count()} files found {problems.Count} problems");
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
            Log.Information($"'{callPath}' will run again at {context.NextFireTimeUtc.GetValueOrDefault().LocalDateTime}");

            return Task.CompletedTask;
        }
    }
}
