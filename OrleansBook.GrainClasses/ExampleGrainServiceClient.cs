using Orleans.Runtime.Services;
using OrleansBook.GrainInterfaces;

namespace OrleansBook.GrainClasses;

public class ExampleGrainServiceClient : GrainServiceClient<IExampleGrainService>, IExampleGrainServiceClient
{
    public ExampleGrainServiceClient(IServiceProvider serviceProvider) : base(serviceProvider)
    { }

    public Task DoSomething() => this.GrainService.DoSomething();
}