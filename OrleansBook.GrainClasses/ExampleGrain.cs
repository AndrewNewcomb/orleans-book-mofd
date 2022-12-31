using Orleans;
using OrleansBook.GrainInterfaces;

namespace OrleansBook.GrainClasses;

public class ExampleGrain : Grain, IExampleGrain
{
    // Example of how to call a GrainServiceClient that then calls a GrainService

    private readonly IExampleGrainServiceClient _client;

    public ExampleGrain(IExampleGrainServiceClient client)
    {
        _client = client;
    }

    public override async Task OnActivateAsync()
    {
        await _client.DoSomething();
        await base.OnActivateAsync();
    }

    public Task<DateTime> GetDateTime() => Task.FromResult<DateTime>(System.DateTime.Now);
}