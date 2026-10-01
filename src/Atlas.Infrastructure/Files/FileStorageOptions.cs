namespace Atlas.Infrastructure.Files;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>Folder for uploaded files. Relative paths are resolved against the content root.</summary>
    public string RootPath { get; set; } = "App_Data/uploads";

    /// <summary>URL prefix under which uploaded files are served.</summary>
    public string RequestPath { get; set; } = "/uploads";
}
