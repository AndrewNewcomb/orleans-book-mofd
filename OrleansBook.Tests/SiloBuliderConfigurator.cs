using Orleans.Hosting;
using Orleans.TestingHost;
using Moq;
using Orleans.Runtime;
using OrleansBook.GrainClases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OrleansBook.Tests;

class SiloBuliderConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder
            .AddMemoryGrainStorage("robotStateStore")
            .AddMemoryGrainStorage("PubSubStore")
            .AddSimpleMessageStreamProvider("SMSProvider");

        var mockState = new Mock<IPersistentState<RobotState>>();
        mockState.Setup(s => s.State).Returns(new RobotState());

        siloBuilder.ConfigureServices(services =>
        {
            services.AddSingleton<IPersistentState<RobotState>>(mockState.Object);
            services.AddSingleton<ILogger<RobotGrain>>(new Mock<ILogger<RobotGrain>>().Object);
        });
    }
}