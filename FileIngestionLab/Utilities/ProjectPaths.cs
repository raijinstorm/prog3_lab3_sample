using System.Diagnostics;

namespace FileIngestionLab.Utilities;

public static class ProjectPaths
{
    public static DirectoryInfo RepositoryRoot { get; } = ResolveRepoRoot();

    public static DirectoryInfo LabRoot { get; } = RepositoryRoot;

    public static DirectoryInfo SampleDataRoot { get; } = EnsureDirectory(
        Path.Combine(LabRoot.FullName, "sample-data"));

    public static DirectoryInfo SnapshotSourceRoot { get; } = EnsureDirectory(
        Path.Combine(SampleDataRoot.FullName, "snapshots"));

    public static DirectoryInfo DropFolder { get; } = EnsureDirectory(
        Path.Combine(SampleDataRoot.FullName, "drop"));

    public static DirectoryInfo ArtifactsRoot { get; } = EnsureDirectory(
        Path.Combine(SampleDataRoot.FullName, "artifacts"));

    public static FileInfo LogFile { get; } = new(Path.Combine(
        ArtifactsRoot.FullName,
        "lab3.log"));

    public static FileInfo GetArchiveFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name must be provided", nameof(fileName));
        }

        return new FileInfo(Path.Combine(ArtifactsRoot.FullName, fileName));
    }

    private static DirectoryInfo ResolveRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "FileIngestionLab.sln")))
        {
            dir = dir.Parent;
        }

        return dir ?? throw new UnreachableException("Unable to locate FileIngestionLab.sln.");
    }

    private static DirectoryInfo EnsureDirectory(string path)
    {
        Directory.CreateDirectory(path);
        return new DirectoryInfo(path);
    }
}
