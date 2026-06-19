namespace RFBDataProject.Domain.Exceptions;

public sealed class DomainBusinessRuleException : DomainException
{
    public DomainBusinessRuleException(string message) : base(message)
    {
    }
}
