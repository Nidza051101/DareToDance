using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NotificationRecordEntity = NotificationService.Domain.NotificationRecord.NotificationRecord;

namespace NotificationService.Infrastructure.Persistence;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<NotificationRecordEntity> NotificationRecords => Set<NotificationRecordEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationRecordEntity>(builder =>
        {
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Recipient).IsRequired().HasMaxLength(320);
            builder.Property(n => n.Template).IsRequired().HasMaxLength(200);
            builder.Property(n => n.Channel).HasConversion<string>().HasMaxLength(20);
            builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(20);

            builder.Property(n => n.Variables)
                .IsRequired()
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null)
                        ?? new Dictionary<string, string>());

            builder.Property(n => n.CreatedAtUtc).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}

