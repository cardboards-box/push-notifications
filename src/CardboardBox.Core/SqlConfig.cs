using Npgsql;

namespace CardboardBox.Core;

internal class SqlConfig : ISqlConfig<NpgsqlConnection>
{
    private readonly IConfiguration _config;

    public string ConnectionString => _config.Required("Database:ConnectionString");

    public int Timeout => _config.OptionalInt("Database:Timeout", 0);

    public SqlConfig(IConfiguration config)
    {
        _config = config;
    }
}