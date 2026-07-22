using bow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bow.Infrastructure.Persistence.Configurations;

public sealed class UserWordProgressConfiguration : IEntityTypeConfiguration<UserWordProgress>
{
    public void Configure(EntityTypeBuilder<UserWordProgress> builder)
    {
        builder.ToTable("user_word_progresses");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.WordId })
            .IsUnique();
        builder.Property(x => x.Stage)
            .HasConversion<string>()
            .HasMaxLength(30);
        builder.HasIndex(x => new { x.UserId, x.NextReviewAt});
    }
}