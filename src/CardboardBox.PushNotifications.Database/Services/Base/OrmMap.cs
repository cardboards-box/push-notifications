namespace CardboardBox.PushNotifications.Database.Services;

using Models;

public interface IOrmMap<T> where T : DbObject
{
    Task<T?> Fetch(Guid id);
    Task<T[]> Get();
    Task<Guid> Insert(T item);
    Task<int> Update(T item);
    Task<int> Delete(Guid id);
    Task<PaginatedResult<T>> Paginate(int page = 1, int size = 100);
    Task<Guid> Upsert(T item);
    Task<int> Count();
}

public abstract class OrmMap<T> : IOrmMap<T> where T : DbObject
{
    #region Query Cache
    private static string? _fetchQuery;
    private static string? _insertQuery;
    private static string? _updateQuery;
    private static string? _deleteQuery;
    private static string? _paginateQuery;
    private static string? _getQuery;
    private static string? _upsertQuery;
    private static string? _countQuery;
    #endregion

    public readonly IQueryService _query;
    public readonly ISqlService _sql;

    public OrmMap(
        IQueryService query,
        ISqlService sql)
    {
        _query = query;
        _sql = sql;
    }

    #region Implementations
    public virtual Task<T?> Fetch(Guid id)
    {
        _fetchQuery ??= _query.Fetch<T>();
        return _sql.Fetch<T>(_fetchQuery, new { Id = id });
    }

    public virtual Task<T[]> Get()
    {
        _getQuery ??= _query.Select<T>();
        return _sql.Get<T>(_getQuery);
    }
    
    public virtual Task<Guid> Insert(T item)
    {
        _insertQuery ??= _query.Insert<T>() + " RETURNING id";
        return _sql.ExecuteScalar<Guid>(_insertQuery, item);
    }

    public virtual Task<int> Update(T item)
    {
        _updateQuery ??= _query.Update<T>();
        return _sql.Execute(_updateQuery, item);
    }

    public virtual Task<int> Delete(Guid id)
    {
        _deleteQuery ??= _query.Delete<T>();
        return _sql.Execute(_deleteQuery, new { Id = id });
    }

    public virtual Task<PaginatedResult<T>> Paginate(int page = 1, int size = 100)
    {
        _paginateQuery ??= _query.Paginate<T, DateTime>(a => a.CreatedAt, true);
        return _sql.Paginate<T>(_paginateQuery, null, page, size);
    }

    public virtual Task<Guid> Upsert(T item)
    {
        _upsertQuery ??= _query.Upsert<T>() + " RETURNING id";
        return _sql.ExecuteScalar<Guid>(_upsertQuery, item);
    }

    public virtual Task<int> Count()
    {
        if (string.IsNullOrWhiteSpace(_countQuery))
        {
            var type = _query.Type<T>();
            _countQuery = $"SELECT COUNT(*) FROM {type.Name.Name}";
        }

        return _sql.ExecuteScalar<int>(_countQuery);
    }
    #endregion
}

