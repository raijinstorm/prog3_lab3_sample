using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace FileIngestionLab.Tests.Infrastructure;

public static class AssertEx
{
    public static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new TestFailureException(message);
        }
    }

    public static void False(bool condition, string message)
        => True(!condition, message);

    public static void Equal<T>(T expected, T actual, string? message = null)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            message ??= $"Expected '{expected}' but found '{actual}'.";
            throw new TestFailureException(message);
        }
    }

    public static void NotEqual<T>(T notExpected, T actual, string? message = null)
    {
        if (EqualityComparer<T>.Default.Equals(notExpected, actual))
        {
            message ??= $"Did not expect '{notExpected}'.";
            throw new TestFailureException(message);
        }
    }

    public static void NotNull<T>([NotNull] T? value, string message)
    {
        if (value is null)
        {
            throw new TestFailureException(message);
        }
    }

    public static void Null(object? value, string message)
    {
        if (value is not null)
        {
            throw new TestFailureException(message);
        }
    }

    public static void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string? message = null)
    {
        using var e1 = expected.GetEnumerator();
        using var e2 = actual.GetEnumerator();

        var index = 0;
        while (true)
        {
            var moved1 = e1.MoveNext();
            var moved2 = e2.MoveNext();

            if (!moved1 || !moved2)
            {
                if (moved1 != moved2)
                {
                    message ??= "Collections have different lengths.";
                    throw new TestFailureException(message);
                }

                break;
            }

            if (!EqualityComparer<T>.Default.Equals(e1.Current, e2.Current))
            {
                message ??= $"Collections differ at index {index}. Expected '{e1.Current}' but found '{e2.Current}'.";
                throw new TestFailureException(message);
            }

            index++;
        }
    }

    public static void DictionaryEqual<TKey, TValue>(
        IReadOnlyDictionary<TKey, TValue> expected,
        IReadOnlyDictionary<TKey, TValue> actual,
        string? message = null)
        where TKey : notnull
    {
        if (expected.Count != actual.Count)
        {
            message ??= $"Dictionary counts differ. Expected {expected.Count} but found {actual.Count}.";
            throw new TestFailureException(message);
        }

        foreach (var pair in expected)
        {
            if (!actual.TryGetValue(pair.Key, out var value))
            {
                message ??= $"Missing key '{pair.Key}'.";
                throw new TestFailureException(message);
            }

            if (!EqualityComparer<TValue>.Default.Equals(pair.Value, value))
            {
                message ??= $"Value mismatch for key '{pair.Key}'. Expected '{pair.Value}' but found '{value}'.";
                throw new TestFailureException(message);
            }
        }
    }

    public static async Task ThrowsAsync<TException>(Func<Task> action, string message)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException)
        {
            return;
        }
        catch (Exception ex)
        {
            throw new TestFailureException($"Expected {typeof(TException).Name} but caught {ex.GetType().Name}: {ex.Message}");
        }

        throw new TestFailureException(message);
    }

    public static void ApproximatelyEqual(double expected, double actual, double tolerance, string? message = null)
    {
        if (double.IsNaN(expected) || double.IsNaN(actual))
        {
            throw new TestFailureException("Values must not be NaN.");
        }

        if (Math.Abs(expected - actual) > tolerance)
        {
            message ??= $"Expected {expected} ± {tolerance} but found {actual}.";
            throw new TestFailureException(message);
        }
    }
}
