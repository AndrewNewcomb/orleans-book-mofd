using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Configuration;
using OrleansBook.GrainInterfaces;

namespace OrleansBook.Client;

class Program
{
    static async Task Main()
    {
        var client = new ClientBuilder()
            .UseLocalhostClustering()
            .Build();  

        using (client)
        {
            await client.Connect();

            while(true)
            {
                Console.WriteLine("Please enter a robot name...");
                var grainId = Console.ReadLine();
                var grain = client.GetGrain<IRobotGrain>(grainId);

                Console.WriteLine("Please enter an instruction...");
                var instruction = Console.ReadLine();
                if(!string.IsNullOrWhiteSpace(instruction))
                {
                    await grain.AddInstruction(instruction);
                }

                var count = await grain.GetInstructionCount();
                Console.WriteLine($"{grainId} has {count} instruction(s)");
            }
        }  
    }
}
