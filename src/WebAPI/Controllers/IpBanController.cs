using MediatR;
using Microsoft.AspNetCore.Mvc;
using NeoServer.Web.API.Requests.Commands;
using NeoServer.Web.API.Requests.Queries;
using NeoServer.Web.API.Response.Constants;

namespace NeoServer.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IpBanController(IMediator mediator) : BaseController
{
    //todo: The permission to ban an account should be restricted to a specific role "admin"
    [HttpGet("{ip}")]
    public async Task<IActionResult> FindByIp([FromRoute] string ip)
    {
        var result =  await mediator.Send(new GetIpBanByIpRequest(ip));
        if (result is null) return NotFound();
        
        return Ok(result);
    }
    
    //todo: The permission to ban an account should be restricted to a specific role "admin"
    [HttpPost]
    public async Task<IActionResult> BanIp([FromBody] BanIpRequest request)
    {
        var result =  await mediator.Send(request);
        if (!result.IsSuccess)
            return UnprocessableEntity(result.ErrorMessage);
        
        return Ok(SuccessMessage.IpBanned);
    }
}