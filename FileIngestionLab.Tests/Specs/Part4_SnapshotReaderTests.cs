using FileIngestionLab.Domain;
using FileIngestionLab.Processing;
using FileIngestionLab.Tests.Infrastructure;

namespace FileIngestionLab.Tests.Specs;

public static class Part4_SnapshotReaderTests
{
    public static async Task ParsesSensorFileAsync()
    {
        var directory = TestEnvironment.CreateUniqueDirectory();
        try
        {
            var file = await TestEnvironment.CreateFileAsync(directory, "weather-alpha-2025-02-10T09-00.sensor", """
# sensor snapshot
sensor=weather-alpha
timestamp=2025-02-10T09:00:00Z
site=rooftop

[measurements]
temperature=21.5
humidity=45.1
pressure=1012

[diagnostics]
battery=93
signal=-71
""");

            var reader = new SnapshotReader();
            var snapshot = reader.ReadSnapshot(file);

            AssertEx.Equal("weather-alpha", snapshot.SensorId, "Sensor identifier should be parsed from metadata.");
            AssertEx.Equal(new DateTime(2025, 2, 10, 9, 0, 0, DateTimeKind.Utc), snapshot.Timestamp, "Timestamp should be parsed as UTC instant.");
            AssertEx.Equal("rooftop", snapshot.Site, "Site metadata should be captured.");

            AssertEx.ApproximatelyEqual(21.5, snapshot.GetMeasurement("temperature")!.Value, 0.001, "Temperature measurement missing or incorrect.");
            AssertEx.ApproximatelyEqual(45.1, snapshot.GetMeasurement("humidity")!.Value, 0.001, "Humidity measurement missing or incorrect.");
            AssertEx.ApproximatelyEqual(1012.0, snapshot.GetMeasurement("pressure")!.Value, 0.001, "Pressure measurement missing or incorrect.");

            AssertEx.Equal("93", snapshot.Diagnostics["battery"], "Diagnostic battery value should be read as string.");
            AssertEx.Equal("-71", snapshot.Diagnostics["signal"], "Diagnostic signal value should be read as string.");
        }
        finally
        {
            TestEnvironment.Cleanup(directory);
        }
    }

    public static async Task RejectsMalformedMeasurementsAsync()
    {
        var directory = TestEnvironment.CreateUniqueDirectory();
        try
        {
            var file = await TestEnvironment.CreateFileAsync(directory, "weather-alpha-2025-02-10T09-00.sensor", """
# header
sensor=weather-alpha
timestamp=2025-02-10T09:00:00Z

[measurements]
temperature=twenty
""");

            var reader = new SnapshotReader();
            await AssertEx.ThrowsAsync<FormatException>(() => Task.FromResult(reader.ReadSnapshot(file)), "Invalid measurement values should trigger FormatException.");
        }
        finally
        {
            TestEnvironment.Cleanup(directory);
        }
    }
}
