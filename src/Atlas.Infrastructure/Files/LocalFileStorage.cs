using Atlas.Application.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Atlas.Infrastructure.Files;

/// <summary>Stores files on the local disk. Swap for a blob-storage implementation for multi-instance hosting.</summary>
public sealed class LocalFileStorage(IOptions<FileStorageOptions> options, IHostEnvironment environment) : IFileStorage
{
    private readonly FileStorageOptions _options = options.Value;

    public string RootPath => ResolveRoot(_options, environment);

    public static string ResolveRoot(FileStorageOptions options, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(environment);
        return Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.RootPath));
    }

    public async Task<string> SaveAsync(string folder, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var safeFolder = SanitizeSegment(folder);
        var safeName = SanitizeSegment(fileName);
        var directory = Path.Combine(RootPath, safeFolder);
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, safeName);
        await using (var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            await content.CopyToAsync(file, cancellationToken);
        }

        return $"{_options.RequestPath.TrimEnd('/')}/{safeFolder}/{safeName}";
    }

    public Task DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        var prefix = _options.RequestPath.TrimEnd('/') + "/";
        if (string.IsNullOrEmpty(url) || !url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask; // Not ours (e.g. an external default logo URL).
        }

        var relative = url[prefix.Length..].Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(RootPath, relative));
        if (fullPath.StartsWith(RootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal) && File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private static string SanitizeSegment(string value)
    {
        var name = Path.GetFileName(value);
        if (string.IsNullOrWhiteSpace(name) || name is "." or ".." || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException($"'{value}' is not a valid file or folder name.", nameof(value));
        }

        return name;
    }
}
