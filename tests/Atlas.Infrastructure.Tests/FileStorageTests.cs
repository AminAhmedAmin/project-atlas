using Atlas.Infrastructure.Files;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Tests;

public sealed class FileStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "atlas-tests-" + Guid.NewGuid().ToString("N"));

    private LocalFileStorage CreateStorage() => new(
        Options.Create(new FileStorageOptions { RootPath = "uploads", RequestPath = "/uploads" }),
        new TestEnvironment(_root));

    [Fact]
    public async Task Save_and_delete_round_trip()
    {
        var storage = CreateStorage();
        var ct = TestContext.Current.CancellationToken;

        var url = await storage.SaveAsync("branding", "logo.png", new MemoryStream([1, 2, 3]), ct);

        Assert.Equal("/uploads/branding/logo.png", url);
        var path = Path.Combine(_root, "uploads", "branding", "logo.png");
        Assert.True(File.Exists(path));

        await storage.DeleteAsync(url, ct);
        Assert.False(File.Exists(path));
    }

    [Theory]
    [InlineData("..", "x.png")]
    [InlineData("branding", "..")]
    public async Task Save_rejects_path_traversal(string folder, string fileName)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateStorage().SaveAsync(folder, fileName, new MemoryStream([1]), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Delete_ignores_urls_outside_the_storage_root()
    {
        var outside = Path.Combine(_root, "secret.txt");
        Directory.CreateDirectory(_root);
        await File.WriteAllTextAsync(outside, "keep", TestContext.Current.CancellationToken);

        await CreateStorage().DeleteAsync("/uploads/../secret.txt", TestContext.Current.CancellationToken);
        await CreateStorage().DeleteAsync("https://cdn.example.com/logo.png", TestContext.Current.CancellationToken);

        Assert.True(File.Exists(outside));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private sealed class TestEnvironment(string contentRoot) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";

        public string ApplicationName { get; set; } = "Atlas.Tests";

        public string ContentRootPath { get; set; } = contentRoot;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
