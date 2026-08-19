using DareToDance.Domain.User;
using DareToDance.Domain.User.Id;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DareToDance.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value));

        builder.Property(u => u.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Phone)
            .HasMaxLength(30);

        builder.HasIndex(u => u.Phone)
            .IsUnique()
            .HasDatabaseName("ix_users_phone");

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.UserRole)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.CreatedAtUtc)
            .IsRequired();

        builder.Property(u => u.UpdatedAtUtc)
            .IsRequired();

        builder.Ignore(u => u.DomainEvents);

        builder.HasData(new
        {
            Id = UserId.Create(
                new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3301")),

            FirstName = "Nikola",
            LastName = "Andric",
            Email = "nikolaandricw@gmail.com",
            Phone = "0641059679",
            Status = UserStatus.Active,
            UserRole = UserRole.Admin,
            CreatedAtUtc = new DateTime(
                2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(
                2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
