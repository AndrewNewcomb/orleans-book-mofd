using Orleans;
using Orleans.Runtime;
using OrleansBook.GrainInterfaces;

namespace OrleansBook.Host;

internal class MyStartupTask : IStartupTask
{
    private readonly IGrainFactory _grainFactory;

    public MyStartupTask(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task Execute(CancellationToken cancellationToken) 
    {
        var grain = _grainFactory.GetGrain<IRobotGrain>("ROBBIE");
        while(!cancellationToken.IsCancellationRequested)
        {
            if(await grain.GetInstructionCount() == 0) await grain.AddInstruction("Put the kettle on");
            await Task.Delay(10000);
        }
    }
}