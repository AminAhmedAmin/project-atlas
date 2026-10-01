using Atlas.Domain.Chat;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atlas.Infrastructure.Persistence.Configurations;

internal sealed class ChatConversationConfiguration : IEntityTypeConfiguration<ChatConversation>
{
    public void Configure(EntityTypeBuilder<ChatConversation> builder)
    {
        builder.ToTable("ChatConversations");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.VisitorName).HasMaxLength(ChatConversation.VisitorNameMaxLength).IsRequired();
        builder.Property(c => c.VisitorContact).HasMaxLength(ChatConversation.VisitorContactMaxLength);
        builder.Property(c => c.Language).HasConversion<string>().HasMaxLength(16).IsUnicode(false);
        builder.Property(c => c.AccessToken).HasMaxLength(ChatConversation.AccessTokenLength).IsUnicode(false).IsFixedLength().IsRequired();

        builder.HasMany(c => c.Messages)
            .WithOne()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Messages).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(c => new { c.IsClosed, c.LastMessageAtUtc });
    }
}

internal sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Sender).HasConversion<string>().HasMaxLength(16).IsUnicode(false);
        builder.Property(m => m.AgentName).HasMaxLength(ChatConversation.VisitorNameMaxLength);
        builder.Property(m => m.Text).HasMaxLength(ChatMessage.TextMaxLength).IsRequired();

        builder.HasIndex(m => new { m.ConversationId, m.SentAtUtc });
    }
}
