using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Prospera.API.Extensions;
using Prospera.API.Middleware;
using Prospera.Application;
using Prospera.Infrastructure;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/prospera-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

try
{
    Log.Information("Starting Prospera API");

    // Add services to the container
    builder.Services.AddApiServices();
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    var jwtSection = builder.Configuration.GetSection("Jwt");
    var signingKey = jwtSection["Key"] ?? "your-secret-key-here";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"] ?? "Prospera.API",
            ValidAudience = jwtSection["Audience"] ?? "Prospera.Client",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
        };
    });

    builder.Services.AddAuthorization();

    // Add Serilog
    builder.Host.UseSerilog();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage(); // Show detailed errors in development
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Prospera API v1");
            c.RoutePrefix = "swagger"; // Swagger at /swagger
        });
    }

    // Custom middleware
    app.UseRequestLogging();

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Prospera API is running");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Prospera API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
