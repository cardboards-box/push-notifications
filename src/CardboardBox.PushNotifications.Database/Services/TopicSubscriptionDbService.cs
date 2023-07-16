namespace CardboardBox.PushNotifications.Database.Services;

using Models;

public interface ITopicSubscriptionDbService : IOrmMap<TopicSubscription>
{
    Task<int> CountByTopic(Guid appId, string topic);
}

public class TopicSubscriptionDbService : OrmMap<TopicSubscription>, ITopicSubscriptionDbService
{
    public TopicSubscriptionDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }

    public Task<int> CountByTopic(Guid appId, string topic)
    {
        const string QUERY = @"SELECT COUNT (s.id)
FROM noti_topic_subscriptions s
JOIN noti_topics t on t.id = s.topic_id
WHERE
    t.application_id = :appId AND
    t.topic_hash = :topic";
        return _sql.ExecuteScalar<int>(QUERY, new { appId, topic });
    }
}