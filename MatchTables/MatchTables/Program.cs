using System;
using System.Threading.Tasks;
using MatchTables.Interfaces;
using MatchTables.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MatchTables
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Setup DI
            var serviceProvider = new ServiceCollection()
                    .AddLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Debug))
                    .AddSingleton<IDataAccessService, DataAccessService>()
                    .AddSingleton<ISyncService, SyncService>()
                    .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILoggerFactory>()
                    .CreateLogger<Program>();

            logger.LogDebug("Starting application");



            //do the actual work here
            var syncService = serviceProvider.GetService<ISyncService>();
            await syncService.StartSyncAsync("SourceTable1", "SourceTable2", "");
            


            logger.LogDebug("All done!");

            Console.ReadKey();
        }
    }
}
