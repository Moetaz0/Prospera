using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prospera.Application.Common.Interfaces;

namespace Prospera.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that refreshes inflation rates from World Bank quarterly
/// Runs once per day but only updates rates if needed (quarterly World Bank updates)
/// </summary>
public class QuarterlyRatesRefreshBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private static DateTime _lastRefreshCheck = DateTime.MinValue;
    private const int CHECK_INTERVAL_HOURS = 24; // Check daily if refresh is needed

    public QuarterlyRatesRefreshBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial delay to let application start up (3 minutes)
        await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Check if enough time has passed since last refresh attempt
                if ((DateTime.UtcNow - _lastRefreshCheck).TotalHours >= CHECK_INTERVAL_HOURS)
                {
                    _lastRefreshCheck = DateTime.UtcNow;
                    
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var ratesService = scope.ServiceProvider.GetRequiredService<IExternalRatesService>();
                        
                        // Refresh all inflation rates from World Bank
                        // This respects admin overrides (won't override custom rates)
                        await ratesService.RefreshInflationRatesAsync();
                    }
                }

                // Check again in 1 hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (Exception ex)
            {
                // Log error but don't crash the service
                // In production: _logger.LogError($"Error in quarterly rates refresh: {ex.Message}");
                
                // Wait before retry
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
