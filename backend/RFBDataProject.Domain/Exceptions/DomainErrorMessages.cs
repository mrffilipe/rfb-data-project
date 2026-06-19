namespace RFBDataProject.Domain.Exceptions;

public static class DomainErrorMessages
{
    public static class Cnpj
    {
        public const string REQUIRED = "CNPJ is required.";
        public const string INVALID_LENGTH = "CNPJ must have 14 digits.";
        public const string INVALID_CHECK_DIGITS = "CNPJ check digits are invalid.";
        public const string INVALID_CHARACTERS = "CNPJ must contain only digits.";
    }

    public static class CnpjBase
    {
        public const string REQUIRED = "CNPJ base is required.";
        public const string INVALID_LENGTH = "CNPJ base must have 8 digits.";
        public const string INVALID_CHARACTERS = "CNPJ base must contain only digits.";
    }

    public static class StateCode
    {
        public const string REQUIRED = "State code is required.";
        public const string INVALID_LENGTH = "State code must have 2 characters.";
        public const string INVALID_VALUE = "State code value is invalid.";
    }

    public static class Company
    {
        public const string LEGAL_NAME_REQUIRED = "Legal name is required.";
        public const string CNPJ_BASE_REQUIRED = "CNPJ base is required.";
        public const string NOT_FOUND = "Company not found.";
    }

    public static class Establishment
    {
        public const string CNPJ_BASE_REQUIRED = "CNPJ base is required.";
        public const string CNPJ_ORDER_REQUIRED = "CNPJ order is required.";
        public const string CNPJ_CHECK_DIGIT_REQUIRED = "CNPJ check digit is required.";
        public const string NOT_FOUND = "Establishment not found.";
    }

    public static class Partner
    {
        public const string CNPJ_BASE_REQUIRED = "CNPJ base is required.";
        public const string PARTNER_NAME_REQUIRED = "Partner name is required.";
    }

    public static class Lookup
    {
        public const string CODE_REQUIRED = "Lookup code is required.";
        public const string DESCRIPTION_REQUIRED = "Lookup description is required.";
    }

    public static class IngestionRelease
    {
        public const string REFERENCE_PERIOD_REQUIRED = "Reference period is required.";
        public const string REFERENCE_PERIOD_INVALID_FORMAT = "Reference period format is invalid.";
        public const string NOT_FOUND = "Ingestion release not found.";
        public const string ALREADY_ACTIVE = "Release is already active.";
        public const string INVALID_STATUS_TRANSITION = "Invalid ingestion release status transition.";
    }

    public static class IngestionArtifact
    {
        public const string FILE_NAME_REQUIRED = "Artifact file name is required.";
        public const string TARGET_TABLE_REQUIRED = "Target table is required.";
        public const string NOT_FOUND = "Ingestion artifact not found.";
    }

    public static class IngestionRun
    {
        public const string RELEASE_ID_REQUIRED = "Release id is required.";
    }
}
