namespace CardboardBox.PushNotifications.Database.Services;

using Models;

public interface IHistoryDbService : IOrmMap<History> { }

public class HistoryDbService : OrmMap<History>, IHistoryDbService
{
    public HistoryDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }
}