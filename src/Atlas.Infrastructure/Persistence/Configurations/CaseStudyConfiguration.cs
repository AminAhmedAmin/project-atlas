using Atlas.Domain.Portfolio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class CaseStudyConfiguration : IEntityTypeConfiguration<CaseStudy>
{
    public void Configure(EntityTypeBuilder<CaseStudy> builder)
    {
        builder.ToTable("CaseStudies");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Language).HasConversion<string>().HasMaxLength(16).IsUnicode(false);
        builder.Property(c => c.Slug).HasMaxLength(CaseStudy.SlugMaxLength).IsUnicode(false).IsRequired();
        builder.Property(c => c.Title).HasMaxLength(CaseStudy.TitleMaxLength).IsRequired();
        builder.Property(c => c.Client).HasMaxLength(CaseStudy.ClientMaxLength);
        builder.Property(c => c.Summary).HasMaxLength(CaseStudy.SummaryMaxLength).IsRequired();
        builder.Property(c => c.Highlights).HasMaxLength(CaseStudy.HighlightsMaxLength);
        builder.Property(c => c.Body).HasMaxLength(CaseStudy.BodyMaxLength);
        builder.Property(c => c.Tags).HasMaxLength(CaseStudy.TagsMaxLength);
        builder.Property(c => c.CoverImageUrl).HasMaxLength(CaseStudy.ImageUrlMaxLength);

        builder.Ignore(c => c.TagList);
        builder.Ignore(c => c.HighlightList);

        builder.HasIndex(c => new { c.Language, c.Slug }).IsUnique();
        builder.HasIndex(c => new { c.Language, c.IsPublished, c.DisplayOrder });
    }
}
