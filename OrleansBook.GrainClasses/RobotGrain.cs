using System.Threading.Tasks;
using Orleans;
using OrleansBook.GrainInterfaces;
using System;
using System.Collections.Generic;

namespace OrleansBook.GrainClases;

public class RobotGrain : Grain, IRobotGrain
{
    private Queue<string> instructions = new Queue<string>();

    public Task AddInstruction(string instruction)
    {
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
        return Task.FromResult<string?>(instruction);
    }
}
