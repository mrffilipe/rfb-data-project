using RFBDataProject.Domain.Common;
using RFBDataProject.Domain.Exceptions;

namespace RFBDataProject.Domain.Entities;

public sealed class LookupEntry : BaseEntity
{
    public string Code { get; private set; } = default!;
    public string Description { get; private set; } = default!;

    private LookupEntry()
    {
    }

    public static LookupEntry Create(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainValidationException(DomainErrorMessages.Lookup.CODE_REQUIRED);
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainValidationException(DomainErrorMessages.Lookup.DESCRIPTION_REQUIRED);

        var entry = new LookupEntry
        {
            Code = code.Trim(),
            Description = description.Trim()
        };
        entry.SetCreatedAt();
        return entry;
    }
}
