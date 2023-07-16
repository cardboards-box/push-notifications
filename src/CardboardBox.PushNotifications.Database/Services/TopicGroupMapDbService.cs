namespace CardboardBox.PushNotifications.Database.Services;

using Models;

public interface ITopicGroupMapDbService : IOrmMap<TopicGroupMap> { }

public class TopicGroupMapDbService : OrmMap<TopicGroupMap>, ITopicGroupMapDbService
{
    public TopicGroupMapDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }
}