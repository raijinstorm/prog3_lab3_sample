# Lab 3: Events, Files, Filesystem, Streams

> [!NOTE]
> You have 80 minutes for this lab. The repository already contains the starter code in `lab3_sample_task/FileIngestionLab`. All changes for this lab must stay inside the `lab3_sample_task` folder.

## Scenario

You are extending a telemetry ingestion service for a fleet of environmental sensors. Each sensor periodically drops a `.sensor` file into a shared folder. The ingestion service must watch that folder, validate every file, parse its contents, and continuously build lightweight archives plus metadata that other teams can query. The sample data used by automated tests lives under `lab3_sample_task/sample-data`, but you can place your own files there while developing.

The starter project contains one console app with toggleable demo sections in `Program.cs`, detailed XML comments inside the helper files, and a total of 12 points split across two independent modules. Focus on mastering the event, filesystem, and stream APIs using the starter code base located in this directory.

## Sample Data Format

Each `.sensor` file follows this simplified structure:

```
# sensor snapshot
sensor=weather-alpha
timestamp=2025-02-10T09:00:00Z
site=rooftop

[measurements]
temperature=21.5
humidity=45
pressure=1012

[diagnostics]
battery=93
signal=-71
```

- Header: free-form comment line (starts with `#`).
- Metadata section: key/value pairs until the `[measurements]` marker.
- Measurement section: numeric values that must be parsed as `double`.
- Diagnostics section: values remain strings.

You can reuse the files in `sample-data/snapshots` or drop new ones into `sample-data/drop` while debugging.

## Starter Layout

```
lab3_sample_task/
├─ FileIngestionLab.sln
├─ FileIngestionLab/             # Console app with Program.cs
│  ├─ Domain/                    # SensorSnapshot & SnapshotEnvelope
│  ├─ Monitoring/                # DropFolderMonitor and event args
│  ├─ Pipelines/                 # DropPipeline orchestrator
│  ├─ Processing/                # Reader, catalog, archive/index builders, log
│  └─ Utilities/ProjectPaths.cs  # Resolves sample-data directories
└─ sample-data/
   ├─ drop/                      # Folder watched by DropFolderMonitor
   ├─ snapshots/                 # Static fixtures for manual tests
   └─ artifacts/                 # Expected output location (archives / logs)
```

`Program.cs` shows how the pieces will be wired together once everything works. Two `#define` flags (`DEMO_MONITOR`, `DEMO_ARCHIVE`) are commented out—you can enable them after completing the implementation to manually verify your solution.

---

## Part 1 – Event-Driven Drop Monitor (4.5 pts)

This block focuses on events + filesystem notifications. All work happens inside `FileIngestionLab/Monitoring` and `FileIngestionLab/Pipelines`.

1. **Task 1 – Event payloads (1 pt)**  
   File: `Monitoring/FileDropEventArgs.cs`  
   Implement strongly typed `EventArgs` for the monitor API. Follow the event pattern described in the *Events* lecture:
   - `FileReadyEventArgs` should expose the discovered `FileInfo`, discovery timestamp (`DateTimeOffset`), stabilized file length, and optional metadata (e.g., hash or reason string).
   - `FileSkippedEventArgs` should include the file path and a human-readable reason.
   - `MonitoringErrorEventArgs` should wrap the underlying `Exception`.  
   Keep constructors protected/internal as needed so `DropFolderMonitor` can raise the events.

2. **Task 2 – DropFolderMonitor (2 pts)**  
   File: `Monitoring/DropFolderMonitor.cs`  
   Create a `FileSystemWatcher` that observes the drop folder specified in the constructor and raises the three events:
   - Validate file extensions using `DropFolderMonitorOptions.AllowedExtensions`.
   - Use the `StabilizationDelay` to make sure a file is no longer growing (store last seen size/timestamp in `_pending` and re-check using `Task.Delay` or a timer).
   - Wire `Error` to `MonitoringErrorEventArgs`.
    - Remember to call `IncludeSubdirectories`, `EnableRaisingEvents`, and to dispose the watcher in `Stop()` / `Dispose()`.  
    Tips: follow the sample code from the Filesystem lecture about `FileSystemWatcher` and respect the guidance from the Events lecture about raising events through `On...` helpers.

