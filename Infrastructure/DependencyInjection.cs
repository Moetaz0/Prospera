using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prospera.Application.Common.Interfaces;
using Prospera.Domain.Interfaces;
using Prospera.Infrastructure.BackgroundServices;
using Prospera.Infrastructure.Caching;
using Prospera.Infrastructure.ExternalServices.AI;
using Prospera.Infrastructure.ExternalServices.Banking;
using Prospera.Infrastructure.ExternalServices.BVMT;
using Prospera.Infrastructure.ExternalServices.FinancialData;
using Prospera.Infrastructure.ExternalServices.MarketData;
using Prospera.Infrastructure.ExternalServices.News;
using Prospera.Infrastructure.Identity;
using Prospera.Infrastructure.Persistence;
using Prospera.Infrastructure.Persistence.Interceptors;
using Prospera.Infrastructure.Persistence.Repositories;
using Prospera.Infrastructure.Repositories;
using Prospera.Infrastructure.Services;
using Prospera.Infrastructure.Services.AssetValuation;
using Prospera.Infrastructure.Services.LocationBased;
using Prospera.Infrastructure.Services.RatesApi;
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
        services.AddScoped<INewsRepository, NewsRepository>();
        services.AddScoped<ICustomFinancialRateRepository, CustomFinancialRateRepository>();
        services.AddScoped<ICoachingSessionRepository, CoachingSessionRepository>();
        services.AddScoped<IRealEstateEvaluationRepository, RealEstateEvaluationRepository>();
        services.AddScoped<ICarEvaluationRepository, CarEvaluationRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();

        // Services
        services.AddTransient<IDateTime, DateTimeService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddScoped<FinancialAnalysisService>();
        services.AddScoped<IAssetValuationService, AssetValuationService>();
        services.AddScoped<ILocationBasedRatesService, LocationBasedRatesService>();
        services.AddScoped<IExternalRatesService, ExternalRatesService>();
        services.AddScoped<ICoachingService, CoachingService>();
        services.AddMemoryCache(); // For caching World Bank inflation rates

        // Python Financial-API Services
        services.AddHttpClient<ICarValuationApiService, CarValuationApiService>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("http://localhost:8000");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

        services.AddHttpClient<IInflationPredictionApiService, InflationPredictionApiService>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri("http://localhost:8000");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

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

        // LLM Provider Configuration
        var ollamaUrl = configuration["Ollama:Url"] ?? "http://localhost:11434";
        var ollamaModel = configuration["Ollama:Model"] ?? "llama3:latest";
        services.AddSingleton<IOllamaConfiguration>(sp =>
            new OllamaConfiguration(ollamaUrl, ollamaModel));

        var openRouterUrl = configuration["OpenRouter:Url"] ?? "https://openrouter.ai/api/v1";
        var openRouterKey = configuration["OpenRouter:ApiKey"] ?? string.Empty;
        services.AddSingleton<IOpenRouterConfiguration>(sp =>
            new OpenRouterConfiguration(openRouterUrl, openRouterKey, "Prospera", "1.0.0"));

        // LLM Provider Services
        services.AddScoped<OllamaProviderService>();
        services.AddScoped<OpenRouterService>();
        services.AddScoped<ILlmProviderFactory, LlmProviderFactory>();

        // External Services
        services.AddScoped<Prospera.Application.Common.Interfaces.IMarketDataService, AlphaVantageService>();
        services.AddScoped<Prospera.Infrastructure.ExternalServices.MarketData.IMarketDataServiceInfra, AlphaVantageService>();
        services.AddScoped<ILlmService, OllamaService>();
        services.AddScoped<IStripeService, StripeService>();
        services.AddScoped<IFinancialNewsService, NewsApiService>();

        // Financial Data Service - FastAPI integration
        services.AddHttpClient<IFinancialDataService, FinancialDataService>()
            .ConfigureHttpClient(client =>
            {
                var baseUrl = configuration["FinancialDataApi:Url"] ?? "http://localhost:8000/api/v1";
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            });

        // BVMT Service - Tunisian Stock Exchange data
        services.AddHttpClient<IBvmtService, BvmtService>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

        // Background Services
        services.AddHostedService<DailyNewsBackgroundService>();
        services.AddHostedService<QuarterlyRatesRefreshBackgroundService>();

        return services;
    }
}
