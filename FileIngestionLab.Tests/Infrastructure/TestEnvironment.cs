using System.Runtime.CompilerServices;

namespace FileIngestionLab.Tests.Infrastructure;

public static class TestEnvironment
{
    public static string CreateUniqueDirectory([CallerMemberName] string? testName = null)
    {
        testName ??= Guid.NewGuid().ToString("n");
        var root = Path.Combine(Path.GetTempPath(), "FileIngestionLab.Tests", testName, Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(root);
        return root;
    }

    public static FileInfo CreateFile(string directory, string fileName, string contents)
    {
        var path = Path.Combine(directory, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, contents);
        return new FileInfo(path);
    }

    public static async Task<FileInfo> CreateFileAsync(string directory, string fileName, string contents)
    {
        var path = Path.Combine(directory, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, contents);
        return new FileInfo(path);
    }

    public static void WriteBinary(string path, ReadOnlySpan<byte> data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, data.ToArray());
    }

    public static void Cleanup(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup failures to avoid hiding test results.
        }
    }
}
