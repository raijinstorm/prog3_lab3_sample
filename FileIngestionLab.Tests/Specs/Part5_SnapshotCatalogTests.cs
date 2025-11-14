using FileIngestionLab.Processing;
using FileIngestionLab.Tests.Infrastructure;

namespace FileIngestionLab.Tests.Specs;

public static class Part5_SnapshotCatalogTests
{
    public static Task EnumeratesChronologicallyAsync()
    {
        var root = TestEnvironment.CreateUniqueDirectory();
        try
        {
            var early = TestEnvironment.CreateFile(root, "alpha-2025-02-10T08-30.sensor", "content");
            var mid = TestEnvironment.CreateFile(root, "alpha-2025-02-10T09-00.sensor", "content");
            var nestedDir = Path.Combine(root, "nested");
            Directory.CreateDirectory(nestedDir);
            var late = TestEnvironment.CreateFile(nestedDir, "alpha-2025-02-10T10-30.sensor", "content");

            var catalog = new SnapshotCatalog(new DirectoryInfo(root));
            var files = catalog.EnumerateSnapshotFiles().ToList();

            AssertEx.SequenceEqual(
                new[] { early.FullName, mid.FullName, late.FullName },
                files.Select(f => f.FullName),
                "SnapshotCatalog should enumerate files in chronological order across directories.");
        }
        finally
        {
            TestEnvironment.Cleanup(root);
        }

        return Task.CompletedTask;
    }

    public static Task BuildsSensorIndexAsync()
    {
        var root = TestEnvironment.CreateUniqueDirectory();
        try
        {
            var alphaEarly = TestEnvironment.CreateFile(root, "alpha-2025-02-10T08-30.sensor", "content");
            var alphaLate = TestEnvironment.CreateFile(root, "alpha-2025-02-10T10-30.sensor", "content");
            var beta = TestEnvironment.CreateFile(root, "beta-2025-02-10T09-15.sensor", "content");

            var catalog = new SnapshotCatalog(new DirectoryInfo(root));
            var index = catalog.BuildSensorIndex();

            AssertEx.Equal(2, index.Count, "Index should contain one entry per sensor.");

            var alphaList = index["alpha"]; // Should throw if missing, which fails test.
            AssertEx.SequenceEqual(
                new[] { alphaEarly.FullName, alphaLate.FullName },
                alphaList.Select(f => f.FullName),
                "Alpha snapshots should be ordered chronologically.");

            var betaList = index["beta"];
            AssertEx.SequenceEqual(
                new[] { beta.FullName },
                betaList.Select(f => f.FullName),
                "Beta should have a single snapshot entry.");
        }
        finally
        {
            TestEnvironment.Cleanup(root);
        }

        return Task.CompletedTask;
    }
}
