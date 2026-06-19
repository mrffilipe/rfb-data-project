namespace RFBDataProject.Domain.Exceptions;

public sealed class DomainNotFoundException : DomainException
{
    public DomainNotFoundException(string message) : base(message)
    {
    }
}
