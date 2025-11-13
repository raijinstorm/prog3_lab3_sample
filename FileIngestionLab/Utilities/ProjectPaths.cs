using System.Diagnostics;

namespace FileIngestionLab.Utilities;

public static class ProjectPaths
{
    public static DirectoryInfo RepositoryRoot { get; } = ResolveRepoRoot();

    public static DirectoryInfo LabRoot { get; } = new(Path.Combine(
        RepositoryRoot.FullName,
        "lab3_gpt_task"));

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

        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "lab3_gpt_task")))
        {
            dir = dir.Parent;
        }

        return dir ?? throw new UnreachableException("Unable to locate lab3_gpt_task folder.");
    }

    private static DirectoryInfo EnsureDirectory(string path)
    {
        Directory.CreateDirectory(path);
        return new DirectoryInfo(path);
    }
}
