namespace CardboardBox.PushNotifications.Database.Services;

using Models;

public interface ITopicDbService : IOrmMap<Topic> 
{
    Task<PaginatedResult<Topic>> ByGroupId(Guid id, int page, int size);

    Task<Topic?> Fetch(Guid appId, string topic);

    Task<Topic[]> GetNonGroupsByUser(Guid appId, string profileId);

    Task<Topic[]> GetByUser(Guid appId, string profileId);
}

public class TopicDbService : OrmMap<Topic>, ITopicDbService
{
    private static string? _fetchByAppIdAndTopic;

    public TopicDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }

    public Task<PaginatedResult<Topic>> ByGroupId(Guid id, int page, int size)
    {
        const string QUERY = @"SELECT t.* 
FROM noti_topics t
JOIN noti_topic_group_map m ON m.topic_id = t.id
WHERE m.group_id = :id
ORDER BY t.created_at DESC
LIMIT :limit OFFSET :offset;

SELECT COUNT(t.*) 
FROM FROM noti_topics t
JOIN noti_topic_group_map m ON m.topic_id = t.id
WHERE m.group_id = :id";

        return _sql.Paginate<Topic>(QUERY, new { id }, page, size);
    }

    public Task<Topic?> Fetch(Guid appId, string topic)
    {
        _fetchByAppIdAndTopic ??= _query.Select<Topic>(t => t
            .With(a => a.ApplicationId)
            .With(a => a.TopicHash));
        return _sql.Fetch<Topic>(_fetchByAppIdAndTopic, new { ApplicationId = appId, TopicHash = topic });
    }

    public Task<Topic[]> GetNonGroupsByUser(Guid appId, string profileId)
    {
        const string QUERY = @"SELECT t.* 
FROM noti_topics t 
JOIN noti_topic_subscriptions s ON s.topic_id = t.id 
WHERE 
    s.profile_id = :profileId AND 
    t.application_id = :appId AND
    s.group_id IS NULL";
        return _sql.Get<Topic>(QUERY, new { appId, profileId });
    }

    public Task<Topic[]> GetByUser(Guid appId, string profileId)
    {
        const string QUERY = @"SELECT t.* 
FROM noti_topics t 
JOIN noti_topic_subscriptions s ON s.topic_id = t.id 
WHERE 
    s.profile_id = :profileId AND 
    t.application_id = :appId";
        return _sql.Get<Topic>(QUERY, new { appId, profileId });
    }
}