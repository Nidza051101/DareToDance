using DareToDance.Domain.LoginCode;
using DareToDance.Domain.LoginCode.Id;
using DareToDance.Domain.User;
using DareToDance.Domain.User.Id;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DareToDance.Infrastructure.Persistence.Configurations;

public sealed class LoginCodeConfiguration : IEntityTypeConfiguration<LoginCode>
{
    public void Configure(EntityTypeBuilder<LoginCode> builder)
    {
        builder.HasKey(lc => lc.Id);

        builder.Property(lc => lc.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => LoginCodeId.Create(value));

        // Nema navigacionog propertija ka User u modelu - agregat referencira
        // drugi agregat samo preko Id-a. Relacija (za FK/cascade u bazi) se
        // ipak definise ovde, preko HasOne<T>() bez navigacije.
        builder.Property(lc => lc.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value));

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(lc => lc.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(lc => lc.Channel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(lc => lc.CodeHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(lc => lc.ExpiresAtUtc)
            .IsRequired();

        builder.Property(lc => lc.ConsumedAtUtc);

        builder.Property(lc => lc.FailedAttempts)
            .IsRequired();

        builder.HasIndex(lc => new { lc.UserId, lc.ConsumedAtUtc });

        builder.Property(lc => lc.CreatedAtUtc)
            .IsRequired();

        builder.Property(lc => lc.UpdatedAtUtc)
            .IsRequired();

        builder.Ignore(lc => lc.DomainEvents);
    }
}
