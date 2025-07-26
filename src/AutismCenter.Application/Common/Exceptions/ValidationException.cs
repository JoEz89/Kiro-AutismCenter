namespace AutismCenter.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ValidationException(IEnumerable<string> failures) : this("One or more validation failures have occurred.", failures)
    {
    }

    public ValidationException(string message, IEnumerable<string> failures) : base(message)
    {
        Failures = failures;
    }

    public IEnumerable<string> Failures { get; } = new List<string>();
}