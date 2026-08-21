using DareToDance.Domain.Membership;
using DareToDance.Domain.Membership.Id;
using DareToDance.Domain.User.Id;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DareToDance.Infrastructure.Persistence.Configurations;

public sealed class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => MembershipId.Create(value));

        builder.Property(m => m.UserId)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value));

        builder.HasIndex(m => m.UserId)
            .HasDatabaseName("ix_memberships_user_id");

        builder.Property(m => m.ValidFrom)
            .IsRequired();

        builder.Property(m => m.ValidTo)
            .IsRequired();

        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.CreatedAtUtc)
            .IsRequired();

        builder.Property(m => m.UpdatedAtUtc)
            .IsRequired();

        builder.Ignore(m => m.DomainEvents);
    }
}
