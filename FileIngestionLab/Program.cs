//#define DEMO_MONITOR
//#define DEMO_ARCHIVE

using FileIngestionLab.Domain;
using FileIngestionLab.Monitoring;
using FileIngestionLab.Pipelines;
using FileIngestionLab.Processing;
using FileIngestionLab.Utilities;

namespace FileIngestionLab;

public static class Program
{
    public static async Task Main()
    {
        Console.WriteLine("-- Lab 3: Events, Files, Filesystem, Streams --");
        Console.WriteLine("Starter project located in lab3_gpt_task/FileIngestionLab");
        Console.WriteLine("Use the README instructions to implement the missing pieces.");
#if DEMO_MONITOR
        await RunMonitorDemoAsync();
#endif
#if DEMO_ARCHIVE
        RunProcessingDemo();
#endif
    }

    private static async Task RunMonitorDemoAsync()
    {
        var monitor = new DropFolderMonitor(
            ProjectPaths.DropFolder.FullName,
            DropFolderMonitorOptions.Default);

        using var pipeline = new DropPipeline(
            monitor,
            new SnapshotReader(),
            new SnapshotCatalog(ProjectPaths.DropFolder),
            new SnapshotArchiveBuilder(),
            new SnapshotIndexWriter(),
            new LogBook(ProjectPaths.LogFile));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await pipeline.StartAsync(cts.Token);
    }

    private static void RunProcessingDemo()
    {
        var reader = new SnapshotReader();
        var catalog = new SnapshotCatalog(ProjectPaths.SnapshotSourceRoot);

        var envelopes = catalog
            .EnumerateSnapshotFiles()
            .Select(file => new SnapshotEnvelope(file, reader.ReadSnapshot(file)))
            .ToList();

        var archive = ProjectPaths.GetArchiveFile("demo-snapshots.gz");
        var index = ProjectPaths.GetArchiveFile("demo-index.bin");

        new SnapshotArchiveBuilder().WriteArchive(envelopes, archive);
        new SnapshotIndexWriter().WriteIndex(envelopes, index);
        new LogBook(ProjectPaths.LogFile).AppendSummary(envelopes, archive, index);
    }
}
