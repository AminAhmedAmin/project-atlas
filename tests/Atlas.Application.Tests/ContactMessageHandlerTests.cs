using Atlas.Application.Contact;
using Atlas.Application.Tests.Fakes;
using Atlas.Domain.Common;
using Atlas.Domain.Contact;
using Atlas.Domain.Settings;
using Microsoft.Extensions.Logging.Abstractions;

namespace Atlas.Application.Tests;

public sealed class ContactMessageHandlerTests
{
    private readonly InMemoryContactMessageRepository _messages = new();
    private readonly InMemorySiteSettingsRepository _settings = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly RecordingEmailSender _email = new();

    private SubmitContactMessageHandler SubmitHandler() => new(
        _messages,
        _settings,
        _unitOfWork,
        _email,
        new SubmitContactMessageValidator(),
        TestData.Clock(),
        NullLogger<SubmitContactMessageHandler>.Instance);

    [Fact]
    public async Task Submit_saves_message()
    {
        var result = await SubmitHandler().HandleAsync(
            new SubmitContactMessageCommand("Jane", "jane@example.com", "Quote", "Hello"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var saved = Assert.Single(_messages.Messages);
        Assert.Equal(result.Value, saved.Id);
        Assert.Equal(TestData.Now.UtcDateTime, saved.ReceivedAtUtc);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Submit_returns_field_errors_for_invalid_input()
    {
        var result = await SubmitHandler().HandleAsync(
            new SubmitContactMessageCommand("", "nope", null, ""),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(["Name", "Email", "Message"], result.Errors.Select(e => e.Field));
        Assert.Empty(_messages.Messages);
        Assert.Equal(0, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Submit_notifies_contact_email_when_configured()
    {
        _settings.Settings = SiteSettings.Create("Co", null, HexColor.Create("#000000"), EmailAddress.Create("inbox@example.com"), TestData.Now.UtcDateTime);

        await SubmitHandler().HandleAsync(
            new SubmitContactMessageCommand("Jane", "jane@example.com", null, "Hello"),
            TestContext.Current.CancellationToken);

        var mail = Assert.Single(_email.Sent);
        Assert.Equal("inbox@example.com", mail.To);
        Assert.Equal("jane@example.com", mail.ReplyTo);
    }

    [Fact]
    public async Task Submit_succeeds_even_when_notification_fails()
    {
        _settings.Settings = SiteSettings.Create("Co", null, HexColor.Create("#000000"), EmailAddress.Create("inbox@example.com"), TestData.Now.UtcDateTime);
        _email.Fail = true;

        var result = await SubmitHandler().HandleAsync(
            new SubmitContactMessageCommand("Jane", "jane@example.com", null, "Hello"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Single(_messages.Messages);
    }

    [Fact]
    public async Task Search_filters_by_text_and_unread_and_pages_newest_first()
    {
        for (var i = 0; i < 5; i++)
        {
            _messages.Add(ContactMessage.Create($"User {i}", EmailAddress.Create($"u{i}@example.com"), null, i % 2 == 0 ? "about pricing" : "hello", TestData.Now.UtcDateTime.AddMinutes(i)));
        }

        _messages.Messages[4].MarkAsRead(TestData.Now.UtcDateTime);
        var handler = new GetContactMessagesHandler(_messages);

        var result = await handler.HandleAsync(new GetContactMessagesQuery("PRICING", UnreadOnly: true, Page: 1, PageSize: 1), TestContext.Current.CancellationToken);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal("User 2", Assert.Single(result.Items).Name);
    }

    [Fact]
    public async Task Search_clamps_paging()
    {
        var handler = new GetContactMessagesHandler(_messages);

        var result = await handler.HandleAsync(new GetContactMessagesQuery(Page: -3, PageSize: 10_000), TestContext.Current.CancellationToken);

        Assert.Equal(1, result.Page);
        Assert.Equal(GetContactMessagesHandler.MaxPageSize, result.PageSize);
    }

    [Fact]
    public async Task Mark_read_and_unread()
    {
        var message = ContactMessage.Create("Jane", EmailAddress.Create("jane@example.com"), null, "Hi", TestData.Now.UtcDateTime);
        _messages.Add(message);
        var handler = new SetContactMessageReadHandler(_messages, _unitOfWork, TestData.Clock());

        Assert.True((await handler.HandleAsync(new SetContactMessageReadCommand(message.Id, true), TestContext.Current.CancellationToken)).IsSuccess);
        Assert.True(message.IsRead);

        Assert.True((await handler.HandleAsync(new SetContactMessageReadCommand(message.Id, false), TestContext.Current.CancellationToken)).IsSuccess);
        Assert.False(message.IsRead);
    }

    [Fact]
    public async Task Delete_removes_message_or_reports_not_found()
    {
        var message = ContactMessage.Create("Jane", EmailAddress.Create("jane@example.com"), null, "Hi", TestData.Now.UtcDateTime);
        _messages.Add(message);
        var handler = new DeleteContactMessageHandler(_messages, _unitOfWork);

        Assert.True((await handler.HandleAsync(new DeleteContactMessageCommand(message.Id), TestContext.Current.CancellationToken)).IsSuccess);
        Assert.Empty(_messages.Messages);

        var missing = await handler.HandleAsync(new DeleteContactMessageCommand(message.Id), TestContext.Current.CancellationToken);
        Assert.Equal(Common.ErrorType.NotFound, Assert.Single(missing.Errors).Type);
    }
}

public sealed class ContactDetailsHandlerTests
{
    private readonly InMemoryContactMessageRepository _messages = new();

    private SubmitContactMessageHandler Handler() => new(
        _messages,
        new InMemorySiteSettingsRepository(),
        new FakeUnitOfWork(),
        new RecordingEmailSender(),
        new SubmitContactMessageValidator(),
        TestData.Clock(),
        NullLogger<SubmitContactMessageHandler>.Instance);

    [Fact]
    public async Task Details_are_saved()
    {
        var result = await Handler().HandleAsync(
            new SubmitContactMessageCommand("Sara", "sara@example.com", null, "Hi", "+966551234567", "Mobile apps", BudgetRanges.From50KTo150K, SiteLanguage.Arabic),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var saved = Assert.Single(_messages.Messages);
        Assert.Equal(BudgetRanges.From50KTo150K, saved.Budget);
        Assert.Equal(SiteLanguage.Arabic, saved.Language);
    }

    [Fact]
    public async Task Invalid_phone_and_unknown_budget_are_field_errors()
    {
        var result = await Handler().HandleAsync(
            new SubmitContactMessageCommand("Sara", "sara@example.com", null, "Hi", "abc", null, "a-million"),
            TestContext.Current.CancellationToken);

        Assert.Equal(["Phone", "Budget"], result.Errors.Select(e => e.Field));
        Assert.Empty(_messages.Messages);
    }

    [Fact]
    public void Every_budget_has_a_label()
    {
        Assert.All(BudgetRanges.All, key => Assert.NotEqual(key, BudgetRanges.Label(key)));
    }
}
