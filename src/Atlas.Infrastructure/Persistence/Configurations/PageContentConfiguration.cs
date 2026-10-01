using Atlas.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class PageContentConfiguration : IEntityTypeConfiguration<PageContent>
{
    public void Configure(EntityTypeBuilder<PageContent> builder)
    {
        builder.ToTable("PageContents");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Key).HasConversion<string>().HasMaxLength(32).IsUnicode(false);
        builder.Property(p => p.Language).HasConversion<string>().HasMaxLength(16).IsUnicode(false);
        builder.HasIndex(p => new { p.Key, p.Language }).IsUnique();

        builder.Property(p => p.Title).HasMaxLength(PageContent.TitleMaxLength).IsRequired();
        builder.Property(p => p.Subtitle).HasMaxLength(PageContent.SubtitleMaxLength);
        builder.Property(p => p.Body).HasMaxLength(PageContent.BodyMaxLength);
        builder.Property(p => p.CallToActionText).HasMaxLength(PageContent.CallToActionTextMaxLength);
        builder.Property(p => p.CallToActionUrl).HasMaxLength(PageContent.CallToActionUrlMaxLength);
        builder.Property(p => p.MetaDescription).HasMaxLength(PageContent.MetaDescriptionMaxLength);
    }
}
