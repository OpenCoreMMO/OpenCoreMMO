using MediatR;
using Microsoft.AspNetCore.Mvc;
using NeoServer.Web.API.Requests.Commands;
using NeoServer.Web.API.Requests.Queries;
using NeoServer.Web.API.Response.Constants;

namespace NeoServer.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorldController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] GetWorldsRequest request)
        => Ok(await mediator.Send(request));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorldByIdAsync([FromRoute] int id)
    {
        var response = await mediator.Send(new GetWorldByIdRequest{ Id = id});
        if (response is null) return NotFound();
        
        return Ok(response);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorldByIdAsync([FromRoute] int id)
    {
        var response = await mediator.Send(new DeleteWorldCommandRequest{ Id = id});
        if (!response.IsSuccess) 
            return UnprocessableEntity(response.ErrorMessage);
        
        return Ok(SuccessMessage.WorldDeleted);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateWorldAsync([FromBody] CreateWorldRequest request)
    {
        var response = await mediator.Send(request);
        if (!response.IsSuccess) 
            return UnprocessableEntity(response.ErrorMessage);
        
        return Ok(SuccessMessage.WorldCreated);
    }
}