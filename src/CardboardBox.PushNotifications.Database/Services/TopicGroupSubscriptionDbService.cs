namespace CardboardBox.PushNotifications.Database.Services;

using Models;

/// <summary>
/// The service for interacting with <see cref="TopicGroupSubscription"/> db objects.
/// </summary>
public interface ITopicGroupSubscriptionDbService : IOrmMap<TopicGroupSubscription> 
{
    /// <summary>
    /// Fetches the subscription for a user to a topic group.
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <param name="resourceId">The ID of the resource</param>
    /// <returns>The subscription</returns>
    Task<TopicGroupSubscription?> FetchByUser(Guid appId, string profileId, string resourceId);
}

/// <summary>
/// The implementation of the <see cref="ITopicGroupSubscriptionDbService"/>
/// </summary>
public class TopicGroupSubscriptionDbService : OrmMap<TopicGroupSubscription>, ITopicGroupSubscriptionDbService
{
    /// <summary>
    /// The implementation of the <see cref="ITopicGroupSubscriptionDbService"/>
    /// </summary>
    /// <param name="query">The service to generate queries</param>
    /// <param name="sql">The service that executes SQL queries</param>
    public TopicGroupSubscriptionDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }

    /// <summary>
    /// Fetches the subscription for a user to a topic group.
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <param name="resourceId">The ID of the resource</param>
    /// <returns>The subscription</returns>
    public Task<TopicGroupSubscription?> FetchByUser(Guid appId, string profileId, string resourceId)
    {
        const string QUERY = @"SELECT gs.*
FROM noti_topic_group_subscription gs
JOIN noti_topic_groups g ON gs.group_id = g.id
WHERE
    g.application_id = :appId AND
    gs.profile_id = :profileId AND
    g.resource_id = :resourceId;";

        return _sql.Fetch<TopicGroupSubscription>(QUERY, new { appId, profileId, resourceId });
    }
}