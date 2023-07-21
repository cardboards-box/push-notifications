namespace CardboardBox.PushNotifications.Database.Services;

using Models;

/// <summary>
/// The service for interacting with <see cref="Application"/> db objects.
/// </summary>
public interface IApplicationDbService : IOrmMap<Application> 
{
    /// <summary>
    /// Fetches an application by its secret key.
    /// </summary>
    /// <param name="key">The secret key</param>
    /// <returns>The application</returns>
    Task<Application?> GetByKey(string key);
}

/// <summary>
/// The implementation of the <see cref="IApplicationDbService"/>
/// </summary>
public class ApplicationDbService : OrmMap<Application>, IApplicationDbService
{
    private static string? _getByKey;

    /// <summary>
    /// The implementation of the <see cref="IApplicationDbService"/>
    /// </summary>
    /// <param name="query">The service to generate queries</param>
    /// <param name="sql">The service that executes SQL queries</param>
    public ApplicationDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }

    /// <summary>
    /// Fetches an application by its secret key.
    /// </summary>
    /// <param name="key">The secret key</param>
    /// <returns>The application</returns>
    public Task<Application?> GetByKey(string key)
    {
        _getByKey ??= _query.Select<Application>(t => t.With(a => a.Secret));
        return _sql.Fetch<Application>(_getByKey, new { Secret = key });
    }
}
