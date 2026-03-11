using StackExchange.Redis;

namespace Prospera.Infrastructure.Services;

public class RedisHealthCheck
{
    private readonly IConnectionMultiplexer _redis;

    public RedisHealthCheck(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _redis.GetDatabase();
            await database.PingAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}