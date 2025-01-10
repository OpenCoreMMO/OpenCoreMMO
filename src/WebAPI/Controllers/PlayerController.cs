using MediatR;
using Microsoft.AspNetCore.Mvc;
using NeoServer.Web.API.Requests.Commands;
using NeoServer.Web.API.Requests.Queries;
using NeoServer.Web.API.Response.Constants;

namespace NeoServer.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] GetPlayersRequest request)
       => Ok(await mediator.Send(request));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var response = await mediator.Send(new GetPlayerByIdRequest { Id = id });
        if (response is null) return NotFound();
        
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreatePlayerRequest request)
    {
        var response = await mediator.Send(request);
        if (!response.IsSuccess) 
            return UnprocessableEntity(response.ErrorMessage);
        
        return Ok(SuccessMessage.PlayerCreated.Replace("{id}", response.Identifier.ToString()));
    }
    
    [HttpPatch("{id}/skills")]
    public async Task<IActionResult> UpdateSkills(int id, [FromBody] UpdatePlayerSkillsRequest request)
    {
        request.SetPlayerId(id);
        var response = await mediator.Send(request);
        if (!response.IsSuccess) 
            return UnprocessableEntity(response.ErrorMessage);
        
        return Ok(SuccessMessage.PlayerSkillsUpdated);
    }
    
    [HttpPatch("{id}/infos")]
    public async Task<IActionResult> UpdateInfos(int id, [FromBody] UpdatePlayerInfosRequest request)
    {
        request.SetPlayerId(id);
        var response = await mediator.Send(request);
        if (!response.IsSuccess) 
            return UnprocessableEntity(response.ErrorMessage);
        
        return Ok(SuccessMessage.PlayerInfosUpdated);
    }
}
