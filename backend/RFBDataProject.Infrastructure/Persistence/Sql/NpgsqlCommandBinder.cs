using Npgsql;

namespace RFBDataProject.Infrastructure.Persistence.Sql;

internal static class NpgsqlCommandBinder
{
    public static void AddParameters(
        NpgsqlCommand command,
        IEnumerable<NpgsqlParameter> parameters,
        bool excludePaging = false)
    {
        foreach (var parameter in parameters)
        {
            if (excludePaging && parameter.ParameterName is "limit" or "offset")
                continue;

            command.Parameters.AddWithValue(parameter.ParameterName!, parameter.Value ?? DBNull.Value);
        }
    }
}
