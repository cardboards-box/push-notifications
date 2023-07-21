namespace CardboardBox.PushNotifications.Database.Services;

using Models;

/// <summary>
/// The service for interacting with the <see cref="TopicGroupMap"/> db objects.
/// </summary>
public interface ITopicGroupMapDbService : IOrmMap<TopicGroupMap> 
{
    /// <summary>
    /// Fetches the mapping between a topic and a group
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="resourceId">The resource ID of the group</param>
    /// <param name="topic">The topic</param>
    /// <returns>The map between the topic and the group</returns>
    Task<TopicGroupMap?> Fetch(Guid appId, string resourceId, string topic);
}

/// <summary>
/// The implementation of the <see cref="ITopicGroupMapDbService"/>
/// </summary>
public class TopicGroupMapDbService : OrmMap<TopicGroupMap>, ITopicGroupMapDbService
{
    /// <summary>
    /// The implementation of the <see cref="ITopicGroupMapDbService"/>
    /// </summary>
    /// <param name="query">The service to generate queries</param>
    /// <param name="sql">The service that executes SQL queries</param>
    public TopicGroupMapDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }

    /// <summary>
    /// Fetches the mapping between a topic and a group
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="resourceId">The resource ID of the group</param>
    /// <param name="topic">The topic</param>
    /// <returns>The map between the topic and the group</returns>
    public Task<TopicGroupMap?> Fetch(Guid appId, string resourceId, string topic)
    {
        const string QUERY = @"SELECT m.*
FROM noti_topic_group_map m
JOIN noti_topic_groups g ON g.id = m.group_id
JOIN noti_topics t ON t.id = m.topic_id
WHERE
    g.resource_id = :resourceId AND
    t.topic_hash = :topic AND
    g.application_id = :appId AND
    t.application_id = :appId;";
        return _sql.Fetch<TopicGroupMap>(QUERY, new { appId, resourceId, topic });
    }
}