using bow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bow.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TelegramId)
            .IsUnique();
        builder.Property(x => x.DisplayName)
            .HasMaxLength(200);
        builder.Property(x => x.NativeLanguage)
            .HasConversion<string>()
            .HasMaxLength(2);
        builder.Property(x => x.LearningLanguage)
            .HasConversion<string>()
            .HasMaxLength(2);
        builder.Property(x => x.LearningLevel)
            .HasConversion<string>()
            .HasMaxLength(2);

        builder.HasMany(x => x.UserWordProgresses)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);
    }
}