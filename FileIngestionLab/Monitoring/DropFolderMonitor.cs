using System.Collections.Concurrent;

namespace FileIngestionLab.Monitoring;

public sealed class DropFolderMonitor : IDisposable
{
    private readonly string _path;
    private readonly DropFolderMonitorOptions _options;
    private FileSystemWatcher? _watcher;
    private readonly ConcurrentDictionary<string, DateTimeOffset> _pending = new(
        StringComparer.OrdinalIgnoreCase);

    public DropFolderMonitor(string path, DropFolderMonitorOptions options)
    {
        _path = path ?? throw new ArgumentNullException(nameof(path));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public event EventHandler<FileReadyEventArgs>? FileReady;

    public event EventHandler<FileSkippedEventArgs>? FileSkipped;

    public event EventHandler<MonitoringErrorEventArgs>? MonitorError;

    public void Start()
    {
        // TODO [Task 2]:
        //  * Instantiate FileSystemWatcher using options.
        //  * Wire up Created / Renamed / Changed / Error events.
        //  * Use a timer/Task to re-check file size after StabilizationDelay to ensure writes finished.
        //  * Enforce allowed extensions and raise FileSkipped when necessary.
        //  * Raise FileReady once the file stopped growing.
        throw new NotImplementedException();
    }

    public void Stop()
    {
        // TODO [Task 2]: gracefully shut down the watcher and dispose of resources.
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        Stop();
    }
}
