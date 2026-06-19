namespace RFBDataProject.API.Common;

public static class ApiErrorMessages
{
    public const string PROBLEM_JSON_CONTENT_TYPE = "application/problem+json";
    public const string DOMAIN_VALIDATION_TITLE = "Validation error";
    public const string DOMAIN_BUSINESS_RULE_TITLE = "Business rule conflict";
    public const string NOT_FOUND_TITLE = "Resource not found";
    public const string APPLICATION_VALIDATION_TITLE = "Request validation error";
    public const string APPLICATION_NOT_FOUND_TITLE = "Resource not found";
    public const string UNHANDLED_SERVER_ERROR_TITLE = "Internal server error";
    public const string UNEXPECTED_ERROR_DETAIL = "An unexpected error occurred.";
}
