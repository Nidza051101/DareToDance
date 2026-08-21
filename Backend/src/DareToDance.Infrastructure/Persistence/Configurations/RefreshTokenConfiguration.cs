using DareToDance.Domain.RefreshToken;
using DareToDance.Domain.RefreshToken.Id;
using DareToDance.Domain.User.Id;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DareToDance.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => RefreshTokenId.Create(value));

        builder.Property(rt => rt.UserId)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value));

        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("ix_refresh_tokens_user_id");

        builder.Property(rt => rt.TokenHash)
            .HasMaxLength(200)
            .IsRequired();

        // Hash je jedinstven - to nam je i glavna putanja za pretragu kad stigne refresh zahtev.
        builder.HasIndex(rt => rt.TokenHash)
            .IsUnique()
            .HasDatabaseName("ix_refresh_tokens_token_hash");

        builder.Property(rt => rt.CreatedAtUtc)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAtUtc)
            .IsRequired();

        builder.Property(rt => rt.RevokedAtUtc);

        builder.Property(rt => rt.ReplacedByTokenId)
            .HasConversion(
                id => ReferenceEquals(id, null) ? (Guid?)null : id.Value,
                value => value != null ? RefreshTokenId.Create(value.Value) : null);

        builder.Ignore(rt => rt.DomainEvents);
    }
}
