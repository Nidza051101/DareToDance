using DareToDance.Domain.LoginCode;
using DareToDance.Domain.PermissionEntity;
using DareToDance.Domain.User;
using DareToDance.Domain.UserPermission;
using Microsoft.EntityFrameworkCore;

namespace DareToDance.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<LoginCode> LoginCodes => Set<LoginCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
