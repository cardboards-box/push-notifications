namespace CardboardBox.PushNotifications.Database.Services;

using Models;

/// <summary>
/// The service for interacting with the <see cref="History"/> db objects
/// </summary>
public interface IHistoryDbService : IOrmMap<History> { }

/// <summary>
/// The implementation of the <see cref="IHistoryDbService"/>
/// </summary>
public class HistoryDbService : OrmMap<History>, IHistoryDbService
{
    /// <summary>
    /// The implementation of the <see cref="IHistoryDbService"/>
    /// </summary>
    /// <param name="query">The service to generate queries</param>
    /// <param name="sql">The service that executes SQL queries</param>
    public HistoryDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }
}