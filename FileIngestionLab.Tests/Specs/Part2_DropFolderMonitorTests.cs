using System.Reflection;
using FileIngestionLab.Monitoring;
using FileIngestionLab.Tests.Infrastructure;

namespace FileIngestionLab.Tests.Specs;

public static class Part2_DropFolderMonitorTests
{
    public static async Task FileReady_EventFiresAfterStabilizationAsync()
    {
        var directory = TestEnvironment.CreateUniqueDirectory();
        try
        {
            var options = new DropFolderMonitorOptions
            {
                StabilizationDelay = TimeSpan.FromMilliseconds(150),
                AllowedExtensions = new[] { ".sensor" },
                IncludeSubdirectories = false,
                Filter = "*"
            };

            using var monitor = new DropFolderMonitor(directory, options);
            var readyTcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            monitor.FileReady += (_, args) =>
            {
                readyTcs.TrySetResult(args);
            };

            monitor.Start();

            var filePath = Path.Combine(directory, "alpha.sensor");
            await using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                var block1 = new byte[32];
                Random.Shared.NextBytes(block1);
                await stream.WriteAsync(block1);
                await stream.FlushAsync();
                await Task.Delay(50);

                var block2 = new byte[48];
                Random.Shared.NextBytes(block2);
                await stream.WriteAsync(block2);
            }

            var result = await readyTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var argsType = result!.GetType();
            var fileProp = argsType.GetProperty("File") ?? argsType.GetProperty("Source") ?? argsType.GetProperty("FileInfo");
            AssertEx.NotNull(fileProp, "FileReadyEventArgs should expose a File property.");
            var lengthProp = argsType.GetProperty("Length") ?? argsType.GetProperty("FileLength") ?? argsType.GetProperty("Size");
            AssertEx.NotNull(lengthProp, "FileReadyEventArgs should expose a Length property.");

            var discoveredProp = argsType.GetProperty("DiscoveredAt")
                ?? argsType.GetProperty("Timestamp")
                ?? argsType.GetProperty("DiscoveryTime");
            AssertEx.NotNull(discoveredProp, "FileReadyEventArgs should expose a discovery timestamp.");

            var fileInfo = (FileInfo)fileProp!.GetValue(result)!;
            AssertEx.Equal(new FileInfo(filePath).FullName, fileInfo.FullName, "FileReadyEventArgs should report the matching file.");

            var length = (long)lengthProp!.GetValue(result)!;
            AssertEx.Equal(new FileInfo(filePath).Length, length, "FileReadyEventArgs should report the stabilized file length.");

            var discovered = (DateTimeOffset)discoveredProp!.GetValue(result)!;
            var now = DateTimeOffset.UtcNow;
            AssertEx.True(discovered <= now && discovered >= now - TimeSpan.FromSeconds(5), "Discovery timestamp should reflect current time.");
        }
        finally
        {
            TestEnvironment.Cleanup(directory);
        }
    }

    public static async Task SkipsDisallowedExtensionsAsync()
    {
        var directory = TestEnvironment.CreateUniqueDirectory();
        try
        {
            var options = new DropFolderMonitorOptions
            {
                StabilizationDelay = TimeSpan.FromMilliseconds(50),
                AllowedExtensions = new[] { ".sensor" },
                Filter = "*"
            };

            using var monitor = new DropFolderMonitor(directory, options);
            var skippedTcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            monitor.FileSkipped += (_, args) => skippedTcs.TrySetResult(args);
            monitor.Start();

            var filePath = Path.Combine(directory, "ignored.tmp");
            await File.WriteAllTextAsync(filePath, "hello");

            var result = await skippedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var argsType = result!.GetType();
            var pathProp = argsType.GetProperty("FilePath") ?? argsType.GetProperty("Path") ?? argsType.GetProperty("File");
            AssertEx.NotNull(pathProp, "FileSkippedEventArgs should expose the skipped file path.");
            var reasonProp = argsType.GetProperty("Reason") ?? argsType.GetProperty("Message") ?? argsType.GetProperty("Explanation");
            AssertEx.NotNull(reasonProp, "FileSkippedEventArgs should expose a skip reason.");

            var reportedPath = (string?)pathProp!.GetValue(result);
            AssertEx.Equal(filePath, reportedPath, "Skip event should report the skipped file path.");

            var reason = (string?)reasonProp!.GetValue(result);
            AssertEx.True(!string.IsNullOrWhiteSpace(reason) && reason!.Contains("extension", StringComparison.OrdinalIgnoreCase),
                "Skip reason should mention the extension restriction.");
        }
        finally
        {
            TestEnvironment.Cleanup(directory);
        }
    }
}
