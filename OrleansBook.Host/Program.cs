using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Statistics;
using OrleansBook.GrainClases;

namespace OrleansBook.Host;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        await host.StartAsync();
        
        Console.WriteLine("Press enter to stop the Silo...");
        Console.ReadLine();

        await host.StopAsync();
    }
 
    public static IHostBuilder CreateHostBuilder(string[] args) 
    {
        // REMARK Wanted to set log level from appsettings, but can't get it to work. 

        var hb = new HostBuilder();
        hb.ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.AddUserSecrets<OrleansBook.Host.Program>();
        });

        hb.UseOrleans((context, builder) => 
        {
            builder
                //.AddApplicationInsightsTelemetryConsumer("INSTRUMENTATION_KEY")
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(RobotGrain).Assembly).WithReferences())
                .UseLocalhostClustering()
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Warning);
                });

            // Instrumentation -------------------
            builder.UseDashboard();
            // Stats are operating system specific
            builder.UseLinuxEnvironmentStatistics(); // for Linux via Microsoft.Orleans.OrleansTelemetryConsumers.Linux
            //builder.UsePerfCounterEnvironmentStatistics(); // for Windows via Microsoft.Orleans.OrleansTelemetryConsumers.Counters
            
            // Persistence -------------------
            builder.AddMemoryGrainStorage("robotStateStore");
            //
            // builder.AddAzureBlobGrainStorage(
            //     name: "robotStateStore",
            //     configureOptions: options =>
            //     {
            //         options.UseJson = true;
            //         options.ConfigureBlobServiceClient(context.Configuration.GetConnectionString("AzureBlobConnectionString"));
            //     });
            //
            // builder.AddAzureTableGrainStorage(
            //     name: "robotStateStore",
            //     configureOptions: options =>
            //     {
            //         options.UseJson = true;
            //         options.ConfigureTableServiceClient(context.Configuration.GetConnectionString("AzureTableConnectionString"));
            //     });
            //
            // builder.AddAdoNetGrainStorage("robotStateStore", options =>
            //      {
            //          options.Invariant = "Npgsql";
            //          options.ConnectionString = context.Configuration.GetConnectionString("PostgresConnectionString");
            //          options.UseJsonFormat = true;
            //      });

            // Streaming meta database -------------------
            builder.AddMemoryGrainStorage("PubSubStore");
            // builder.AddAzureTableGrainStorage(
            //     name: "PubSubStore",
            //     configureOptions: options =>
            //     {
            //         options.UseJson = true;
            //         options.ConfigureTableServiceClient(context.Configuration.GetConnectionString("AzureTableConnectionString"));
            //     });

            //
            // Streaming -------------------
            builder.AddSimpleMessageStreamProvider("SMSProvider"); 
            //
            // // https://docs.microsoft.com/en-us/dotnet/orleans/implementation/streams-implementation/azure-queue-streams
            // builder.AddAzureQueueStreams("SMSProvider", configurator => 
            //     {
            //         configurator.ConfigureAzureQueue(
            //             ob => ob.Configure(options =>
            //             {
            //                 options.ConfigureQueueServiceClient(context.Configuration.GetConnectionString("AzureTableConnectionString"));
            //                 options.QueueNames = new List<string> { "orleans-stream-azurequeueprovider-0" };
            //             }));
            //         configurator.ConfigureCacheSize(1024);
            //         configurator.ConfigurePullingAgent(ob => ob.Configure(options =>
            //             {
            //                 options.GetQueueMsgsTimerPeriod = TimeSpan.FromMilliseconds(200);
            //             }));
            //     }
            // );

            // Reminders -------------------
            builder.UseInMemoryReminderService();
            //
            //builder.UseAzureTableReminderService(
            //    options => options.ConfigureTableServiceClient(context.Configuration.GetConnectionString("AzureTableConnectionString"))
            //);
            
            // Transactions --- yes really, distributed transactions ---
            builder
                .AddAzureTableTransactionalStateStorage(
                    name: "TransactionStore",
                    configureOptions: options =>
                    {
                        options.ConfigureTableServiceClient(context.Configuration.GetConnectionString("AzureTableConnectionString"));
                    })
                .UseTransactions();
        });

        return hb;
    }             
}