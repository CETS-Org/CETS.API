using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ocelot.Provider.Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add Ocelot with Cache Manager for rate limiting and Polly for QoS
builder.Services.AddOcelot()
    .AddCacheManager(x =>
    {
        x.WithDictionaryHandle();
    })
    .AddPolly();

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
        .AllowCredentials()
        .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset"));
});

// Configure Authentication (JWT)
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"])),
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Memory Cache for rate limiting
builder.Services.AddMemoryCache();

var app = builder.Build();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Remove("Server");
    
    if (context.Request.IsHttps)
    {
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }
    
    await next();
});

// Apply CORS before Ocelot middleware
app.UseCors("GatewayCors");

// Authentication middleware
app.UseAuthentication();

// Use Ocelot with configuration
await app.UseOcelot();

app.Run();
