using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;

public class MyOutgoingGrainCallFilter : IOutgoingGrainCallFilter
{
    // Chpater 15. System wide filter, set via `builder.AddOutgoingGrainCallFilter<MyOutgoingGrainCallFilter>()`

    private readonly ILogger<MyOutgoingGrainCallFilter> logger;

    public MyOutgoingGrainCallFilter(ILogger<MyOutgoingGrainCallFilter> logger)
    {
        this.logger = logger;     
    }

    public async Task Invoke(IOutgoingGrainCallContext context)
    {
        var grainType = context.Grain.GetType();
        if(grainType.AssemblyQualifiedName!.StartsWith("OrleansBook."))
        {
            this.logger.Debug(
                "OutgoingGrainCall RequestContext.ActivityId {activityId}", RequestContext.ActivityId);
        }

        await context.Invoke();
    }
}