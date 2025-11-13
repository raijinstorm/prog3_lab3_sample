namespace FileIngestionLab.Processing;

/// <summary>
/// Provides filesystem-oriented operations for discovering snapshot files.
/// </summary>
public sealed class SnapshotCatalog
{
    private readonly DirectoryInfo _root;

    public SnapshotCatalog(DirectoryInfo root)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
    }

    public IEnumerable<FileInfo> EnumerateSnapshotFiles()
    {
        // TODO [Task 5]:
        //  * Use Directory.EnumerateFiles + SearchOption.AllDirectories.
        //  * Accept files matching *.sensor.
        //  * Order by timestamp extracted from file name (ISO-8601 suffix).
        //  * Skip files that no longer exist by the time you iterate over them.
        throw new NotImplementedException();
    }

    public IReadOnlyDictionary<string, IReadOnlyList<FileInfo>> BuildSensorIndex()
    {
        // TODO [Task 5]:
        //  * Group files by sensor id (prefix before first '-').
        //  * Value should be a list ordered by timestamp ascending.
        throw new NotImplementedException();
    }
}
