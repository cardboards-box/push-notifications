namespace CardboardBox.PushNotifications.Database.Services;

using Models;

public interface ITopicGroupSubscriptionDbService : IOrmMap<TopicGroupSubscription> 
{
}

public class TopicGroupSubscriptionDbService : OrmMap<TopicGroupSubscription>, ITopicGroupSubscriptionDbService
{
    public TopicGroupSubscriptionDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }

}