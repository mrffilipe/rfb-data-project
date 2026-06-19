namespace RFBDataProject.Application.Exceptions;

public sealed class ApplicationValidationException : Exception
{
    public ApplicationValidationException(string message) : base(message)
    {
    }
}

public sealed class ApplicationNotFoundException : Exception
{
    public ApplicationNotFoundException(string message) : base(message)
    {
    }
}
