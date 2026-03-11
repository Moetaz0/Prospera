using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Prospera.Application.Common.Interfaces;
using Prospera.Domain.Entities;
using Prospera.Domain.Events;
using Prospera.Domain.Interfaces;
using Prospera.Infrastructure.Persistence.Interceptors;

namespace Prospera.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly AuditableEntityInterceptor _auditableEntityInterceptor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        AuditableEntityInterceptor auditableEntityInterceptor)
        : base(options)
    {
        _auditableEntityInterceptor = auditableEntityInterceptor;
    }

    // These DbSet properties satisfy the IQueryable interface contract
    public DbSet<User> Users => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Liability> Liabilities => Set<Liability>();
    public DbSet<InvestmentRecommendation> InvestmentRecommendations => Set<InvestmentRecommendation>();

    // Explicit interface implementation to return IQueryable
    IQueryable<User> IApplicationDbContext.Users => Users;
    IQueryable<Transaction> IApplicationDbContext.Transactions => Transactions;
    IQueryable<Asset> IApplicationDbContext.Assets => Assets;
    IQueryable<Liability> IApplicationDbContext.Liabilities => Liabilities;
    IQueryable<InvestmentRecommendation> IApplicationDbContext.InvestmentRecommendations => InvestmentRecommendations;


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // MongoDB configuration is handled in Program.cs via AddDbContext
            optionsBuilder.AddInterceptors(_auditableEntityInterceptor);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ignore abstract DomainEvent class - it shouldn't be mapped as an entity
        modelBuilder.Ignore<DomainEvent>();

        // Ignore domain events collections on aggregate roots
        modelBuilder.Entity<User>().Ignore(u => u.DomainEvents);
        modelBuilder.Entity<Asset>().Ignore(a => a.DomainEvents);
        modelBuilder.Entity<Liability>().Ignore(l => l.DomainEvents);
        modelBuilder.Entity<Transaction>().Ignore(t => t.DomainEvents);
        modelBuilder.Entity<InvestmentRecommendation>().Ignore(ir => ir.DomainEvents);

        // Ignore Portfolio entity entirely (not needed for MongoDB)
        modelBuilder.Ignore<Portfolio>();

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}