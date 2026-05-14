namespace Prospera.Contracts.DTOs.Admin;

public class AdminStatsDto
{
    public AdminUserStatsDto UserStats { get; set; } = new();
    public AdminDatabaseStatsDto DatabaseStats { get; set; } = new();
    public AdminCacheStatsDto CacheStats { get; set; } = new();
    public AdminSystemHealthDto SystemHealth { get; set; } = new();
    public AdminFinancialMetricsDto FinancialMetrics { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class AdminUserStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
    public decimal UserGrowthRate { get; set; }
}

public class AdminDatabaseStatsDto
{
    public int QueriesPerSecond { get; set; }
    public decimal AvgQueryTime { get; set; }
    public int ConnectionPoolUsage { get; set; }
    public string DatabaseSize { get; set; } = string.Empty;
}

public class AdminCacheStatsDto
{
    public decimal CacheHitRate { get; set; }
    public string MemoryUsage { get; set; } = string.Empty;
    public int KeysStored { get; set; }
    public decimal EvictionRate { get; set; }
}

public class AdminSystemHealthDto
{
    public decimal ApiUptime { get; set; }
    public decimal AvgResponseTime { get; set; }
    public decimal ErrorRate { get; set; }
    public int RequestsPerSecond { get; set; }
}

public class AdminFinancialMetricsDto
{
    public decimal TotalAssetsManaged { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal AverageNetWorth { get; set; }
    public int TotalTransactions { get; set; }
}
