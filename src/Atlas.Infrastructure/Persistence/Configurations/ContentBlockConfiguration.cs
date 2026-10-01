using Atlas.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class ContentBlockConfiguration : IEntityTypeConfiguration<ContentBlock>
{
    public void Configure(EntityTypeBuilder<ContentBlock> builder)
    {
        builder.ToTable("ContentBlocks");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();

        builder.Property(b => b.Kind).HasConversion<string>().HasMaxLength(32).IsUnicode(false);
        builder.Property(b => b.Title).HasMaxLength(ContentBlock.TitleMaxLength).IsRequired();
        builder.Property(b => b.Subtitle).HasMaxLength(ContentBlock.SubtitleMaxLength);
        builder.Property(b => b.Text).HasMaxLength(ContentBlock.TextMaxLength);
        builder.Property(b => b.ImageUrl).HasMaxLength(ContentBlock.ImageUrlMaxLength);

        builder.Property(b => b.Language).HasConversion<string>().HasMaxLength(16).IsUnicode(false);
        builder.HasIndex(b => new { b.Language, b.Kind, b.IsPublished, b.DisplayOrder });
    }
}
