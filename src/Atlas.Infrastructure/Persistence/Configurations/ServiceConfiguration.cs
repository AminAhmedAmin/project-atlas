using Atlas.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Title).HasMaxLength(Service.TitleMaxLength).IsRequired();
        builder.Property(s => s.Summary).HasMaxLength(Service.SummaryMaxLength).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(Service.DescriptionMaxLength);
        builder.Property(s => s.Icon).HasMaxLength(Service.IconMaxLength);

        builder.HasIndex(s => new { s.IsPublished, s.DisplayOrder });
    }
}
