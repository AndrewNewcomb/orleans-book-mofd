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

            // Instrumentation
            builder.UseDashboard();
            // Stats are operating system specific
            builder.UseLinuxEnvironmentStatistics(); // for Linux via Microsoft.Orleans.OrleansTelemetryConsumers.Linux
            //builder.UsePerfCounterEnvironmentStatistics(); // for Windows via Microsoft.Orleans.OrleansTelemetryConsumers.Counters
            
            // Persistence
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
        });

        return hb;
    }             
}