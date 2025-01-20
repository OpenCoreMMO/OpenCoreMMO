using System.Net;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.OpenApi.Models;
using NeoServer.Shared.IoC.Modules;
using NeoServer.Web.API.HttpFilters;
using NeoServer.Web.API.IoC.Modules;
using NeoServer.Web.API.Middlewares;
using NeoServer.Web.API.Requests.Validators;
using NeoServer.Web.API.Swagger.SchemaFilters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.JsonMultipartFormDataSupport.Extensions;
using Swashbuckle.AspNetCore.JsonMultipartFormDataSupport.Integrations;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment;

// Configure appsettings
builder.Configuration
    .SetBasePath(environment.ContentRootPath)
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true)
    .AddEnvironmentVariables();

// Add services to the container
var services = builder.Services;

builder.AddDefaultValuesInjection();
services.AddHttpContextAccessor();
services.AddServicesApi();
services.AddAutoMapperProfiles();

services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

services.AddValidatorsFromAssembly(typeof(Program).Assembly);

services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardLimit = 2;
    options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
    options.KnownProxies.Add(IPAddress.Parse("0.0.0.0"));
    options.ForwardedForHeaderName = "X-Forwarded-For-My-Custom-Header-Name";
});

services.AddSwaggerGen(c =>
{
    c.SchemaFilter<EnumSchemaFilter>();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NeoServer.API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

    c.OperationFilter<SwaggerJsonIgnoreFilter>();
});

services.AddJsonMultipartFormDataSupport(JsonSerializerChoice.Newtonsoft);

services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
    });


services.AddLogger(configuration);
services.AddDatabases(configuration);
services.AddRepositories();

var app = builder.Build();

app.UseMiddleware<ValidationExceptionHandlingMiddleware>();
// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.MapControllers();

app.Run();

namespace NeoServer.Web.API
{
    public class Program
    {
    }
}