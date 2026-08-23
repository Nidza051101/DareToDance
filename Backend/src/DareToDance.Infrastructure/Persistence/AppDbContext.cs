using DareToDance.Domain.Common;
using DareToDance.Domain.OtpChallenge;
using DareToDance.Domain.OtpChallenge.Id;
using DareToDance.Domain.RefreshToken;
using DareToDance.Domain.RefreshToken.Id;
using DareToDance.Domain.User;
using DareToDance.Domain.User.Id;
using DareToDance.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DareToDance.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<OtpChallenge> OtpChallenges => Set<OtpChallenge>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();



    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }


    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Typed IDs convert globally: every property of these CLR types — keys
        // and foreign keys alike — maps to uuid without per-config wiring.
        configurationBuilder.Properties<UserId>().HaveConversion<UserIdConverter>();
        configurationBuilder.Properties<OtpChallengeId>().HaveConversion<OtpChallengeIdConverter>();
        configurationBuilder.Properties<RefreshTokenId>().HaveConversion<RefreshTokenIdConverter>();

        configurationBuilder.Properties<OtpPurpose>().HaveConversion<string>().HaveMaxLength(20);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Domain-event lists are in-memory only — never mapped.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList()
                     .Where(t => typeof(IHasDomainEvent).IsAssignableFrom(t.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType).Ignore(nameof(IHasDomainEvent.DomainEvents));
        }

        base.OnModelCreating(modelBuilder);
    }
}

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder
            .UseNpgsql("Host=localhost;Port=5433;Database=daretodance;Username=daretodance;Password=daretodance")
            .UseSnakeCaseNamingConvention();
        return new AppDbContext(optionsBuilder.Options);
    }
}