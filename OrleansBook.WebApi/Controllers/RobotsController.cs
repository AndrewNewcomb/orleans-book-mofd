using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using OrleansBook.GrainInterfaces;
using OrleansBook.WebApi.Models;

namespace OrleansBook.WebApi.Controllers;

[ApiController]
public class RobotsController : ControllerBase
{
    private readonly IClusterClient _client;

    public RobotsController(IClusterClient client)
    {
        _client = client;
    }

    [HttpGet]
    [Route("robot/{name}/instruction")]
    public async Task<string?> Get(string name)
    {
        var grain = _client.GetGrain<IRobotGrain>(name, "OrleansBook.GrainClasses.EventSourcedGrain");

        return await grain.GetNextInstruction();
    }

    [HttpPost]
    [Route("robot/{name}/instruction")]
    public async Task<IActionResult> Post(string name, [FromBody]RobotsPostRequest request)
    {
        if(!ModelState.IsValid)
            return BadRequest(ModelState);

        var grain = _client.GetGrain<IRobotGrain>(name, "OrleansBook.GrainClasses.EventSourcedGrain");
        await grain.AddInstruction(request.Instruction);
        return Ok();    
    }

    [HttpGet]
    [Route("robot/{name}/doSomethingSlow/{slowTaskTimeSeconds}/{secondsToWaitBeforeCancelling}")]
    public async Task<string> DoSomethingSlow(string name, int slowTaskTimeSeconds, int secondsToWaitBeforeCancelling)
    {
        var grain = _client.GetGrain<IRobotGrain>(name, "OrleansBook.GrainClasses.EventSourcedGrain");

        using var tcs = new GrainCancellationTokenSource();
        var slowTask = grain.DoSomethingSlow(slowTaskTimeSeconds, tcs.Token);

        var cancelTask = CancelAfterNSeconds(tcs, secondsToWaitBeforeCancelling);
        
        await Task.WhenAll(slowTask, cancelTask);

        var wasCancelled = slowTask.Result;
        return wasCancelled ? "Request to grain was cancelled" : "Request to grain ran to completion";
    }

    private async Task CancelAfterNSeconds(GrainCancellationTokenSource tcs, int secondsToWaitBeforeCancelling)
    {
        await Task.Delay(secondsToWaitBeforeCancelling * 1000);
        await tcs.Cancel();       
    }
}