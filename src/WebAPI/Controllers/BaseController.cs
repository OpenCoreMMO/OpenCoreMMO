using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NeoServer.Web.API.Response;
using NeoServer.Web.Shared.Exceptions;

namespace NeoServer.Web.API.Controllers;

[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    #region protected methods implementation

    protected new IActionResult Response(object result = null)
    {
        if (result != null)
            return Ok(new BaseResponseViewModel
            {
                Success = true,
                Data = result,
                Errors = new List<string>()
            });
        return BadRequest(new
        {
            Success = false,
            Data = new { },
            Errors = new List<string>()
        });
    }

    protected new IActionResult Response(object result, ModelStateDictionary modelState)
    {
        if (result != null)
            return Ok(new BaseResponseViewModel
            {
                Success = true,
                Data = result,
                Errors = new List<string>()
            });

        var modelErrors = new List<string>();

        if (modelState is null || ModelState.IsValid)
            return BadRequest(new
            {
                Success = false,
                Data = new { },
                Errors = modelErrors
            });

        foreach (var state in ModelState.Values)
            modelErrors.AddRange(state.Errors.Select(modelError => modelError.ErrorMessage));

        return BadRequest(new
        {
            Success = false,
            Data = new { },
            Errors = modelErrors
        });
    }

    protected new IActionResult Response(object result, List<string> modelErrors)
    {
        if (result != null)
            return Ok(new BaseResponseViewModel
            {
                Success = true,
                Errors = new List<string>()
            });

        return BadRequest(new
        {
            Success = false,
            Data = new { },
            Errors = modelErrors
        });
    }

    protected new IActionResult Response(object result, string error)
    {
        if (result != null)
            return Ok(new BaseResponseViewModel
            {
                Success = true,
                Errors = new List<string>()
            });

        return BadRequest(new
        {
            Success = false,
            Data = new { },
            Errors = new List<string> { error }
        });
    }

    protected IActionResult ExceptionResponse(NeoException ex)
    {
        return StatusCode((int)ex.HttpStatusCode, new
        {
            Success = false,
            Data = new { },
            Errors = new List<string> { ex.Message }
        });
    }

    #endregion protected methods implementation
}