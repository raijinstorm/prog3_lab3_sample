using FileIngestionLab.Domain;

namespace FileIngestionLab.Processing;

public sealed class LogBook
{
    private readonly FileInfo _target;

    public LogBook(FileInfo target)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
    }

    public void AppendSummary(
        IReadOnlyCollection<SnapshotEnvelope> snapshots,
        FileInfo archive,
        FileInfo indexFile)
    {
        // TODO [Task 8]:
        //  * Use File.AppendText or StreamWriter to log ingestion summary.
        //  * Include timestamp, number of processed files, archive path/size, index path/size.
        //  * Append one section per ingestion run, separated by "----".
        throw new NotImplementedException();
    }
}
