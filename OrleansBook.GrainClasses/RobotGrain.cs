using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using Orleans.Transactions.Abstractions;
using OrleansBook.GrainInterfaces;

namespace OrleansBook.GrainClases;

public class RobotGrain : Grain, IRobotGrain
{
    // The timers and reminders added in branch chapter10_timers_and_reminders
    // were removed in chapter11_transactions to cut down on noise.

    private readonly ITransactionalState<RobotState> state; // was IPersistentState<RobotState>
    private readonly ILogger<RobotGrain> logger;
    private string key;
    private IAsyncStream<InstructionMessage>? stream;

    public RobotGrain(ILogger<RobotGrain> logger, 
        [TransactionalState("robotState", "robotStateStore")] // was PersistentState
        ITransactionalState<RobotState> state) // was IPersistentState
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

        // was
        // this.state.State.Instructions.Enqueue(instruction);
        // await this.state.WriteStateAsync();
        //
        await this.state.PerformUpdate(state =>
            state.Instructions.Enqueue(instruction)
        );
    }

    public async Task<int> GetInstructionCount()
    {
        // was
        // return Task.FromResult(this.state.State.Instructions.Count);
        //
        return await this.state.PerformUpdate(state =>
            state.Instructions.Count
        );        
    }

    public async Task<string?> GetNextInstruction()
    {
        string? instruction = null;
        await this.state.PerformUpdate(state => 
        {
            if(state.Instructions.Count == 0) return;
            instruction = state.Instructions.Dequeue();
        });

        if(null != instruction)
        {
            this.logger.LogDebug("{Key} next '{Instruction}'", this.key, instruction);
            await this.Publish(instruction);
        }

        return instruction;
    }

    public async Task<bool> DoSomethingSlow(int slowTaskTimeSeconds, GrainCancellationToken token)
    {
        for(var i = 0; i < slowTaskTimeSeconds; i++)
        {
            await Task.Delay(1000, token.CancellationToken);
            if(token.CancellationToken.IsCancellationRequested) break;           
        }

        return token.CancellationToken.IsCancellationRequested;
    }
}
