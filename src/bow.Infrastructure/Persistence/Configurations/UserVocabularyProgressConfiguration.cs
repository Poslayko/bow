using bow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace bow.Infrastructure.Persistence.Configurations;

public sealed class UserVocabularyProgressConfiguration : IEntityTypeConfiguration<UserVocabularyProgress>
{
    public void Configure(EntityTypeBuilder<UserVocabularyProgress> builder)
    {
        builder.ToTable("user_vocabulary_progresses");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UserId, x.VocabularyItemId })
            .IsUnique();
        builder.Property(x => x.Stage)
            .HasConversion<string>()
            .HasMaxLength(30);
        builder.HasIndex(x => new { x.UserId, x.NextReviewAt});
    }
}