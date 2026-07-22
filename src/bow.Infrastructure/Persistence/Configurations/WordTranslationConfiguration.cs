using bow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bow.Infrastructure.Persistence.Configurations;

public sealed class WordTranslationConfiguration : IEntityTypeConfiguration<WordTranslation>
{
    public void Configure(EntityTypeBuilder<WordTranslation> builder)
    {
        builder.ToTable("word_translations");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.SourceWordId, x.TargetWordId})
            .IsUnique();
    }
}