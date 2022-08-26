using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using OrleansBook.GrainInterfaces;

namespace OrleansBook.GrainClases;

public class RobotGrain : Grain, IRobotGrain
{
    private readonly IPersistentState<RobotState> state;
    private readonly ILogger<RobotGrain> logger;
    private string key;
    private IAsyncStream<InstructionMessage>? stream;


    public RobotGrain(ILogger<RobotGrain> logger, 
        [PersistentState("robotState", "robotStateStore")]
        IPersistentState<RobotState> state)
    {
        this.logger = logger;
        this.state = state;
        this.key = "";       
    }

    public override async Task OnActivateAsync()
    {
        this.key = this.GetPrimaryKeyString();

        this.stream = this
           .GetStreamProvider("SMSProvider")
           .GetStream<InstructionMessage>(Guid.Empty, "StartingInstruction");

        await base.OnActivateAsync();
    }  

    private Task Publish(string instruction)
    {
        var message = new InstructionMessage(instruction, this.key);
        return this.stream!.OnNextAsync(message);
    }

    public async Task AddInstruction(string instruction)
    {
        this.logger.LogDebug("{Key} adding '{Instruction}'", this.key, instruction);

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
        this.logger.LogDebug("{Key} next '{Instruction}'", this.key, instruction);

        await this.Publish(instruction);

        await this.state.WriteStateAsync();
        return instruction;
    }
}
