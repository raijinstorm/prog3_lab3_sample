using System.Security.Cryptography;
using FileIngestionLab.Domain;
using FileIngestionLab.Processing;
using FileIngestionLab.Tests.Infrastructure;

namespace FileIngestionLab.Tests.Specs;

public static class Part7_SnapshotIndexWriterTests
{
    public static async Task WritesBinaryIndexAsync()
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
""");
            var file2 = await TestEnvironment.CreateFileAsync(root, "beta-2025-02-10T09-30.sensor", """
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

            var indexFile = new FileInfo(Path.Combine(root, "index.bin"));
            new SnapshotIndexWriter().WriteIndex(envelopes, indexFile);

            AssertEx.True(indexFile.Exists, "Index writer should create the destination file.");

            using var stream = indexFile.OpenRead();
            using var binary = new BinaryReader(stream);

            var magic = binary.ReadBytes(4);
            AssertEx.SequenceEqual(new byte[] { (byte)'S', (byte)'N', (byte)'P', (byte)'3' }, magic, "Magic header must be SNP3.");

            var version = binary.ReadByte();
            AssertEx.Equal((byte)1, version, "Index version should be 1.");

            foreach (var envelope in envelopes)
            {
                var sensorId = binary.ReadString();
                var ticks = binary.ReadInt64();
                var length = binary.ReadInt64();
                var hash = binary.ReadBytes(32);

                AssertEx.Equal(envelope.SensorId, sensorId, "Index should record the sensor identifier.");
                AssertEx.Equal(envelope.Timestamp.Ticks, ticks, "Index should record the snapshot timestamp ticks.");
                AssertEx.Equal(envelope.Length, length, "Index should record the original file length.");

                using var fileStream = envelope.SourceFile.OpenRead();
                var expectedHash = SHA256.HashData(fileStream);
                AssertEx.SequenceEqual(expectedHash, hash, "Index should record a SHA-256 hash of the snapshot file.");
            }

            AssertEx.True(stream.Position == stream.Length, "Index file should not contain trailing data.");
        }
        finally
        {
            TestEnvironment.Cleanup(root);
        }
    }
}
