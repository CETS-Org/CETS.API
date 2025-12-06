using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot();

// Configure CORS for external clients
var allowedOrigins = builder.Configuration
    .GetSection("AllowedCorsOrigins")
    .Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayCors", policy => policy
        .WithOrigins(allowedOrigins!)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

// Apply CORS before Ocelot middleware
app.UseCors("GatewayCors");

await app.UseOcelot();
app.Run();
