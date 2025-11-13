namespace FileIngestionLab.Monitoring;

public sealed record DropFolderMonitorOptions
{
    public string Filter { get; init; } = "*.sensor";

    public bool IncludeSubdirectories { get; init; }

    public TimeSpan StabilizationDelay { get; init; } = TimeSpan.FromMilliseconds(800);

    public IReadOnlyCollection<string> AllowedExtensions { get; init; } = new[] { ".sensor" };

    public static DropFolderMonitorOptions Default { get; } = new();
}
