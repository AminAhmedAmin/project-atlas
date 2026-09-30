using Atlas.Domain.Common;
using Atlas.Domain.Contact;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.ToTable("ContactMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Name).HasMaxLength(ContactMessage.NameMaxLength).IsRequired();
        builder.Property(m => m.Subject).HasMaxLength(ContactMessage.SubjectMaxLength);
        builder.Property(m => m.Message).HasMaxLength(ContactMessage.MessageMaxLength).IsRequired();

        // A complex property (rather than a value converter) keeps Email.Value queryable for search.
        builder.ComplexProperty(m => m.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(EmailAddress.MaxLength)
                .IsRequired();
        });

        builder.HasIndex(m => m.ReceivedAtUtc);
        builder.HasIndex(m => m.IsRead);
    }
}
