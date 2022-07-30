using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using OrleansBook.GrainInterfaces;
using System;
using System.Collections.Generic;

namespace OrleansBook.GrainClases;

public class RobotGrain : Grain, IRobotGrain
{
    private readonly Queue<string> instructions = new Queue<string>();
    private readonly ILogger<RobotGrain> logger;

    public RobotGrain(ILogger<RobotGrain> logger)
    {
        this.logger = logger;
    }

    public Task AddInstruction(string instruction)
    {
        var key = this.GetPrimaryKeyString();
        this.logger.LogDebug("{Key} adding '{Instruction}'", key, instruction);

        this.instructions.Enqueue(instruction);
        return Task.CompletedTask;
    }

    public Task<int> GetInstructionCount()
    {
        return Task.FromResult(this.instructions.Count);
    }

    public Task<string?> GetNextInstruction()
    {
        if(this.instructions.Count == 0)
        {
            return Task.FromResult<string?>(null);
        }

        var instruction = this.instructions.Dequeue();
        var key = this.GetPrimaryKeyString();
        this.logger.LogDebug("{Key} next '{Instruction}'", key, instruction);
        return Task.FromResult<string?>(instruction);
    }
}
