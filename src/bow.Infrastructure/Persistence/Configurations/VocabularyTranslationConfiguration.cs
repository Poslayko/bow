using bow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bow.Infrastructure.Persistence.Configurations;

public sealed class VocabularyTranslationConfiguration : IEntityTypeConfiguration<VocabularyTranslation>
{
    public void Configure(EntityTypeBuilder<VocabularyTranslation> builder)
    {
        builder.ToTable("vocabulary_translations");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TranslationFromId, x.TranslationToId})
            .IsUnique();
        builder.Property(x => x.Level)
            .HasConversion<string>()
            .HasMaxLength(2);
    }
}