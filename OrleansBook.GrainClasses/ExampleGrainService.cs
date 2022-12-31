using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using OrleansBook.GrainInterfaces;

namespace OrleansBook.GrainClases;

public class ExampleGrainService : GrainService, IExampleGrainService
{
    public ExampleGrainService(
        IGrainIdentity id,
        Silo silo,
        ILoggerFactory loggerFactory) : base(id, silo, loggerFactory)
    { }

    public override Task Init(IServiceProvider serviceProvider)
    {
        Console.WriteLine("ExampleGrainService Init");
        return base.Init(serviceProvider);
    }

    public override Task Start()
    {
        Console.WriteLine("ExampleGrainService Start");
        return base.Start();
    }

    public override Task Stop()
    {
        Console.WriteLine("ExampleGrainService Stop");
        return base.Stop();
    }

    public Task DoSomething()
    {
        Console.WriteLine($"ExampleGrainService DoSomething at {System.DateTime.Now}");
        return Task.CompletedTask;
    }
}