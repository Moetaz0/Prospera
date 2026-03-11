


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prospera.Application.Common.Interfaces;
using Prospera.Domain.Interfaces;
using Prospera.Infrastructure.Caching;
using Prospera.Infrastructure.ExternalServices.AI;
using Prospera.Infrastructure.ExternalServices.Banking;
using Prospera.Infrastructure.ExternalServices.MarketData;
using Prospera.Infrastructure.Identity;
using Prospera.Infrastructure.Persistence;
using Prospera.Infrastructure.Persistence.Interceptors;
using Prospera.Infrastructure.Persistence.Repositories;
using Prospera.Infrastructure.Services;
using StackExchange.Redis;

namespace Prospera.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Interceptors
        services.AddScoped<AuditableEntityInterceptor>();

        // Database - MongoDB
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<AuditableEntityInterceptor>();

            options.UseMongoDB(
                configuration.GetConnectionString("DefaultConnection"),
                "ProsperyDb")
                .AddInterceptors(interceptor);
        });

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Repositories - FIX: Use actual implementation classes
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<ILiabilityRepository, LiabilityRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IInvestmentRecommendationRepository, InvestmentRecommendationRepository>();
        

        // Services
        services.AddTransient<IDateTime, DateTimeService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddScoped<FinancialAnalysisService>();

        // Identity & Authentication
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IIdentityService, IdentityService>();

        // Caching - Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConnection = configuration["Redis:Configuration"] ?? "localhost:6379";
            return ConnectionMultiplexer.Connect(redisConnection);
        });
        services.AddSingleton<ICacheService, RedisCacheService>();

        // HTTP Client - Required for external services
        services.AddHttpClient();

        // External Services
        services.AddScoped<Prospera.Infrastructure.ExternalServices.MarketData.IMarketDataService, AlphaVantageService>();
        services.AddScoped<ILlmService, OllamaService>();
        services.AddScoped<IStripeService, StripeService>();

        return services;
    }
}