namespace RFBDataProject.Application.Services.Companies;

public abstract record CompanyFilterRequest : Common.PagedRequest
{
    public string? Query { get; init; }
    public bool ExcludeQuery { get; init; }

    public string[]? StateCodes { get; init; }
    public bool ExcludeStates { get; init; }

    public string[]? Cnaes { get; init; }
    public bool ExcludeCnaes { get; init; }

    public string[]? LegalNatureCodes { get; init; }
    public bool ExcludeLegalNatureCodes { get; init; }

    public string[]? CompanySizeCodes { get; init; }
    public bool ExcludeCompanySizes { get; init; }

    public string[]? RegistrationStatuses { get; init; }
    public bool ExcludeRegistrationStatuses { get; init; }

    public bool HeadOfficeOnly { get; init; }
    public bool ExcludeHeadOfficeOnly { get; init; }

    public decimal? ShareCapitalMin { get; init; }
    public decimal? ShareCapitalMax { get; init; }
    public bool ExcludeShareCapitalRange { get; init; }
}
