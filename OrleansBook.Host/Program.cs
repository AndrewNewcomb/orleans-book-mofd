using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Hosting;
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
 
    public static IHostBuilder CreateHostBuilder(string[] args) =>    
        new HostBuilder()
        .UseOrleans(builder => 
        {
            builder.ConfigureApplicationParts(parts =>
                parts.AddApplicationPart(typeof(RobotGrain).Assembly).WithReferences())
                .UseLocalhostClustering();    
        });
}