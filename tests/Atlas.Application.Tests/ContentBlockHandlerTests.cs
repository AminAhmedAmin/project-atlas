using Atlas.Application.Common;
using Atlas.Application.Content;
using Atlas.Application.Tests.Fakes;
using Atlas.Domain.Content;

namespace Atlas.Application.Tests;

public sealed class ContentBlockHandlerTests
{
    private static readonly byte[] PngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];

    private readonly InMemoryContentBlockRepository _blocks = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly InMemoryFileStorage _files = new();

    private SaveContentBlockHandler SaveHandler() =>
        new(_blocks, _unitOfWork, _files, new SaveContentBlockValidator(), TestData.Clock());

    [Fact]
    public async Task Create_update_and_list_published_blocks_in_order()
    {
        var save = SaveHandler();
        var ct = TestContext.Current.CancellationToken;

        var second = await save.HandleAsync(new SaveContentBlockCommand(null, BlockKind.Faq, "Q2", null, "A2", null, 1, true), ct);
        await save.HandleAsync(new SaveContentBlockCommand(null, BlockKind.Faq, "Q1", null, "A1", null, 0, true), ct);
        await save.HandleAsync(new SaveContentBlockCommand(null, BlockKind.Faq, "Hidden", null, "A", null, 2, false), ct);
        await save.HandleAsync(new SaveContentBlockCommand(null, BlockKind.Stat, "50+", null, "Projects", null, 0, true), ct);

        var updated = await save.HandleAsync(
            new SaveContentBlockCommand(second.Value.Id, BlockKind.Faq, "Q2 edited", null, "A2", null, 1, true), ct);
        Assert.True(updated.IsSuccess);

        var faqs = await new GetContentBlocksHandler(_blocks).HandleAsync(new GetContentBlocksQuery(BlockKind.Faq, PublishedOnly: true), ct);

        Assert.Equal(["Q1", "Q2 edited"], faqs.Select(f => f.Title));
    }

    [Fact]
    public async Task Validation_uses_kind_specific_labels()
    {
        var result = await SaveHandler().HandleAsync(
            new SaveContentBlockCommand(null, BlockKind.Testimonial, "", null, "", null, 0, true),
            TestContext.Current.CancellationToken);

        Assert.Equal(["Person's name is required.", "Quote is required."], result.Errors.Select(e => e.Message));
    }

    [Fact]
    public async Task Changing_the_kind_of_an_existing_block_is_rejected()
    {
        var ct = TestContext.Current.CancellationToken;
        var created = await SaveHandler().HandleAsync(new SaveContentBlockCommand(null, BlockKind.Faq, "Q", null, "A", null, 0, true), ct);

        var result = await SaveHandler().HandleAsync(new SaveContentBlockCommand(created.Value.Id, BlockKind.Stat, "Q", null, "A", null, 0, true), ct);

        Assert.Equal(ErrorType.NotFound, Assert.Single(result.Errors).Type);
    }

    [Fact]
    public async Task Replacing_or_deleting_a_logo_removes_the_old_file()
    {
        var ct = TestContext.Current.CancellationToken;
        var upload = new UploadContentImageHandler(_files, new UploadContentImageValidator());
        var first = await upload.HandleAsync(new UploadContentImageCommand(new MemoryStream(PngHeader), "a.png", "image/png", PngHeader.Length), ct);
        var second = await upload.HandleAsync(new UploadContentImageCommand(new MemoryStream(PngHeader), "b.png", "image/png", PngHeader.Length), ct);

        var created = await SaveHandler().HandleAsync(new SaveContentBlockCommand(null, BlockKind.ClientLogo, "Acme", null, null, first.Value, 0, true), ct);
        await SaveHandler().HandleAsync(new SaveContentBlockCommand(created.Value.Id, BlockKind.ClientLogo, "Acme", null, null, second.Value, 0, true), ct);

        Assert.Equal([second.Value], _files.Files.Keys);

        var deleted = await new DeleteContentBlockHandler(_blocks, _unitOfWork, _files).HandleAsync(new DeleteContentBlockCommand(created.Value.Id), ct);

        Assert.True(deleted.IsSuccess);
        Assert.Empty(_files.Files);
        Assert.Empty(_blocks.Blocks);
    }

    [Fact]
    public async Task Upload_rejects_files_that_are_not_images()
    {
        var bytes = "<script>alert(1)</script>"u8.ToArray();

        var result = await new UploadContentImageHandler(_files, new UploadContentImageValidator()).HandleAsync(
            new UploadContentImageCommand(new MemoryStream(bytes), "x.png", "image/png", bytes.Length),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Empty(_files.Files);
    }

    [Fact]
    public void Default_blocks_are_valid_and_hide_unverifiable_claims()
    {
        foreach (var (kind, fields) in DefaultContent.Blocks)
        {
            var block = ContentBlock.Create(kind, fields, TestData.Now.UtcDateTime);
            if (kind is BlockKind.Testimonial or BlockKind.ClientLogo)
            {
                Assert.False(block.IsPublished);
            }
        }
    }
}
