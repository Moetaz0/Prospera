using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Prospera.Application.Common.Behaviors;
using Prospera.Application.Common.Interfaces;

namespace Prospera.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Adds Application layer services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // Register validation behavior
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // Register AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Register FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }

    /// <summary>
    /// Adds Application layer services with configuration
    /// </summary>
    public static IServiceCollection AddApplicationServicesWithConfiguration(
        this IServiceCollection services, 
        string ollamaUrl = "http://localhost:11434",
        string ollamaModel = "llama3.2")
    {
        // First register basic services
        services.AddApplicationServices();

        // Register Ollama configuration
        services.AddSingleton<IOllamaConfiguration>(new OllamaConfiguration(ollamaUrl, ollamaModel));

        return services;
    }
}
