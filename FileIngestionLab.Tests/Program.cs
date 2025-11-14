using FileIngestionLab.Tests.Infrastructure;
using FileIngestionLab.Tests.Specs;

var tests = new (string Name, Func<Task> Run)[]
{
    ("Part1_FileReadyEventArgs_CarriesMetadata", Part1_EventArgsTests.FileReadyEventArgs_CarriesMetadataAsync),
    ("Part1_FileSkippedEventArgs_StoresReason", Part1_EventArgsTests.FileSkippedEventArgs_StoresReasonAsync),
    ("Part1_MonitoringErrorEventArgs_WrapsException", Part1_EventArgsTests.MonitoringErrorEventArgs_WrapsExceptionAsync),
    ("Part2_DropFolderMonitor_RaisesFileReadyAfterStabilization", Part2_DropFolderMonitorTests.FileReady_EventFiresAfterStabilizationAsync),
    ("Part2_DropFolderMonitor_SkipsDisallowedExtensions", Part2_DropFolderMonitorTests.SkipsDisallowedExtensionsAsync),
    ("Part3_DropPipeline_FlushesBufferOnBatchSize", Part3_DropPipelineTests.FlushesBufferOnBatchSizeAsync),
    ("Part3_DropPipeline_FlushesOnCancellation", Part3_DropPipelineTests.FlushesOnCancellationAsync),
    ("Part4_SnapshotReader_ParsesSensorFile", Part4_SnapshotReaderTests.ParsesSensorFileAsync),
    ("Part4_SnapshotReader_RejectsMalformedMeasurements", Part4_SnapshotReaderTests.RejectsMalformedMeasurementsAsync),
    ("Part5_SnapshotCatalog_EnumeratesChronologically", Part5_SnapshotCatalogTests.EnumeratesChronologicallyAsync),
    ("Part5_SnapshotCatalog_BuildsSensorIndex", Part5_SnapshotCatalogTests.BuildsSensorIndexAsync),
    ("Part6_SnapshotArchiveBuilder_WritesCompressedArchive", Part6_SnapshotArchiveBuilderTests.WritesCompressedArchiveAsync),
    ("Part7_SnapshotIndexWriter_WritesBinaryIndex", Part7_SnapshotIndexWriterTests.WritesBinaryIndexAsync),
    ("Part8_LogBook_AppendsSummary", Part8_LogBookTests.AppendsSummaryAsync),
};

var failures = new List<string>();
var stopwatch = new System.Diagnostics.Stopwatch();

foreach (var (name, run) in tests)
{
    Console.WriteLine($"[ RUN      ] {name}");
    stopwatch.Restart();
    try
    {
        await run();
        stopwatch.Stop();
        Console.WriteLine($"[     PASS ] {name} ({stopwatch.ElapsedMilliseconds} ms)");
    }
    catch (TestFailureException ex)
    {
        stopwatch.Stop();
        failures.Add(name);
        Console.WriteLine($"[   FAILED ] {name} ({stopwatch.ElapsedMilliseconds} ms)");
        Console.WriteLine($"             {ex.Message}");
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        failures.Add(name);
        Console.WriteLine($"[   FAILED ] {name} ({stopwatch.ElapsedMilliseconds} ms)");
        Console.WriteLine($"             Unexpected exception: {ex}");
    }
}

Console.WriteLine();
if (failures.Count == 0)
{
    Console.WriteLine($"ALL TESTS PASSED ({tests.Length}).");
    return 0;
}
else
{
    Console.WriteLine($"{failures.Count} TEST(S) FAILED:");
    foreach (var failure in failures)
    {
        Console.WriteLine($" - {failure}");
    }

    return 1;
}
