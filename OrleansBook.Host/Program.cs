using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;
using OrleansBook.GrainClases;
using OrleansBook.GrainClasses;
using OrleansBook.GrainInterfaces;

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
                    logging.SetMinimumLevel(LogLevel.Debug); // was Warning
                });

            // Instrumentation -------------------
            builder.UseDashboard();
            // Stats are operating system specific
            builder.UseLinuxEnvironmentStatistics(); // for Linux via Microsoft.Orleans.OrleansTelemetryConsumers.Linux
            //builder.UsePerfCounterEnvironmentStatistics(); // for Windows via Microsoft.Orleans.OrleansTelemetryConsumers.Counters
            
            // Persistence -------------------
            // builder.AddMemoryGrainStorage("robotStateStore");
            //
            // builder.AddAzureBlobGrainStorage(
            //     name: "robotStateStore",
            //     configureOptions: options =>
            //     {
            //         options.UseJson = true;
            //         options.ConfigureBlobServiceClient(context.Configuration.GetConnectionString("AzureBlobConnectionString"));
            //     });
            //
            builder.AddAzureTableGrainStorage(
                name: "robotStateStore",
                configureOptions: options =>
                {
                    options.UseJson = true;
                    options.ConfigureTableServiceClient(context.Configuration.GetConnectionString("AzureTableConnectionString"));
                });
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

            // Event Sourcing -------------------
            // If LogConsistencyProvider is not specified it defaults to the state storage provider
            // builder.AddStateStorageBasedLogConsistencyProvider("EventStorage"); // does not store the latest state
            builder.AddLogStorageBasedLogConsistencyProvider("EventStorage"); // stores history of events            
            // builder.AddCustomStorageBasedLogConsistencyProvider("EventStorage"); // not tried this one

            // Chapter15 -------------------
            builder.Configure<SiloMessagingOptions>(options =>
                options.PropagateActivityId = true
            );

            builder.AddIncomingGrainCallFilter<MyIncomingGrainCallFilter>();
            builder.AddOutgoingGrainCallFilter<MyOutgoingGrainCallFilter>();
            
            // builder.AddStartupTask(async (IServiceProvider services, CancellationToken cancellation) =>
            // {
            //     var factory = services.GetRequiredService<IGrainFactory>();
            //     var grain = factory.GetGrain<IRobotGrain>("ROBBIE");
            //     while(!cancellation.IsCancellationRequested)
            //     {
            //         if(await grain.GetInstructionCount() == 0) await grain.AddInstruction("Put the kettle on");
            //         await Task.Delay(10000);
            //     }
            // });
            builder.AddStartupTask<MyStartupTask>();

            builder.AddGrainService<ExampleGrainService>();

            builder.ConfigureServices(s =>
            {
                s.AddSingleton<IExampleGrainService, ExampleGrainService>();
                s.AddSingleton<IExampleGrainServiceClient, ExampleGrainServiceClient>();
            });
        });

        return hb;
    }
}