using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NeoServer.Web.Shared.Exceptions;

namespace NeoServer.Web.API.HttpFilters;

public class ExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ExceptionFilter> _logger;

    public ExceptionFilter(ILogger<ExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "{Message}", context.Exception.Message);

        if (context.Exception is NeoException ex)
        {
            var response = new
            {
                Success = false,
                Data = ex.CustomData == null ? new { } : ex.CustomData,
                Errors = new List<string> { ex.Message }
            };

            context.Result = new ObjectResult(response) { StatusCode = (int)ex.HttpStatusCode };
        }
        else
        {
            var response = new
            {
                Success = false,
                Data = new { },
                Errors = new List<string> { context.Exception.Message }
            };

            context.Result = new ObjectResult(response) { StatusCode = (int)HttpStatusCode.InternalServerError };
        }
    }
}