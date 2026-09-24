using bow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bow.Infrastructure.Persistence.Configurations;

public sealed class TelegramAccountConfiguration : IEntityTypeConfiguration<TelegramAccount>
{
    public void Configure(EntityTypeBuilder<TelegramAccount> builder)
    {
        builder.ToTable("telegram_accounts");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TelegramId)
            .IsUnique();
        builder.HasIndex(x => x.UserId)
            .IsUnique();
        builder.Property(x => x.Name)
            .HasMaxLength(200);
        builder.Property(x => x.TelegramId)
            .IsRequired();
    }
}