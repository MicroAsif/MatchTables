using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using MatchTables.Helpers;
using MatchTables.Interfaces;
using MatchTables.Options;
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
           await Parser.Default.ParseArguments<ParseOptions>(args)
                .WithParsedAsync(RunOptions);
        
        }
        static async Task RunOptions(ParseOptions opts)
        {
            //Setup DI
            var serviceProvider = new ServiceCollection()
                    .AddLogging(loggingBuilder => loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Debug))
                    .AddSingleton<IDataAccessService, DataAccessService>()
                    .AddSingleton<ISyncService, SyncService>()
                    .AddSingleton<IDbHelper, DBHelper>()
                    .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILoggerFactory>()
                    .CreateLogger<Program>();

            logger.LogDebug("Starting application");

          
            var syncService = serviceProvider.GetService<ISyncService>();
            await syncService.StartSyncAsync(opts.table1, opts.table2, opts.primarykey);


            logger.LogDebug("All done!");

            Console.ReadKey();
        }
    }
}
