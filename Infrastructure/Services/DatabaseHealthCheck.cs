using Microsoft.EntityFrameworkCore;
using Prospera.Infrastructure.Persistence;

namespace Prospera.Infrastructure.Services;

public class DatabaseHealthCheck
{
    private readonly ApplicationDbContext _context;

    public DatabaseHealthCheck(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }
}