namespace RFBDataProject.Application.Exceptions;

public static class ApplicationErrorMessages
{
    public static class Search
    {
        public const string QUERY_TOO_SHORT = "Search query must have at least 2 characters.";
        public const string QUERY_TOO_LONG = "Search query max length is 200.";
        public const string INVALID_PAGE = "Page must be greater than zero.";
        public const string INVALID_PAGE_SIZE = "Page size must be between 1 and 100.";
    }

    public static class Ingestion
    {
        public const string SYNC_ALREADY_RUNNING = "Ingestion sync is already running.";
        public const string NO_RELEASE_AVAILABLE = "No ingestion release is available.";
        public const string DATA_NOT_READY = "CNPJ data is not ready yet. Check ingestion status.";
    }

    public static class Lookup
    {
        public const string QUERY_TOO_SHORT = "Lookup query must have at least 1 character.";
    }
}
