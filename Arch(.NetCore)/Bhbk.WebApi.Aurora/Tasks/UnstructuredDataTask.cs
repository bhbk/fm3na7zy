using Bhbk.Lib.Aurora.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Aurora.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Aurora.Domain.Helpers;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Aurora.Tasks
{
    public class UnstructuredDataTask : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly JsonSerializerSettings _serializer;
        private readonly int _pollingDelay, _verifyDelay;
        public string Status { get; private set; }

        public UnstructuredDataTask(IServiceScopeFactory factory, IConfiguration conf)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            _factory = factory;
            _pollingDelay = int.Parse(conf["Tasks:UnstructuredData:PollingDelay"]);
            _verifyDelay = int.Parse(conf["Tasks:UnstructuredData:VerifyDelay"]);
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = callPath + " not run yet."
                }, _serializer);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            while (!cancellationToken.IsCancellationRequested)
            {
#if DEBUG
                Log.Information($"'{callPath}' sleeping for {TimeSpan.FromSeconds(_pollingDelay)}");
#endif
                await Task.Delay(TimeSpan.FromSeconds(_pollingDelay), cancellationToken);
#if DEBUG
                Log.Information($"'{callPath}' running");
#endif
                try
                {
                    using (var scope = _factory.CreateScope())
                    {
                        var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var files = uow.UserFiles.Get(QueryExpressionFactory.GetQueryExpression<tbl_UserFiles>()
                            .Where(x => x.LastVerified < DateTime.UtcNow.AddSeconds(-_verifyDelay)).ToLambda());

                        var problems = new List<tbl_UserFiles>();

                        foreach(var file in files)
                        {
                            var filePath = new FileInfo(conf["Storage:BaseLocalPath"]
                                + Path.DirectorySeparatorChar + file.RealPath
                                + Path.DirectorySeparatorChar + file.RealFileName);

                            try
                            {
                                using (var sha256 = new SHA256Managed())
                                using (var fs = new FileStream(filePath.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    var hash = sha256.ComputeHash(fs);
                                    var hashCheck = HashHelper.GetHexString(hash);

                                    if (file.FileHashSHA256 != hashCheck)
                                    {
                                        problems.Add(file);

                                        Log.Error($"{callPath} validation on {filePath} returned hash of '{hashCheck}' that does not" +
                                            $" match recorded hash of '{file.FileHashSHA256}'");

                                        continue;
                                    }
                                    else
                                        file.LastVerified = DateTime.UtcNow;
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

                            Status = JsonConvert.SerializeObject(
                                new
                                {
                                    status = msg
                                }, _serializer);

                            Log.Information(msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }

                /*
                 * https://docs.microsoft.com/en-us/aspnet/core/performance/memory?view=aspnetcore-3.1
                 */
                GC.Collect();
            }
        }
    }
}
