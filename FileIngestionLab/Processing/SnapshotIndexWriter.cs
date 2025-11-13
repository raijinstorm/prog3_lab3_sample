using FileIngestionLab.Domain;

namespace FileIngestionLab.Processing;

public sealed class SnapshotIndexWriter
{
    public void WriteIndex(IEnumerable<SnapshotEnvelope> snapshots, FileInfo destination)
    {
        // TODO [Task 7]:
        //  * Create a FileStream + BinaryWriter pair.
        //  * Write a short header (magic bytes + version).
        //  * For every snapshot write:
        //      - sensor id as length-prefixed UTF-8 string
        //      - timestamp ticks (Int64)
        //      - original file length (Int64)
        //      - SHA256 hash of the file content (32 bytes)
        //  * Hash computation should use FileStream + IncrementalHash / SHA256 class.
        //  * Keep the stream open only as long as needed (using statements).
        throw new NotImplementedException();
    }
}
