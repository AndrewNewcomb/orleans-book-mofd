using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using OrleansBook.GrainInterfaces;

namespace OrleansBook.GrainClases;

[StatelessWorker]
public class BatchGrain : Grain, IBatchGrain
{
    public Task AddInstructions((string Name, string Instruction)[] values)
    {
        var tasks = values.Select(kv =>
            this.GrainFactory
                .GetGrain<IRobotGrain>(kv.Name)
                .AddInstruction(kv.Instruction)
        );

        return Task.WhenAll(tasks);
    }
}