using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Streams;
using OrleansBook.GrainInterfaces;

namespace OrleansBook.GrainClases;

[ImplicitStreamSubscription("StartingInstruction")]
public class SubscriberGrain : Grain, ISubscriberGrain, IAsyncObserver<InstructionMessage>
{
    public override async Task OnActivateAsync()
    {
        var key = this.GetPrimaryKey();

        await GetStreamProvider("SMSProvider")
            .GetStream<InstructionMessage>(key, "StartingInstruction")
            .SubscribeAsync(this);

        await base.OnActivateAsync();
    }

    public Task OnNextAsync(InstructionMessage instruction, StreamSequenceToken? token = null)
    {
        var msg = $"{instruction.Robot} starting \"{instruction.Instruction}\"";

        Console.WriteLine(msg);
        return Task.CompletedTask;
    }

    public Task OnCompletedAsync() => Task.CompletedTask;

    public Task OnErrorAsync(Exception ex) => Task.CompletedTask;
}