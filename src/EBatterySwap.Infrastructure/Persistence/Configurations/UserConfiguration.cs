using EBatterySwap.Domain.Entities;
using EBatterySwap.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EBatterySwap.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.UserId).HasName("PK_Users");

        builder.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(20).IsUnicode(false);
        builder.Property(x => x.Email).HasMaxLength(254).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Status).HasEnumAsText(20);
        builder.Property(x => x.CreatedAt).HasDateTime3();
        builder.Property(x => x.UpdatedAt).HasDateTime3();

        builder.HasIndex(x => x.Email, "UQ_Users_Email").IsUnique();

        builder.HasOne(x => x.Role)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.RoleId)
            .HasConstraintName("FK_Users_Role")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
