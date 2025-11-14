using FileIngestionLab.Domain;
using FileIngestionLab.Processing;
using FileIngestionLab.Tests.Infrastructure;

namespace FileIngestionLab.Tests.Specs;

public static class Part8_LogBookTests
{
    public static async Task AppendsSummaryAsync()
    {
        var root = TestEnvironment.CreateUniqueDirectory();
        try
        {
            var snapshotDir = Path.Combine(root, "snapshots");
            Directory.CreateDirectory(snapshotDir);

            var file1 = await TestEnvironment.CreateFileAsync(snapshotDir, "alpha-2025-02-10T09-00.sensor", """
# header
sensor=alpha
timestamp=2025-02-10T09:00:00Z

[measurements]
a=1
""");
            var file2 = await TestEnvironment.CreateFileAsync(snapshotDir, "beta-2025-02-10T09-30.sensor", """
# header
sensor=beta
timestamp=2025-02-10T09:30:00Z

[measurements]
a=2
""");

            var reader = new SnapshotReader();
            var envelopes = new List<SnapshotEnvelope>
            {
                new(file1, reader.ReadSnapshot(file1)),
                new(file2, reader.ReadSnapshot(file2)),
            };

            var artifacts = Path.Combine(root, "artifacts");
            Directory.CreateDirectory(artifacts);

            var archive = new FileInfo(Path.Combine(artifacts, "archive.gz"));
            await File.WriteAllBytesAsync(archive.FullName, new byte[] { 1, 2, 3, 4 });

            var index = new FileInfo(Path.Combine(artifacts, "index.bin"));
            await File.WriteAllBytesAsync(index.FullName, new byte[] { 5, 6, 7 });

            var logFile = new FileInfo(Path.Combine(artifacts, "lab3.log"));
            var logBook = new LogBook(logFile);

            logBook.AppendSummary(envelopes, archive, index);

            var logContent = await File.ReadAllTextAsync(logFile.FullName);
            AssertEx.True(logContent.Contains("Files processed: 2"), "Log should report number of processed files.");
            AssertEx.True(logContent.Contains(archive.FullName), "Log should include archive path.");
            AssertEx.True(logContent.Contains(index.FullName), "Log should include index path.");
            AssertEx.True(logContent.Contains("alpha: 1"), "Log should summarize counts per sensor.");
            AssertEx.True(logContent.Contains("beta: 1"), "Log should summarize counts per sensor.");

            logBook.AppendSummary(envelopes, archive, index);
            var secondRun = await File.ReadAllTextAsync(logFile.FullName);
            AssertEx.True(secondRun.Contains("----"), "Subsequent runs should be separated by ----.");
        }
        finally
        {
            TestEnvironment.Cleanup(root);
        }
    }
}
