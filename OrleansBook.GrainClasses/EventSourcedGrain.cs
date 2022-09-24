using System;
using Orleans.EventSourcing;
using OrleansBook.GrainInterfaces;
using Orleans.Providers;
using Orleans;

namespace OrleansBook.GrainClasses;

// https://learn.microsoft.com/en-us/dotnet/orleans/grains/event-sourcing/journaledgrain-basics
// https://dotnet.github.io/orleans/docs/grains/event_sourcing/log_consistency_providers.html
// https://dotnet.github.io/orleans/docs/grains/event_sourcing/event_sourcing_configuration.html

[StorageProvider(ProviderName = "robotStateStore")]
[LogConsistencyProvider(ProviderName = "EventStorage")]
public class EventSourcedGrain : JournaledGrain<EventSourcedState, IEvent>, IRobotGrain
{
    public async Task AddInstruction(string instruction) 
    {
        RaiseEvent(new EnqueueEvent(instruction));
        await ConfirmEvents();
    }

    public Task<int> GetInstructionCount() 
        => Task.FromResult(this.State.Count);
    
    public async Task<string?> GetNextInstruction()
    {
        if(this.State.Count == 0) return null;

        var @event = new DequeueEvent();
        RaiseEvent(@event);
        await ConfirmEvents();
        return @event.Value;
    }

    public async Task<bool> DoSomethingSlow(int slowTaskTimeSeconds, GrainCancellationToken token)
    {
        for(var i = 0; i < slowTaskTimeSeconds; i++)
        {
            await Task.Delay(1000);
            if(token.CancellationToken.IsCancellationRequested) break;           
        }

        return token.CancellationToken.IsCancellationRequested;
    }
}