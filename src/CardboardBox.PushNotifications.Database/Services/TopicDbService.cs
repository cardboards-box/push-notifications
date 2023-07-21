namespace CardboardBox.PushNotifications.Database.Services;

using Models;

/// <summary>
/// The service for interacting with the <see cref="Topic"/> db objects.
/// </summary>
public interface ITopicDbService : IOrmMap<Topic> 
{
    /// <summary>
    /// Gets all of the topics by the group
    /// </summary>
    /// <param name="id">The ID of the group</param>
    /// <param name="page">The page of groups</param>
    /// <param name="size">The size of the page</param>
    /// <returns>The topics</returns>
    Task<PaginatedResult<Topic>> ByGroupId(Guid id, int page, int size);

    /// <summary>
    /// Fetches the topic by app ID and topic name
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="topic">The topic name</param>
    /// <returns>The topic</returns>
    Task<Topic?> Fetch(Guid appId, string topic);

    /// <summary>
    /// Gets all of the topics that are not group subscriptions
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The topics</returns>
    Task<Topic[]> GetNonGroupsByUser(Guid appId, string profileId);

    /// <summary>
    /// Gets all of the topics for the given user
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The topics</returns>
    Task<Topic[]> GetByUser(Guid appId, string profileId);

    /// <summary>
    /// Gets all of the topics for the given group
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="groupId">The ID of the group</param>
    /// <returns>The topics</returns>
    Task<Topic[]> GetByGroup(Guid appId, Guid groupId);
}

/// <summary>
/// The implementation of the <see cref="ITopicDbService"/>
/// </summary>
public class TopicDbService : OrmMap<Topic>, ITopicDbService
{
    private static string? _fetchByAppIdAndTopic;

    /// <summary>
    /// The implementation of the <see cref="ITopicDbService"/>
    /// </summary>
    /// <param name="query">The service to generate queries</param>
    /// <param name="sql">The service that executes SQL queries</param>
    public TopicDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }

    /// <summary>
    /// Gets all of the topics by the group
    /// </summary>
    /// <param name="id">The ID of the group</param>
    /// <param name="page">The page of groups</param>
    /// <param name="size">The size of the page</param>
    /// <returns>The topics</returns>
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

    /// <summary>
    /// Fetches the topic by app ID and topic name
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="topic">The topic name</param>
    /// <returns>The topic</returns>
    public Task<Topic?> Fetch(Guid appId, string topic)
    {
        _fetchByAppIdAndTopic ??= _query.Select<Topic>(t => t
            .With(a => a.ApplicationId)
            .With(a => a.TopicHash));
        return _sql.Fetch<Topic>(_fetchByAppIdAndTopic, new { ApplicationId = appId, TopicHash = topic });
    }

    /// <summary>
    /// Gets all of the topics that are not group subscriptions
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The topics</returns>
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

    /// <summary>
    /// Gets all of the topics for the given user
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The topics</returns>
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

    /// <summary>
    /// Gets all of the topics for the given group
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="groupId">The ID of the group</param>
    /// <returns>The topics</returns>
    public Task<Topic[]> GetByGroup(Guid appId, Guid groupId)
    {
        const string QUERY = @"SELECT t.*
FROM noti_topics t 
JOIN noti_topic_group_map g on t.id = g.topic_id
WHERE
    t.application_id = :appId AND
    g.group_id = :groupId";
        return _sql.Get<Topic>(QUERY, new { appId, groupId });
    }
}