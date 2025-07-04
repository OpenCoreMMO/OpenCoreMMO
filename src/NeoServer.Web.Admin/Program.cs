using MudBlazor.Services;
using NeoServer.Shared.IoC.Modules;
using NeoServer.Web.Admin;
using NeoServer.Web.Admin.Components;
using NeoServer.Web.API.IoC.Modules;
using NeoServer.Web.API.Requests.Validators;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddServicesApi();
builder.Services.AddAutoMapperProfiles();

builder.Services.AddLogger(configuration);
builder.Services.AddDatabases(configuration);
builder.Services.AddRepositories();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(NeoServer.Web.API.Program).Assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
});


builder.Services.AddScoped<ProgressBarState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();