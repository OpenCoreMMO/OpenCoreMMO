using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NeoServer.Web.API.HttpFilters;

public class SwaggerJsonIgnoreFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var ignoredProperties = context.MethodInfo.GetParameters()
            .SelectMany(p => p.ParameterType.GetProperties()
                .Where(prop => prop.GetCustomAttribute<JsonIgnoreAttribute>() != null))
            .ToList();

        if (ignoredProperties.Count == 0) return;

        foreach (var property in ignoredProperties)
            operation.Parameters = operation.Parameters
                .Where(p => !p.Name.Equals(property.Name, StringComparison.InvariantCulture))
                .ToList();
    }
}