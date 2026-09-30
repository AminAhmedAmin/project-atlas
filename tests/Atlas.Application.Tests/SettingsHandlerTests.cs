using Atlas.Application.Settings;
using Atlas.Application.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace Atlas.Application.Tests;

public sealed class SettingsHandlerTests
{
    private static readonly byte[] PngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];

    private readonly InMemorySiteSettingsRepository _settings = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly InMemoryFileStorage _files = new();
    private readonly BrandingDefaults _defaults = new() { CompanyName = "Default Co", PrimaryColor = "#112233" };

    private UploadLogoHandler UploadHandler() => new(
        _settings,
        _unitOfWork,
        _files,
        new UploadLogoValidator(),
        _defaults,
        TestData.Clock(),
        NullLogger<UploadLogoHandler>.Instance);

    [Fact]
    public async Task Get_returns_defaults_until_saved()
    {
        var dto = await new GetSiteSettingsHandler(_settings, _defaults).HandleAsync(new GetSiteSettingsQuery(), TestContext.Current.CancellationToken);

        Assert.Equal("Default Co", dto.CompanyName);
        Assert.Equal("#112233", dto.PrimaryColor);
    }

    [Fact]
    public async Task Update_creates_settings_and_normalizes_color()
    {
        var handler = new UpdateSiteSettingsHandler(_settings, _unitOfWork, new UpdateSiteSettingsValidator(), TestData.Clock());

        var result = await handler.HandleAsync(new UpdateSiteSettingsCommand("New Co", null, "#ABC", "hi@example.com"), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("#aabbcc", result.Value.PrimaryColor);
        Assert.Equal("New Co", _settings.Settings?.CompanyName);
    }

    [Fact]
    public async Task Update_rejects_invalid_values()
    {
        var handler = new UpdateSiteSettingsHandler(_settings, _unitOfWork, new UpdateSiteSettingsValidator(), TestData.Clock());

        var result = await handler.HandleAsync(new UpdateSiteSettingsCommand("", null, "blue", "nope"), TestContext.Current.CancellationToken);

        Assert.Equal(["CompanyName", "PrimaryColor", "ContactEmail"], result.Errors.Select(e => e.Field));
        Assert.Null(_settings.Settings);
    }

    [Fact]
    public async Task Upload_logo_stores_file_and_replaces_previous()
    {
        var handler = UploadHandler();

        var first = await handler.HandleAsync(new UploadLogoCommand(new MemoryStream(PngHeader), "a.png", "image/png", PngHeader.Length), TestContext.Current.CancellationToken);
        var second = await handler.HandleAsync(new UploadLogoCommand(new MemoryStream(PngHeader), "b.png", "image/png", PngHeader.Length), TestContext.Current.CancellationToken);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(second.Value, _settings.Settings?.LogoUrl);
        Assert.Equal([second.Value], _files.Files.Keys);
        Assert.EndsWith(".png", second.Value, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Upload_logo_rejects_disallowed_type()
    {
        var result = await UploadHandler().HandleAsync(
            new UploadLogoCommand(new MemoryStream("<svg/>"u8.ToArray()), "x.svg", "image/svg+xml", 6),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Empty(_files.Files);
    }

    [Fact]
    public async Task Upload_logo_rejects_content_that_does_not_match_type()
    {
        var bytes = "not really a png"u8.ToArray();

        var result = await UploadHandler().HandleAsync(
            new UploadLogoCommand(new MemoryStream(bytes), "x.png", "image/png", bytes.Length),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Empty(_files.Files);
    }

    [Fact]
    public async Task Upload_logo_rejects_stream_larger_than_declared_limit()
    {
        var bytes = new byte[UploadLogoValidator.MaxBytes + 1];
        PngHeader.CopyTo(bytes, 0);

        // Declared length lies; the handler must still stop reading at the limit.
        var result = await UploadHandler().HandleAsync(
            new UploadLogoCommand(new MemoryStream(bytes), "x.png", "image/png", 100),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Empty(_files.Files);
    }

    [Fact]
    public async Task Remove_logo_clears_setting_and_deletes_file()
    {
        await UploadHandler().HandleAsync(new UploadLogoCommand(new MemoryStream(PngHeader), "a.png", "image/png", PngHeader.Length), TestContext.Current.CancellationToken);

        var result = await new RemoveLogoHandler(_settings, _unitOfWork, _files, TestData.Clock()).HandleAsync(new RemoveLogoCommand(), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Null(_settings.Settings?.LogoUrl);
        Assert.Empty(_files.Files);
    }
}
