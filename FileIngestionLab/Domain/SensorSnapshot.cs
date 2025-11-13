namespace FileIngestionLab.Domain;

public sealed record SensorSnapshot(
    string SensorId,
    DateTime Timestamp,
    IReadOnlyDictionary<string, double> Measurements,
    IReadOnlyDictionary<string, string> Diagnostics,
    string? Site = null)
{
    public double? GetMeasurement(string key)
        => Measurements.TryGetValue(key, out var value) ? value : null;
}
