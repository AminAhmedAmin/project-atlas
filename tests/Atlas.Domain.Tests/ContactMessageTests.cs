using Atlas.Domain.Common;
using Atlas.Domain.Contact;

namespace Atlas.Domain.Tests;

public sealed class ContactMessageTests
{
    private static ContactMessage NewMessage() =>
        ContactMessage.Create("  Jane  ", EmailAddress.Create("jane@example.com"), " ", "Hello there", TestTime.Now);

    [Fact]
    public void Create_trims_input_and_starts_unread()
    {
        var message = NewMessage();

        Assert.Equal("Jane", message.Name);
        Assert.Null(message.Subject);
        Assert.False(message.IsRead);
        Assert.Null(message.ReadAtUtc);
        Assert.Equal(TestTime.Now, message.ReceivedAtUtc);
        Assert.NotEqual(Guid.Empty, message.Id);
    }

    [Theory]
    [InlineData("", "Hello")]
    [InlineData("Jane", "   ")]
    public void Create_requires_name_and_message(string name, string body)
    {
        Assert.Throws<DomainException>(() =>
            ContactMessage.Create(name, EmailAddress.Create("jane@example.com"), null, body, TestTime.Now));
    }

    [Fact]
    public void Create_rejects_overlong_message()
    {
        var body = new string('x', ContactMessage.MessageMaxLength + 1);

        Assert.Throws<DomainException>(() =>
            ContactMessage.Create("Jane", EmailAddress.Create("jane@example.com"), null, body, TestTime.Now));
    }

    [Fact]
    public void Create_requires_utc_timestamp()
    {
        var local = DateTime.SpecifyKind(TestTime.Now, DateTimeKind.Local);

        Assert.Throws<DomainException>(() =>
            ContactMessage.Create("Jane", EmailAddress.Create("jane@example.com"), null, "Hi", local));
    }

    [Fact]
    public void MarkAsRead_is_idempotent_and_keeps_first_read_time()
    {
        var message = NewMessage();
        var first = TestTime.Now.AddHours(1);

        message.MarkAsRead(first);
        message.MarkAsRead(first.AddHours(1));

        Assert.True(message.IsRead);
        Assert.Equal(first, message.ReadAtUtc);
    }

    [Fact]
    public void MarkAsUnread_clears_read_state()
    {
        var message = NewMessage();
        message.MarkAsRead(TestTime.Now);

        message.MarkAsUnread();

        Assert.False(message.IsRead);
        Assert.Null(message.ReadAtUtc);
    }
}
