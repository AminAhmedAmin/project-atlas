namespace Atlas.Application.Common;

/// <summary>Shared rules for uploaded images (logos, client logos).</summary>
public static class ImageUpload
{
    public const long MaxBytes = 2 * 1024 * 1024;

    /// <summary>Allowed image types. SVG is excluded because it can carry script.</summary>
    public static readonly IReadOnlyDictionary<string, string> AllowedTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["image/png"] = ".png",
        ["image/jpeg"] = ".jpg",
        ["image/webp"] = ".webp",
    };

    public static ValidationErrors ValidateUpload(this ValidationErrors errors, long length, string contentType, string field)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return errors
            .Must(length > 0, field, "The file is empty.")
            .Must(length <= MaxBytes, field, "The image must be 2 MB or smaller.")
            .Must(AllowedTypes.ContainsKey(contentType), field, "The image must be a PNG, JPEG or WebP file.");
    }

    /// <summary>
    /// Reads at most <see cref="MaxBytes"/> from <paramref name="source"/> and checks that the bytes
    /// really are the declared image type. Returns the buffered image positioned at the start.
    /// </summary>
    public static async Task<Result<MemoryStream>> ReadVerifiedAsync(Stream source, string contentType, string field, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);

        var buffer = new MemoryStream();
        var chunk = new byte[81920];
        long total = 0;
        int read;
        while ((read = await source.ReadAsync(chunk, cancellationToken)) > 0)
        {
            total += read;
            if (total > MaxBytes)
            {
                await buffer.DisposeAsync();
                return Result.Failure<MemoryStream>(Error.Validation(field, "The image must be 2 MB or smaller."));
            }

            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }

        if (!Matches(buffer.GetBuffer().AsSpan(0, (int)buffer.Length), contentType))
        {
            await buffer.DisposeAsync();
            return Result.Failure<MemoryStream>(Error.Validation(field, "The file content does not match its image type."));
        }

        buffer.Position = 0;
        return Result.Success(buffer);
    }

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF];

    private static bool Matches(ReadOnlySpan<byte> data, string contentType) => contentType.ToUpperInvariant() switch
    {
        "IMAGE/PNG" => data.StartsWith(Png),
        "IMAGE/JPEG" => data.StartsWith(Jpeg),
        "IMAGE/WEBP" => data.Length >= 12 && data[..4].SequenceEqual("RIFF"u8) && data[8..12].SequenceEqual("WEBP"u8),
        _ => false,
    };
}
