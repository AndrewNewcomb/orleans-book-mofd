using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using OrleansBook.GrainInterfaces;
using OrleansBook.WebApi.Models;

namespace OrleansBook.WebApi.Controllers;

[ApiController]
public class BatchController : ControllerBase
{
    private readonly IClusterClient _client;

    public BatchController(IClusterClient client)
    {
        _client = client;
    }

    [HttpPost]
    [Route("batch")]
    public async Task<IActionResult> Post(IDictionary<string, string> values)
    {
        var grain = _client.GetGrain<IBatchGrain>(0);

        var instructions = values
            .Select(kv => (kv.Key, kv.Value))
            .ToArray();

        await grain.AddInstructions(instructions);

        return Ok();
    }
}