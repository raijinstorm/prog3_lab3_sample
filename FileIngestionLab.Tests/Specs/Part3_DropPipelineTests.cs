using FileIngestionLab.Monitoring;
using FileIngestionLab.Pipelines;
using FileIngestionLab.Processing;
using FileIngestionLab.Tests.Infrastructure;

namespace FileIngestionLab.Tests.Specs;

public static class Part3_DropPipelineTests
{
    public static async Task FlushesBufferOnBatchSizeAsync()
    {
        var root = TestEnvironment.CreateUniqueDirectory();
        var drop = Path.Combine(root, "drop");
        var artifacts = Path.Combine(root, "artifacts");
        Directory.CreateDirectory(drop);
        Directory.CreateDirectory(artifacts);

        try
        {
            var options = new DropFolderMonitorOptions
            {
                StabilizationDelay = TimeSpan.FromMilliseconds(50),
                AllowedExtensions = new[] { ".sensor" },
                Filter = "*.sensor"
            };

            using var monitor = new DropFolderMonitor(drop, options);
            using var pipeline = new DropPipeline(
                monitor,
                new SnapshotReader(),
                new SnapshotCatalog(new DirectoryInfo(drop)),
                new SnapshotArchiveBuilder(),
                new SnapshotIndexWriter(),
                new LogBook(new FileInfo(Path.Combine(artifacts, "lab3.log"))));

            using var cts = new CancellationTokenSource();
            var runTask = pipeline.StartAsync(cts.Token);

            for (var i = 0; i < 5; i++)
            {
                var fileName = $"alpha-2025-02-10T09-0{i}.sensor";
                await File.WriteAllTextAsync(Path.Combine(drop, fileName), BuildSnapshotContent($"alpha", new DateTime(2025, 2, 10, 9, i, 0, DateTimeKind.Utc), i));
                await Task.Delay(100);
            }

            await WaitForConditionAsync(() => Directory.GetFiles(artifacts, "*.gz").Any(), TimeSpan.FromSeconds(10), "Pipeline should emit a compressed archive once five files are processed.");
            await WaitForConditionAsync(() => Directory.GetFiles(artifacts, "*.bin").Any(), TimeSpan.FromSeconds(10), "Pipeline should emit a binary index once five files are processed.");

            var logFile = Path.Combine(artifacts, "lab3.log");
            await WaitForConditionAsync(() => File.Exists(logFile), TimeSpan.FromSeconds(5), "Pipeline should append to the log book.");
            var logContent = await File.ReadAllTextAsync(logFile);
            AssertEx.True(logContent.Contains("Files processed: 5"), "Log should include the number of processed files after batch flush.");

            cts.Cancel();
            try
            {
                await runTask.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (OperationCanceledException)
            {
                // Expected once cancellation is requested.
            }
        }
        finally
        {
            TestEnvironment.Cleanup(root);
        }
    }

    public static async Task FlushesOnCancellationAsync()
    {
        var root = TestEnvironment.CreateUniqueDirectory();
        var drop = Path.Combine(root, "drop");
        var artifacts = Path.Combine(root, "artifacts");
        Directory.CreateDirectory(drop);
        Directory.CreateDirectory(artifacts);

        try
        {
            var options = new DropFolderMonitorOptions
            {
                StabilizationDelay = TimeSpan.FromMilliseconds(50),
                AllowedExtensions = new[] { ".sensor" },
                Filter = "*.sensor"
            };

            using var monitor = new DropFolderMonitor(drop, options);
            using var pipeline = new DropPipeline(
                monitor,
                new SnapshotReader(),
                new SnapshotCatalog(new DirectoryInfo(drop)),
                new SnapshotArchiveBuilder(),
                new SnapshotIndexWriter(),
                new LogBook(new FileInfo(Path.Combine(artifacts, "lab3.log"))));

            using var cts = new CancellationTokenSource();
            var runTask = pipeline.StartAsync(cts.Token);

            for (var i = 0; i < 2; i++)
            {
                var fileName = $"beta-2025-02-10T09-1{i}.sensor";
                await File.WriteAllTextAsync(Path.Combine(drop, fileName), BuildSnapshotContent($"beta", new DateTime(2025, 2, 10, 9, i + 10, 0, DateTimeKind.Utc), i));
                await Task.Delay(100);
            }

            await Task.Delay(500);
            cts.Cancel();

            try
            {
                await runTask.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (OperationCanceledException)
            {
                // Expected because cancellation stops the pipeline.
            }

            await WaitForConditionAsync(() => Directory.GetFiles(artifacts, "*.gz").Any(), TimeSpan.FromSeconds(5), "Pipeline should flush remaining files when cancellation is requested.");
            await WaitForConditionAsync(() => Directory.GetFiles(artifacts, "*.bin").Any(), TimeSpan.FromSeconds(5), "Pipeline should flush remaining files when cancellation is requested.");

            var logFile = Path.Combine(artifacts, "lab3.log");
            AssertEx.True(File.Exists(logFile), "Log should exist after cancellation flush.");
            var logContent = await File.ReadAllTextAsync(logFile);
            AssertEx.True(logContent.Contains("Files processed: 2"), "Log should record the number of processed files after cancellation flush.");
        }
        finally
        {
            TestEnvironment.Cleanup(root);
        }
    }

    private static string BuildSnapshotContent(string sensor, DateTime timestamp, int measurement)
    {
        return $"""
# header
sensor={sensor}
timestamp={timestamp:O}

[measurements]
value={measurement}
""";
    }

    private static async Task WaitForConditionAsync(Func<bool> predicate, TimeSpan timeout, string failureMessage)
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < timeout)
        {
            if (predicate())
            {
                return;
            }

            await Task.Delay(100);
        }

        throw new TestFailureException(failureMessage);
    }
}
