namespace FileIngestionLab.Domain;

public sealed record SnapshotEnvelope(FileInfo SourceFile, SensorSnapshot Snapshot)
{
    public string SensorId => Snapshot.SensorId;

    public DateTime Timestamp => Snapshot.Timestamp;

    public long Length => SourceFile.Exists ? SourceFile.Length : 0;
}
