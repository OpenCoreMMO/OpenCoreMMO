using System.Net;
using MudBlazor.Services;
using NeoServer.Shared.IoC.Modules;
using NeoServer.Web.Admin;
using NeoServer.Web.Admin.Components;
using NeoServer.Web.API.IoC.Modules;
using NeoServer.Web.API.Requests.Validators;
using NeoServer.Web.Shared.IoC.Modules;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var environment = builder.Environment;

// Configure appsettings
builder.Configuration
    .SetBasePath(environment.ContentRootPath)
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true)
    .AddEnvironmentVariables();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddServicesApi();
builder.Services.AddAutoMapperProfiles(typeof(Program).Assembly);

builder.Services.AddLogger(configuration);
builder.Services.AddDatabases(configuration);
builder.Services.AddRepositories();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardLimit = 2;
    options.KnownProxies.Add(IPAddress.Parse("127.0.0.1"));
    options.KnownProxies.Add(IPAddress.Parse("0.0.0.0"));
    options.ForwardedForHeaderName = "X-Forwarded-For-My-Custom-Header-Name";
});

builder.Services.AddCors();

builder.Services.AddScoped<ProgressBarState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();