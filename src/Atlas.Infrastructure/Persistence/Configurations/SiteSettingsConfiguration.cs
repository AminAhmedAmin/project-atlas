using Atlas.Domain.Common;
using Atlas.Domain.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class SiteSettingsConfiguration : IEntityTypeConfiguration<SiteSettings>
{
    public void Configure(EntityTypeBuilder<SiteSettings> builder)
    {
        builder.ToTable("SiteSettings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.CompanyName).HasMaxLength(SiteSettings.CompanyNameMaxLength).IsRequired();
        builder.Property(s => s.Tagline).HasMaxLength(SiteSettings.TaglineMaxLength);
        builder.Property(s => s.LogoUrl).HasMaxLength(SiteSettings.LogoUrlMaxLength);
        builder.Property(s => s.ArabicCompanyName).HasMaxLength(SiteSettings.CompanyNameMaxLength);
        builder.Property(s => s.ArabicTagline).HasMaxLength(SiteSettings.TaglineMaxLength);

        builder.Property(s => s.PrimaryColor)
            .HasConversion(c => c.Value, v => HexColor.Create(v))
            .HasMaxLength(7)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(s => s.ContactEmail)
            .HasConversion(e => e!.Value, v => EmailAddress.Create(v))
            .HasMaxLength(EmailAddress.MaxLength);
    }
}
