using Microsoft.OpenApi.Models;
using Prospera.API.Filters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Prospera.API.Extensions;

/// <summary>
/// Extension methods for registering API services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds API-specific services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // Add controllers with global exception filter
        services.AddControllers(options =>
        {
            options.Filters.Add<ApiExceptionFilter>();
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.WriteIndented = false;
        });

        // API explorer for Swagger generation
        services.AddEndpointsApiExplorer();

        // Add Swagger/OpenAPI
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Prospera FinTech API",
                Version = "v1",
                Description = "RESTful API for Prospera - AI-powered financial planning system",
                Contact = new OpenApiContact
                {
                    Name = "Prospera Support",
                    Email = "support@prospera.ai"
                },
                License = new OpenApiLicense
                {
                    Name = "Proprietary"
                }
            });

            // Add JWT Bearer authentication definition
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });

            // Add XML documentation
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }
}
