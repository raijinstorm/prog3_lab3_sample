using System.IO.Compression;
using FileIngestionLab.Domain;

namespace FileIngestionLab.Processing;

public sealed class SnapshotArchiveBuilder
{
    public void WriteArchive(IEnumerable<SnapshotEnvelope> snapshots, FileInfo destination)
    {
        // TODO [Task 6]:
        //  * Use FileStream + GZipStream (+ optionally BufferedStream) as described in the streams lecture.
        //  * Stream the original snapshot bytes without loading them all to memory.
        //  * For each input file write:
        //      - A header line: --- {sensorId} {timestamp:o} {length} ---
        //      - Raw file contents.
        //      - A blank line between entries.
        //  * Ensure the target directory exists and the file is overwritten if it already exists.
        throw new NotImplementedException();
    }
}
