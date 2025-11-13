using FileIngestionLab.Domain;
using FileIngestionLab.Monitoring;
using FileIngestionLab.Processing;

namespace FileIngestionLab.Pipelines;

public sealed class DropPipeline : IDisposable
{
    private readonly DropFolderMonitor _monitor;
    private readonly SnapshotReader _reader;
    private readonly SnapshotCatalog _catalog;
    private readonly SnapshotArchiveBuilder _archiveBuilder;
    private readonly SnapshotIndexWriter _indexWriter;
    private readonly LogBook _logBook;
    private readonly List<SnapshotEnvelope> _buffer = [];

    public DropPipeline(
        DropFolderMonitor monitor,
        SnapshotReader reader,
        SnapshotCatalog catalog,
        SnapshotArchiveBuilder archiveBuilder,
        SnapshotIndexWriter indexWriter,
        LogBook logBook)
    {
        _monitor = monitor;
        _reader = reader;
        _catalog = catalog;
        _archiveBuilder = archiveBuilder;
        _indexWriter = indexWriter;
        _logBook = logBook;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO [Task 3]:
        //  * Subscribe to monitor events (FileReady, FileSkipped, MonitorError).
        //  * Start the monitor and keep it alive until cancellation is requested.
        //  * For each ready file:
        //      - Parse the snapshot (SnapshotReader).
        //      - Stage envelopes in _buffer.
        //      - Periodically flush the buffer to archive/index/log once N files arrive
        //        or the cancellation token is triggered.
        //  * Remember to unsubscribe and dispose resources.
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _monitor.Dispose();
    }
}
