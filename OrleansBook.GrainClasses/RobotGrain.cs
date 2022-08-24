using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using OrleansBook.GrainInterfaces;
using System;
using System.Collections.Generic;
using Orleans.Streams;

namespace OrleansBook.GrainClases;

public class RobotGrain : Grain, IRobotGrain
{
    private readonly IPersistentState<RobotState> state;
    private readonly ILogger<RobotGrain> logger;
    private IAsyncStream<InstructionMessage>? stream;


    public RobotGrain(ILogger<RobotGrain> logger, 
        [PersistentState("robotState", "robotStateStore")]
        IPersistentState<RobotState> state)
    {
        this.logger = logger;
        this.state = state;

        // Calling GetPrimaryKeyString or setting the stream in the constructor 
        // result in exception 'Passing a half baked grain as an argument'. 
        //this.stream = this
        //    .GetStreamProvider("SMSProvider");
        //    .GetStream<InstructionMessage>(Guid.Empty, "StartingInstruction");
    }

    private Task Publish(string instruction)
    {
        if(this.stream is null)
        {
            this.stream = this
                .GetStreamProvider("SMSProvider")
                .GetStream<InstructionMessage>(Guid.Empty, "StartingInstruction");
        }

        var key = this.GetPrimaryKeyString();
        var message = new InstructionMessage(instruction, key);

        return this.stream.OnNextAsync(message);
    }

    public async Task AddInstruction(string instruction)
    {
        var key = this.GetPrimaryKeyString();
        this.logger.LogDebug("{Key} adding '{Instruction}'", key, instruction);

        this.state.State.Instructions.Enqueue(instruction);
        await this.state.WriteStateAsync();
    }

    public Task<int> GetInstructionCount()
    {
        return Task.FromResult(this.state.State.Instructions.Count);
    }

    public async Task<string?> GetNextInstruction()
    {
        if(this.state.State.Instructions.Count == 0)
        {
            return null;
        }

        var instruction = this.state.State.Instructions.Dequeue();
        var key = this.GetPrimaryKeyString();
        this.logger.LogDebug("{Key} next '{Instruction}'", key, instruction);

        await this.Publish(instruction);

        await this.state.WriteStateAsync();
        return instruction;
    }
}
