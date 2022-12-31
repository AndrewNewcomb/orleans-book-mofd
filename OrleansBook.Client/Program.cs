using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using OrleansBook.GrainInterfaces;

namespace OrleansBook.Client;

class Program
{
    static async Task Main()
    {
        var client = new ClientBuilder()
            //.AddApplicationInsightsTelemetryConsumer("INSTRUMENTATION_KEY")
            .UseLocalhostClustering()
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
            })
            .Build();  

        using (client)
        {
            await client.Connect();

            var exampleGrain = client.GetGrain<IExampleGrain>("Ex1");
            Console.WriteLine($"ExampleGrain says the time is {exampleGrain.GetDateTime().Result}");

            while(true)
            {
                Console.WriteLine("Please enter a robot name (or 'exit' to stop)...");              

                var grainId = Console.ReadLine();
                if("exit".Equals(grainId, StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
                var grain = client.GetGrain<IRobotGrain>(grainId);

                Console.WriteLine("Please enter an instruction, or '-' to read the next instruction...");
                var instruction = Console.ReadLine();

                if("-".Equals(instruction))
                {
                    var poppedInstruction = await grain.GetNextInstruction();
                    if(poppedInstruction != null) Console.WriteLine($"{grainId} responded: {poppedInstruction}");
                }
                else if(!string.IsNullOrWhiteSpace(instruction))
                {
                    await grain.AddInstruction(instruction);
                }

                var count = await grain.GetInstructionCount();
                Console.WriteLine($"{grainId} has {count} instruction(s)");
            }
        }  
    }
}
