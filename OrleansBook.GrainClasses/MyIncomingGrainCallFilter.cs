using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;

public class MyIncomingGrainCallFilter : IIncomingGrainCallFilter
{
    // Chapter 15. System wide filter, set via `builder.AddIncomingGrainCallFilter<MyIncomingGrainCallFilter>()`

    private readonly ILogger<MyIncomingGrainCallFilter> logger;

    public MyIncomingGrainCallFilter(ILogger<MyIncomingGrainCallFilter> logger)
    {
        this.logger = logger;     
    }

    public async Task Invoke(IIncomingGrainCallContext context)
    {
        var grainType = context.Grain.GetType();
        if(grainType.AssemblyQualifiedName!.StartsWith("OrleansBook."))
        {
            var stopwatch = Stopwatch.StartNew();

            if(RequestContext.ActivityId == Guid.Empty) RequestContext.ActivityId = Guid.NewGuid();
            
            await context.Invoke();

            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            var grainName = grainType.Name;
            var methodName = context.ImplementationMethod.Name;

            this.logger.Debug(
                "{grainName} {methodName}. {elapsedMilliseconds}ms. RequestContext.ActivityId {activityId} with PropagateActivityId {propagateActivityId}", 
                grainName, methodName, elapsedMilliseconds,
                RequestContext.ActivityId, RequestContext.PropagateActivityId);
        }
        else
        {
            await context.Invoke();
        }
    }
}