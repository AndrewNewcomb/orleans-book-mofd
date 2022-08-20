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
        var grain = _client.GetGrain<IRobotGrain>(name);

        return await grain.GetNextInstruction();
    }

    [HttpPost]
    [Route("robot/{name}/instruction")]
    public async Task<IActionResult> Post(string name, [FromBody]RobotsPostRequest request)
    {
        if(!ModelState.IsValid)
            return BadRequest(ModelState);

        var grain = _client.GetGrain<IRobotGrain>(name);
        await grain.AddInstruction(request.Instruction);
        return Ok();    
    }
}