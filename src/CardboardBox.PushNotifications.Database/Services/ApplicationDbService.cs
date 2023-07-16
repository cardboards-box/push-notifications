namespace CardboardBox.PushNotifications.Database.Services;

using Models;

public interface IApplicationDbService : IOrmMap<Application> 
{
    Task<Application?> GetByKey(string key);
}

public class ApplicationDbService : OrmMap<Application>, IApplicationDbService
{
    private static string? _getByKey;

    public ApplicationDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }

    public Task<Application?> GetByKey(string key)
    {
        _getByKey ??= _query.Select<Application>(t => t.With(a => a.Secret));
        return _sql.Fetch<Application>(_getByKey, new { Secret = key });
    }
}
