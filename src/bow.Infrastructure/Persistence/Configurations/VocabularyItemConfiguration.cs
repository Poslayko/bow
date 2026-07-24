using bow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bow.Infrastructure.Persistence.Configurations;

public sealed class VocabularyItemConfiguration : IEntityTypeConfiguration<VocabularyItem>
{
    public void Configure(EntityTypeBuilder<VocabularyItem> builder)
    {
        builder.ToTable("vocabulary_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Text)
            .HasMaxLength(200);
        builder.Property(x => x.NormalizedText)
            .HasMaxLength(200);
        builder.HasIndex(x => new { x.Language, x.NormalizedText})
            .IsUnique();
        builder.Property(x => x.Language)
            .HasConversion<string>()
            .HasMaxLength(2);
        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasMany(x => x.SourceTranslations)
            .WithOne(x => x.TranslationFrom)
            .HasForeignKey(x => x.TranslationFromId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.TargetTranslations)
            .WithOne(x => x.TranslationTo)
            .HasForeignKey(x => x.TranslationToId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.UserVocabularyItemProgresses)
            .WithOne(x => x.VocabularyItem)
            .HasForeignKey(x => x.VocabularyItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.SourceTranslations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.TargetTranslations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.UserVocabularyItemProgresses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}