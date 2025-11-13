namespace FileIngestionLab.Monitoring;

/// <summary>
/// TODO [Task 1]: implement strongly-typed EventArgs for monitor events.
/// The minimum expectation:
/// <list type="bullet">
/// <item><description><see cref="FileReadyEventArgs"/> exposes <see cref="FileInfo"/>, discovery timestamp, and file length.</description></item>
/// <item><description><see cref="FileSkippedEventArgs"/> explains why a file was ignored (extension, missing, etc.).</description></item>
/// <item><description><see cref="MonitoringErrorEventArgs"/> wraps internal <see cref="Exception"/> instances.</description></item>
/// </list>
/// </summary>
public sealed class FileReadyEventArgs : EventArgs
{
}

public sealed class FileSkippedEventArgs : EventArgs
{
}

public sealed class MonitoringErrorEventArgs : EventArgs
{
}
