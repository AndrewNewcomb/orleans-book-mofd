using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Statistics;
using OrleansBook.GrainClases;

// Needs nuget packages
// Microsoft.Orleans.OrleansTelemetryConsumers.Linux for linux
// Microsoft.Orleans.OrleansTelemetryConsumers.Counters for windows

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
 
    public static IHostBuilder CreateHostBuilder(string[] args) =>    
        new HostBuilder()
            // Wanted to set log level from appsettings, but can't get
            // it to work. 
            // .ConfigureAppConfiguration((hostingContext, config) =>
            // {
            //     config.AddJsonFile("appsettings.json", optional: false);
            // })
            .UseOrleans(builder => 
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

                builder.UseDashboard();
                builder.UseLinuxEnvironmentStatistics();
                //builder.UsePerfCounterEnvironmentStatistics();
                builder.AddMemoryGrainStorage("robotStateStore");  
            });             
}