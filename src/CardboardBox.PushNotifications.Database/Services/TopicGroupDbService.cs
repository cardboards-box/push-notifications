namespace CardboardBox.PushNotifications.Database.Services;

using Models;

public interface ITopicGroupDbService : IOrmMap<TopicGroup> 
{
    Task<PaginatedResult<TopicGroup>> ByAppId(Guid appId, int page, int size);

    Task<TopicGroup[]> GetByUser(Guid appId, string profileId);
}

public class TopicGroupDbService : OrmMap<TopicGroup>, ITopicGroupDbService
{
    private static string? _byAppId;

    public TopicGroupDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }

    public Task<PaginatedResult<TopicGroup>> ByAppId(Guid appId, int page, int size)
    {
        _byAppId ??= _query.Paginate<TopicGroup, DateTime>(t => t.CreatedAt, true, t => t.With(a => a.ApplicationId));
        return _sql.Paginate<TopicGroup>(_byAppId, new { ApplicationId = appId }, page, size);
    }

    public Task<TopicGroup[]> GetByUser(Guid appId, string profileId)
    {
        const string QUERY = @"SELECT DISTINCT g.*
FROM noti_topic_groups g 
JOIN noti_topic_subscriptions s ON s.group_id = g.id
JOIN noti_topics t ON s.topic_id = t.id
WHERE
    s.profile_id = :profileId AND 
    t.application_id = :appId";
        return _sql.Get<TopicGroup>(QUERY, new { profileId, appId });
    }
}