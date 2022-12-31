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
            await NewMethod(client);
    }

    private static async Task NewMethod(IClusterClient client)
    {
        await client.Connect();

        // Trigger grain which uses GrainServiceClient to call GrainService
        var exampleGrain = client.GetGrain<IExampleGrain>("Ex1");
        Console.WriteLine($"ExampleGrain says the time is {exampleGrain.GetDateTime().Result}");

        //
        await GrainWithObserverExample(client);

        // Loop for sending instructions to RobotGrains
        await RobotGrainLoop(client);
    }

    private static async Task RobotGrainLoop(IClusterClient client)
    {
        while (true)
        {
            Console.WriteLine("Please enter a robot name (or 'exit' to stop)...");

            var grainId = Console.ReadLine();
            if ("exit".Equals(grainId, StringComparison.InvariantCultureIgnoreCase))
            {
                break;
            }
            var grain = client.GetGrain<IRobotGrain>(grainId);

            Console.WriteLine("Please enter an instruction, or '-' to read the next instruction...");
            var instruction = Console.ReadLine();

            if ("-".Equals(instruction))
            {
                var poppedInstruction = await grain.GetNextInstruction();
                if (poppedInstruction != null) Console.WriteLine($"{grainId} responded: {poppedInstruction}");
            }
            else if (!string.IsNullOrWhiteSpace(instruction))
            {
                await grain.AddInstruction(instruction);
            }

            var count = await grain.GetInstructionCount();
            Console.WriteLine($"{grainId} has {count} instruction(s)");
        }
    }

    private static async Task GrainWithObserverExample(IClusterClient client)
    {
        var hello = client.GetGrain<IHelloGrain>("Hello1");
        Chat chat1 = new Chat();
        Chat chat2 = new Chat();
        //Create a reference for chat, usable for subscribing to the observable grain.
        var obj1 = await client.CreateObjectReference<IChat>(chat1);
        var obj2 = await client.CreateObjectReference<IChat>(chat2);
        //Subscribe the instance to receive messages.
        await hello.Subscribe(obj1);
        await hello.Subscribe(obj2);

        //Make a call and should get progress updates to both chats 
        await hello.DoSomethingSlow();

        //Unsubscribe second chat
        await hello.UnSubscribe(obj2);

        //Make a call and should get progress updates
        await hello.DoSomethingSlow(); 
    }
}
