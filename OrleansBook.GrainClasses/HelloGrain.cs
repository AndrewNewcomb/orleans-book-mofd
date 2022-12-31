using Microsoft.Extensions.Logging;
using Orleans;
using OrleansBook.GrainClasses.Utils;
using OrleansBook.GrainInterfaces;

namespace OrleansBook.GrainClasses;

class HelloGrain : Grain, IHelloGrain
{
    private string _key;
    private readonly ObserverManager<IChat> _subsManager;

    public HelloGrain(ILogger<HelloGrain> logger)
    {
        _subsManager =
            new ObserverManager<IChat>(
                TimeSpan.FromMinutes(5), logger, "subs");
    }

    public override async Task OnActivateAsync()
    {
        _key = this.GetPrimaryKeyString();
        await base.OnActivateAsync();
    } 

    // Clients call this to subscribe.
    public Task Subscribe(IChat observer)
    {
        _subsManager.Subscribe(observer, observer);

        return Task.CompletedTask;
    }

    //Clients use this to unsubscribe and no longer receive messages.
    public Task UnSubscribe(IChat observer)
    {
        _subsManager.Unsubscribe(observer);

        return Task.CompletedTask;
    }

    private Task SendUpdateMessage(string message)
    {
        _subsManager.Notify(s => s.ReceiveMessage(message));

        return Task.CompletedTask;
    }

    public async Task DoSomethingSlow()
    {
        await Task.Delay(1000);
        await SendUpdateMessage($"HelloGrain {_key} is 25% done.");
        await Task.Delay(1000);
        await SendUpdateMessage($"HelloGrain {_key} is 50% done.");
        await Task.Delay(1000);
        await SendUpdateMessage($"HelloGrain {_key} is 75% done.");
        await Task.Delay(1000);
        await SendUpdateMessage($"HelloGrain {_key} is done.");
    }
}