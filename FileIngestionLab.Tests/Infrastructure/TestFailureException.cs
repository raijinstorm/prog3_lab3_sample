namespace FileIngestionLab.Tests.Infrastructure;

public sealed class TestFailureException : Exception
{
    public TestFailureException(string message)
        : base(message)
    {
    }
}
