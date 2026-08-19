using DareToDance.Domain.PermissionEntity;
using DareToDance.Domain.PermissionEntity.Id;
using DareToDance.Domain.User;
using DareToDance.Domain.User.Id;
using DareToDance.Domain.UserPermission;
using DareToDance.Domain.UserPermission.Id;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DareToDance.Infrastructure.Persistence.Configurations;

public sealed class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.HasKey(up => up.Id);

        builder.Property(up => up.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => UserPermissionId.Create(value));

        // Nema navigacionih propertija ka User/Permission u modelu - agregat
        // referencira druge agregate samo preko Id-a. Relacija (za FK/cascade
        // u bazi) se ipak definise ovde, preko HasOne<T>() bez navigacije.
        builder.Property(up => up.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value));

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(up => up.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(up => up.PermissionId)
            .HasConversion(
                id => id.Value,
                value => PermissionId.Create(value));

        builder.HasOne<Permission>()
            .WithMany()
            .HasForeignKey(up => up.PermissionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(up => new { up.UserId, up.PermissionId })
            .IsUnique();

        builder.Property(up => up.CreatedAtUtc)
            .IsRequired();

        builder.Property(up => up.UpdatedAtUtc)
            .IsRequired();

        builder.Ignore(up => up.DomainEvents);
    }
}
