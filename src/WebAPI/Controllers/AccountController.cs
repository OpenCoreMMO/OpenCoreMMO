using MediatR;
using Microsoft.AspNetCore.Mvc;
using NeoServer.Web.API.Requests.Commands;
using NeoServer.Web.API.Response.Constants;

namespace NeoServer.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IMediator mediator) : BaseController
{
    //todo: The permission to ban an account should be restricted to a specific role "admin or user by website"
    [HttpPost]
    public async Task<IActionResult> Post(CreateAccountRequest request)
    {
        var result =  await mediator.Send(request);

        if (!result.IsSuccess)
            return UnprocessableEntity(result.ErrorMessage);
        
        return Ok(SuccessMessage.AccountCreated.Replace("{id}", result.Identifier.ToString()));
    }
    
    //todo: The permission to ban an account should be restricted to a specific role "admin"
    [HttpPatch("{accountId}/premium")]
    public async Task<IActionResult>  AddPremium([FromRoute] int accountId, [FromBody] AddPremioumDaysAccountRequest request)
    {
        request.SetAccountId(accountId);

        var result =  await mediator.Send(request);

        if (!result.IsSuccess)
            return UnprocessableEntity(result.ErrorMessage);
        
        return Ok(SuccessMessage.AddedPremiumDays);
    }
    
    //todo: The permission to ban an account should be restricted to a specific role "admin"
    [HttpPatch("{accountId}/ban")]
    public async Task<IActionResult> Ban([FromRoute] int accountId, [FromBody] BanAccountRequest request)
    {
        request.SetAccountId(accountId);

        var result =  await mediator.Send(request);

        if (!result.IsSuccess)
            return UnprocessableEntity(result.ErrorMessage);
        
        return Ok(SuccessMessage.AccountBanned);
    }
    
    //todo: The permission to ban an account should be restricted to a specific role "user by website"
    [HttpPatch("{accountId}/change-password")]
    public async Task<IActionResult> Ban([FromRoute] int accountId, [FromBody] ChangePasswordRequest request)
    {
        request.SetAccountId(accountId);

        var result =  await mediator.Send(request);

        if (!result.IsSuccess)
            return UnprocessableEntity(result.ErrorMessage);
        
        return Ok(SuccessMessage.PasswordChanged);
    }
}