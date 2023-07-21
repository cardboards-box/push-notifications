namespace CardboardBox.PushNotifications.Database.Services;

using Models;

/// <summary>
/// The service for interacting with <see cref="TopicSubscription"/> db objects.
/// </summary>
public interface ITopicSubscriptionDbService : IOrmMap<TopicSubscription>
{
    /// <summary>
    /// Fetches the subscription by user and topic
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="topic">The topic</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The subscription</returns>
    Task<TopicSubscription?> FetchByUser(Guid appId, string topic, string profileId);

    /// <summary>
    /// The number of subscriptions to a topic
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="topic">The topic</param>
    /// <returns>The number of topics</returns>
    Task<int> CountByTopic(Guid appId, string topic);

    /// <summary>
    /// Deletes all of the subscriptions for the given user
    /// </summary>
    /// <param name="groupId">The ID of the group</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The number of records deleted</returns>
    Task<int> DeleteByGroup(Guid groupId, string profileId);

    /// <summary>
    /// Gets all of the subscriptions and topics for the given profile
    /// </summary>
    /// <param name="groupId">The ID of the group</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The subscriptions and topics</returns>
    Task<(TopicSubscription sub, Topic topic)[]> GetByGroup(Guid groupId, string profileId);
}

/// <summary>
/// The implementation of the <see cref="ITopicSubscriptionDbService"/>
/// </summary>
public class TopicSubscriptionDbService : OrmMap<TopicSubscription>, ITopicSubscriptionDbService
{
    private static string? _deleteByGroup;

    /// <summary>
    /// The implementation of the <see cref="ITopicSubscriptionDbService"/>
    /// </summary>
    /// <param name="query">The service to generate queries</param>
    /// <param name="sql">The service that executes SQL queries</param>
    public TopicSubscriptionDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }

    /// <summary>
    /// Deletes all of the subscriptions for the given user
    /// </summary>
    /// <param name="groupId">The ID of the group</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The number of records deleted</returns>
    public Task<int> DeleteByGroup(Guid groupId, string profileId)
    {
        _deleteByGroup ??= _query.Delete<TopicSubscription>(t => 
            t.With(a => a.GroupId)
             .With(a => a.ProfileId)
             .NotNull(t => t.GroupId));
        return _sql.Execute(_deleteByGroup, new { GroupId = groupId, ProfileId = profileId });
    }

    /// <summary>
    /// Gets all of the subscriptions and topics for the given profile
    /// </summary>
    /// <param name="groupId">The ID of the group</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The subscriptions and topics</returns>
    public Task<(TopicSubscription sub, Topic topic)[]> GetByGroup(Guid groupId, string profileId)
    {
        const string QUERY = @"SELECT s.*, '' as split, t.*
FROM noti_topic_subscriptions s 
JOIN noti_topics t ON t.id = s.topic_id
WHERE
    s.group_id = :groupId AND
    s.profile_id = :profileId;";
        return _sql.QueryTupleAsync<TopicSubscription, Topic>(QUERY, new { groupId, profileId });
    }

    /// <summary>
    /// Fetches the subscription by user and topic
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="topic">The topic</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The subscription</returns>
    public Task<TopicSubscription?> FetchByUser(Guid appId, string topic, string profileId)
    {
        const string QUERY = @"SELECT s.*
FROM noti_topic_subscriptions s 
JOIN noti_topics t ON t.id = s.topic_id
WHERE 
    t.application_id = :appId AND
    s.profile_id = :profileId AND
    t.topic_hash = :topic";
        return _sql.Fetch<TopicSubscription>(QUERY, new { appId, topic, profileId });
    }

    /// <summary>
    /// The number of subscriptions to a topic
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="topic">The topic</param>
    /// <returns>The number of topics</returns>
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