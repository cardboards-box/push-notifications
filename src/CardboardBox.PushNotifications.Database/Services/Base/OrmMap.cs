namespace CardboardBox.PushNotifications.Database.Services;

using Models;

/// <summary>
/// Represents a service that can query database models
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IOrmMap<T> where T : DbObject
{
    /// <summary>
    /// Fetches a single item from the database
    /// </summary>
    /// <param name="id">The ID of the item from the database</param>
    /// <returns>The item or null if not found</returns>
    Task<T?> Fetch(Guid id);

    /// <summary>
    /// Gets all of the items from the database
    /// </summary>
    /// <returns>The items in the database</returns>
    Task<T[]> Get();

    /// <summary>
    /// Inserts a new item into the database
    /// </summary>
    /// <param name="item">The item to insert into the database</param>
    /// <returns>The unique ID for the record that was inserted</returns>
    Task<Guid> Insert(T item);

    /// <summary>
    /// Updates the given item in the database by it's unique ID
    /// </summary>
    /// <param name="item">The item to update</param>
    /// <returns>The number of records updated</returns>
    Task<int> Update(T item);

    /// <summary>
    /// Deletes the given item in the database by it's unique ID
    /// </summary>
    /// <param name="id">The ID of the record to delete</param>
    /// <returns>The number of records deleted</returns>
    Task<int> Delete(Guid id);

    /// <summary>
    /// Gets a paginated list of items from the database, ordered by it's created date
    /// </summary>
    /// <param name="page">The page of records to get</param>
    /// <param name="size">The number of records per page</param>
    /// <returns>The paginated results</returns>
    Task<PaginatedResult<T>> Paginate(int page = 1, int size = 100);

    /// <summary>
    /// Inserts or updates the given item in the database by it's unique IDs
    /// </summary>
    /// <param name="item">The item to insert or update</param>
    /// <returns>The unique ID of the record that was inserted or updated</returns>
    Task<Guid> Upsert(T item);

    /// <summary>
    /// Gets the number of records in the current table
    /// </summary>
    /// <returns>The numerb of records in the table</returns>
    Task<int> Count();
}

/// <summary>
/// Implementation of the <see cref="IOrmMap{T}"/>
/// </summary>
/// <typeparam name="T">The type of object represented in the database</typeparam>
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

    /// <summary>
    /// The service to generate queries
    /// </summary>
    public readonly IQueryService _query;
    /// <summary>
    /// The service that executes SQL queries
    /// </summary>
    public readonly ISqlService _sql;

    /// <summary>
    /// Implementation of the <see cref="IOrmMap{T}"/>
    /// </summary>
    /// <param name="query">The service to generate queries</param>
    /// <param name="sql">The service that executes SQL queries</param>
    public OrmMap(
        IQueryService query,
        ISqlService sql)
    {
        _query = query;
        _sql = sql;
    }

    #region Implementations
    /// <summary>
    /// Fetches a single item from the database
    /// </summary>
    /// <param name="id">The ID of the item from the database</param>
    /// <returns>The item or null if not found</returns>
    public virtual Task<T?> Fetch(Guid id)
    {
        _fetchQuery ??= _query.Fetch<T>();
        return _sql.Fetch<T>(_fetchQuery, new { Id = id });
    }

    /// <summary>
    /// Gets all of the items from the database
    /// </summary>
    /// <returns>The items in the database</returns>
    public virtual Task<T[]> Get()
    {
        _getQuery ??= _query.Select<T>();
        return _sql.Get<T>(_getQuery);
    }

    /// <summary>
    /// Inserts a new item into the database
    /// </summary>
    /// <param name="item">The item to insert into the database</param>
    /// <returns>The unique ID for the record that was inserted</returns>
    public virtual Task<Guid> Insert(T item)
    {
        _insertQuery ??= _query.Insert<T>() + " RETURNING id";
        return _sql.ExecuteScalar<Guid>(_insertQuery, item);
    }

    /// <summary>
    /// Updates the given item in the database by it's unique ID
    /// </summary>
    /// <param name="item">The item to update</param>
    /// <returns>The number of records updated</returns>
    public virtual Task<int> Update(T item)
    {
        _updateQuery ??= _query.Update<T>();
        return _sql.Execute(_updateQuery, item);
    }

    /// <summary>
    /// Deletes the given item in the database by it's unique ID
    /// </summary>
    /// <param name="id">The ID of the record to delete</param>
    /// <returns>The number of records deleted</returns>
    public virtual Task<int> Delete(Guid id)
    {
        _deleteQuery ??= _query.Delete<T>();
        return _sql.Execute(_deleteQuery, new { Id = id });
    }

    /// <summary>
    /// Gets a paginated list of items from the database, ordered by it's created date
    /// </summary>
    /// <param name="page">The page of records to get</param>
    /// <param name="size">The number of records per page</param>
    /// <returns>The paginated results</returns>
    public virtual Task<PaginatedResult<T>> Paginate(int page = 1, int size = 100)
    {
        _paginateQuery ??= _query.Paginate<T, DateTime>(a => a.CreatedAt, true);
        return _sql.Paginate<T>(_paginateQuery, null, page, size);
    }

    /// <summary>
    /// Inserts or updates the given item in the database by it's unique IDs
    /// </summary>
    /// <param name="item">The item to insert or update</param>
    /// <returns>The unique ID of the record that was inserted or updated</returns>
    public virtual Task<Guid> Upsert(T item)
    {
        _upsertQuery ??= _query.Upsert<T>() + " RETURNING id";
        return _sql.ExecuteScalar<Guid>(_upsertQuery, item);
    }

    /// <summary>
    /// Gets the number of records in the current table
    /// </summary>
    /// <returns>The numerb of records in the table</returns>
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

