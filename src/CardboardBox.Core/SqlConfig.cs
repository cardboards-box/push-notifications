using Npgsql;

namespace CardboardBox.Core;

/// <summary>
/// Gets the configuration for the SQL service
/// </summary>
internal class SqlConfig : ISqlConfig<NpgsqlConnection>
{
    private readonly IConfiguration _config;

    /// <summary>
    /// The connection string to the database
    /// </summary>
    public string ConnectionString => _config.Required("Database:ConnectionString");

    /// <summary>
    /// The timeout for sql statements in milliseconds
    /// </summary>
    public int Timeout => _config.OptionalInt("Database:Timeout", 0);

    /// <summary>
    /// Gets the configuration for the SQL service
    /// </summary>
    /// <param name="config">The configuration instance to get the variables from</param>
    public SqlConfig(IConfiguration config)
    {
        _config = config;
    }
}