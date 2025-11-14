using System.IO.Compression;
using FileIngestionLab.Domain;
using FileIngestionLab.Processing;
using FileIngestionLab.Tests.Infrastructure;

namespace FileIngestionLab.Tests.Specs;

public static class Part6_SnapshotArchiveBuilderTests
{
    public static async Task WritesCompressedArchiveAsync()
    {
        var root = TestEnvironment.CreateUniqueDirectory();
        try
        {
            var file1 = await TestEnvironment.CreateFileAsync(root, "alpha-2025-02-10T09-00.sensor", """
# header
sensor=alpha
timestamp=2025-02-10T09:00:00Z

[measurements]
a=1

[diagnostics]
x=foo
""");

            var file2 = await TestEnvironment.CreateFileAsync(root, "beta-2025-02-10T09-30.sensor", """
# header
sensor=beta
timestamp=2025-02-10T09:30:00Z

[measurements]
a=2

[diagnostics]
y=bar
""");

            var reader = new SnapshotReader();
            var envelopes = new List<SnapshotEnvelope>
            {
                new(file1, reader.ReadSnapshot(file1)),
                new(file2, reader.ReadSnapshot(file2)),
            };

            var archive = new FileInfo(Path.Combine(root, "archive.gz"));
            new SnapshotArchiveBuilder().WriteArchive(envelopes, archive);

            AssertEx.True(archive.Exists, "Archive builder should create the destination file.");
            AssertEx.True(archive.Length > 0, "Archive should not be empty.");

            using var stream = archive.OpenRead();
            using var gzip = new GZipStream(stream, CompressionMode.Decompress);
            using var readerStream = new StreamReader(gzip);
            var content = await readerStream.ReadToEndAsync();

            AssertEx.True(content.Contains("--- alpha 2025-02-10T09:00:00.0000000Z"), "Archive should include alpha header with timestamp.");
            AssertEx.True(content.Contains("--- beta 2025-02-10T09:30:00.0000000Z"), "Archive should include beta header with timestamp.");
            AssertEx.True(content.Contains("sensor=alpha"), "Archive should embed original file contents.");
            AssertEx.True(content.Contains("sensor=beta"), "Archive should embed original file contents.");
        }
        finally
        {
            TestEnvironment.Cleanup(root);
        }
    }
}