3. **Task 3 – DropPipeline (1.5 pts)**  
   File: `Pipelines/DropPipeline.cs`  
   Build a small orchestrator that subscribes to the monitor:
   - On `FileReady`, parse the file with `SnapshotReader`, stash `SnapshotEnvelope` instances in `_buffer`, and when either five files accumulate or cancellation is requested, call the processing block (Tasks 6–8).
   - On `FileSkipped`, print the reason to the console (or redirect to the log).
   - On `MonitorError`, stop the pipeline and rethrow/cancel.  
    Hold the monitor alive until the `CancellationToken` fires, then flush the buffer once more. Follow general resource cleanup advice (unsubscribe, dispose).

## Part 2 – Snapshot Processing (7.5 pts)

This block covers file parsing plus stream/binary work. All files mentioned below are under `FileIngestionLab/Processing`.

4. **Task 4 – SnapshotReader (1.5 pts)**  
   File: `Processing/SnapshotReader.cs`  
   Parse a `.sensor` file using `FileStream` + `StreamReader`. Follow the format described earlier:
   - Ignore empty lines and treat keys as case-sensitive.
   - Use `ReadLine` in a loop; do not load the whole file with `ReadAllText`.
   - Measurements must be parsed with `double.Parse(..., CultureInfo.InvariantCulture)` and stored in a dictionary.
   - Diagnostics remain strings.  
   Return a `SensorSnapshot` filled with metadata (sensor id, timestamp, optional site).

5. **Task 5 – SnapshotCatalog (1.5 pts)**  
   File: `Processing/SnapshotCatalog.cs`  
   Provide filesystem utilities using `Directory.EnumerateFiles` from the *Filesystem* lecture:
   - `EnumerateSnapshotFiles()` should lazily enumerate `*.sensor` files within the provided root (recursively), sorted by the timestamp suffix contained in the file name (assume `sensorId-YYYY-MM-DDTHH-mm.sensor` naming).
   - `BuildSensorIndex()` should group the files per sensor id (prefix before the first `-`) and order each list chronologically.  
   Pay attention to nonexistent files disappearing between enumeration and processing.

6. **Task 6 – SnapshotArchiveBuilder (1.5 pts)**  
   File: `Processing/SnapshotArchiveBuilder.cs`  
    Using a decorator stack inspired by the Streams lecture, stream the raw snapshot bytes into a compressed `.gz` file:
   - Wrap a `FileStream` inside `BufferedStream` + `GZipStream`.
   - For every snapshot write a header line (`--- sensor timestamp length ---`) followed by the original file contents and a blank line.
   - Never read an entire file into memory—copy using a reusable buffer.

7. **Task 7 – SnapshotIndexWriter (1.5 pts)**  
   File: `Processing/SnapshotIndexWriter.cs`  
   Produce a compact binary index using `BinaryWriter`:
   - Start with a 4-byte magic number (`SNP3`) and a single byte with the version (`1`).
   - For each snapshot write the sensor id as length-prefixed UTF-8, the timestamp (`long` ticks), byte length (`long`), and a SHA-256 hash of the file contents.
   - Use `FileStream` + `IncrementalHash` (or `SHA256.Create`) so hashing also streams the data.

8. **Task 8 – LogBook (1.5 pts)**  
   File: `Processing/LogBook.cs`  
   Append a textual summary after every flush:
   - Use `File.AppendText` / `StreamWriter` to write a section with a timestamp, number of files processed, archive path & size, index path & size, and a short per-sensor breakdown (counts).
   - Separate runs with `----`.
   - Ensure the log file is created in `sample-data/artifacts/lab3.log`.

---

### Scoring Summary

| Task | Topic Focus | Points |
| --- | --- | --- |
| 1 | Custom events | 1.0 |
| 2 | FileSystemWatcher + events | 2.0 |
| 3 | Event-driven pipeline | 1.5 |
| 4 | Streams (text parsing) | 1.5 |
| 5 | Filesystem queries | 1.5 |
| 6 | Stream decorators + compression | 1.5 |
| 7 | Binary streams + hashing | 1.5 |
| 8 | StreamWriter facade | 1.5 |

**Total: 12 points.** Complete the tasks in any order; parts remain independent so you can focus on whichever module you prefer. When you are ready to validate your work, run `dotnet run --project lab3_sample_task/FileIngestionLab` and enable the relevant `#define` in `Program.cs` to exercise either the monitor or the processing pipeline.
