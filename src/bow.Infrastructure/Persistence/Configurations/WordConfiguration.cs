using bow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bow.Infrastructure.Persistence.Configurations;

public sealed class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.ToTable("words");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Text)
            .HasMaxLength(100);
        builder.HasIndex(x => x.Text);
        builder.HasIndex(x => new { x.Language, x.Level, x.Text});
        builder.Property(x => x.Language)
            .HasConversion<string>()
            .HasMaxLength(2);
        builder.Property(x => x.Level)
            .HasConversion<string>()
            .HasMaxLength(2);

        builder.HasMany(x => x.SourceTranslations)
            .WithOne(x => x.SourceWord)
            .HasForeignKey(x => x.SourceWordId);
        builder.HasMany(x => x.TargetTranslations)
            .WithOne(x => x.TargetWord)
            .HasForeignKey(x => x.TargetWordId);
        builder.HasMany(x => x.UserWordProgresses)
            .WithOne(x => x.Word)
            .HasForeignKey(x => x.WordId);
    }
}