using DareToDance.Domain.RefreshToken;
using DareToDance.Domain.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DareToDance.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // SHA-256 digest as base64 is 44 chars; 64 leaves headroom.
        builder.Property(t => t.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        // No partial unique index here, unlike otp_challenges: a user holds one
        // live token per signed-in device, so several live rows are legal.
        builder.HasIndex(t => t.UserId);

        // Reuse detection and logout revoke by family.
        builder.HasIndex(t => t.FamilyId);

        // Postgres xmin as optimistic concurrency token: two parallel refreshes
        // of the same token must produce exactly one successor — the loser's
        // SaveChanges fails instead of silently double-rotating.
        builder.Property<uint>("xmin")
            .IsRowVersion();
    }
}
