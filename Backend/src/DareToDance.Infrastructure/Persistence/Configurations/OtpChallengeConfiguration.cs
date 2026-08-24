using DareToDance.Domain.OtpChallenge;
using DareToDance.Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DareToDance.Infrastructure.Persistence.Configurations;

public sealed class OtpChallengeConfiguration : IEntityTypeConfiguration<OtpChallenge>
{
    public void Configure(EntityTypeBuilder<OtpChallenge> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.CodeHash)
            .HasMaxLength(128)
            .IsRequired();

        // At most one live challenge per user and purpose, enforced by the
        // database. The filter must stay in sync with what handlers treat as
        // "live": not consumed and not invalidated (expiry is deliberately
        // NOT part of the filter — expired rows must still be invalidated,
        // or they would occupy the slot forever).
        builder.HasIndex(c => new { c.UserId, c.Purpose })
            .IsUnique()
            .HasFilter("consumed_at_utc IS NULL AND invalidated_at_utc IS NULL")
            .HasDatabaseName("ix_otp_challenges_user_id_purpose_active");

        // The partial index above cannot serve cooldown/daily-cap lookups.
        builder.HasIndex(c => c.UserId);

        // Postgres xmin as optimistic concurrency token: without it, parallel
        // wrong-code verifies overwrite each other's failed_attempts increment
        // and the attempt cap can be bypassed.
        builder.Property<uint>("xmin")
            .IsRowVersion();
    }
}
