using System.Reflection;
using FileIngestionLab.Monitoring;
using FileIngestionLab.Tests.Infrastructure;

namespace FileIngestionLab.Tests.Specs;

public static class Part1_EventArgsTests
{
    public static Task FileReadyEventArgs_CarriesMetadataAsync()
    {
        var type = typeof(FileReadyEventArgs);
        var constructor = FindConstructor(type, typeof(FileInfo), typeof(DateTimeOffset), typeof(long), typeof(string));
        AssertEx.NotNull(constructor, "FileReadyEventArgs must expose a constructor receiving file metadata.");

        var file = new FileInfo(Path.Combine(Path.GetTempPath(), "sample.sensor"));
        var discoveredAt = DateTimeOffset.UtcNow;
        const long length = 12345L;
        const string metadata = "hash:abc";

        var args = (FileReadyEventArgs)constructor!.Invoke(new object?[] { file, discoveredAt, length, metadata });

        var fileProp = GetProperty(type, "File", "Source", "FileInfo");
        AssertEx.NotNull(fileProp, "FileReadyEventArgs should expose the discovered FileInfo.");
        AssertEx.Equal(typeof(FileInfo), fileProp!.PropertyType, "File property must be of type FileInfo.");
        AssertEx.Equal(file.FullName, ((FileInfo)fileProp.GetValue(args)!).FullName, "Unexpected file info value.");

        var timestampProp = GetProperty(type, "DiscoveredAt", "Timestamp", "DiscoveryTime");
        AssertEx.NotNull(timestampProp, "FileReadyEventArgs should expose a discovery timestamp.");
        AssertEx.Equal(typeof(DateTimeOffset), timestampProp!.PropertyType, "Discovery timestamp must use DateTimeOffset.");
        AssertEx.Equal(discoveredAt, (DateTimeOffset)timestampProp.GetValue(args)!, "Unexpected discovery timestamp.");

        var lengthProp = GetProperty(type, "Length", "FileLength", "Size");
        AssertEx.NotNull(lengthProp, "FileReadyEventArgs should expose the stabilized file length.");
        AssertEx.Equal(typeof(long), lengthProp!.PropertyType, "Length property must be of type long.");
        AssertEx.Equal(length, (long)lengthProp.GetValue(args)!, "Unexpected file length value.");

        var metadataProp = GetProperty(type, "Metadata", "Notes", "Details");
        AssertEx.NotNull(metadataProp, "FileReadyEventArgs should expose optional metadata for consumers.");
        AssertEx.Equal(typeof(string), metadataProp!.PropertyType, "Metadata property must be a string.");
        AssertEx.Equal(metadata, (string?)metadataProp.GetValue(args), "Unexpected metadata value.");

        return Task.CompletedTask;
    }

    public static Task FileSkippedEventArgs_StoresReasonAsync()
    {
        var type = typeof(FileSkippedEventArgs);
        var constructor = FindConstructor(type, typeof(string), typeof(string));
        AssertEx.NotNull(constructor, "FileSkippedEventArgs must capture file path and reason.");

        const string path = "/tmp/sample.txt";
        const string reason = "Extension not allowed";

        var args = (FileSkippedEventArgs)constructor!.Invoke(new object?[] { path, reason });

        var pathProp = GetProperty(type, "FilePath", "Path", "File");
        AssertEx.NotNull(pathProp, "FileSkippedEventArgs should expose the skipped file path.");
        AssertEx.Equal(typeof(string), pathProp!.PropertyType, "FilePath property must be a string.");
        AssertEx.Equal(path, (string?)pathProp.GetValue(args), "Unexpected file path value.");

        var reasonProp = GetProperty(type, "Reason", "Message", "Explanation");
        AssertEx.NotNull(reasonProp, "FileSkippedEventArgs should expose a human readable reason.");
        AssertEx.Equal(typeof(string), reasonProp!.PropertyType, "Reason property must be a string.");
        AssertEx.Equal(reason, (string?)reasonProp.GetValue(args), "Unexpected skip reason value.");

        return Task.CompletedTask;
    }

    public static Task MonitoringErrorEventArgs_WrapsExceptionAsync()
    {
        var type = typeof(MonitoringErrorEventArgs);
        var constructor = FindConstructor(type, typeof(Exception));
        AssertEx.NotNull(constructor, "MonitoringErrorEventArgs must expose a constructor receiving an Exception instance.");

        var exception = new InvalidOperationException("boom");
        var args = (MonitoringErrorEventArgs)constructor!.Invoke(new object?[] { exception });

        var exceptionProp = GetProperty(type, "Exception", "Error", "Cause");
        AssertEx.NotNull(exceptionProp, "MonitoringErrorEventArgs should expose the captured exception.");
        AssertEx.Equal(typeof(Exception), exceptionProp!.PropertyType, "Exception property must be of type Exception.");
        AssertEx.Equal(exception, (Exception?)exceptionProp.GetValue(args), "Unexpected exception value.");

        return Task.CompletedTask;
    }

    private static ConstructorInfo? FindConstructor(Type type, params Type[] parameterTypes)
    {
        return type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(c =>
            {
                var parameters = c.GetParameters();
                if (parameters.Length != parameterTypes.Length)
                {
                    return false;
                }

                for (var i = 0; i < parameters.Length; i++)
                {
                    if (!parameters[i].ParameterType.IsAssignableFrom(parameterTypes[i]))
                    {
                        return false;
                    }
                }

                return true;
            });
    }

    private static PropertyInfo? GetProperty(Type type, params string[] names)
    {
        foreach (var name in names)
        {
            var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property is not null)
            {
                return property;
            }
        }

        return null;
    }
}
