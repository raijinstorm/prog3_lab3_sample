using FileIngestionLab.Domain;

namespace FileIngestionLab.Processing;

/// <summary>
/// Parses <c>.sensor</c> files located in <c>sample-data/snapshots</c>.
/// Follow the format explained in README Task 4.
/// </summary>
public sealed class SnapshotReader
{
    public SensorSnapshot ReadSnapshot(FileInfo file)
    {
        // TODO [Task 4]: Use FileStream + StreamReader to parse the snapshot.
        //  * Validate the header (first non-empty line should start with '#').
        //  * Parse key/value pairs until [measurements] and [diagnostics] sections.
        //  * Measurements must be returned as doubles, diagnostics stay as strings.
        //  * Use the provided culture-agnostic parsing rules from the lectures.
        throw new NotImplementedException();
    }
}
