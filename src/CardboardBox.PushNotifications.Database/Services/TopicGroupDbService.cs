namespace CardboardBox.PushNotifications.Database.Services;

using Models;

/// <summary>
/// The service for interacting with the <see cref="TopicGroup"/> db objects.
/// </summary>
public interface ITopicGroupDbService : IOrmMap<TopicGroup> 
{
    /// <summary>
    /// Fetchs the topic group by its resource id.
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="resourceId">The ID of the resource</param>
    /// <returns>The topic group</returns>
    Task<TopicGroup?> Fetch(Guid appId, string resourceId);

    /// <summary>
    /// Fetchs all of the topic groups by the application ID
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="page">The page of topics</param>
    /// <param name="size">The size of the page</param>
    /// <returns>The topic groups</returns>
    Task<PaginatedResult<TopicGroup>> ByAppId(Guid appId, int page, int size);

    /// <summary>
    /// Gets all of the topic groups for the given user
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The topic groups</returns>
    Task<TopicGroup[]> ByUser(Guid appId, string profileId);
}

/// <summary>
/// The implementation of the <see cref="ITopicGroupDbService"/>
/// </summary>
public class TopicGroupDbService : OrmMap<TopicGroup>, ITopicGroupDbService
{
    private static string? _byAppId;
    private static string? _byResourceId;

    /// <summary>
    /// The implementation of the <see cref="ITopicGroupDbService"/>
    /// </summary>
    /// <param name="query">The service to generate queries</param>
    /// <param name="sql">The service that executes SQL queries</param>
    public TopicGroupDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }

    /// <summary>
    /// Fetchs the topic group by its resource id.
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="resourceId">The ID of the resource</param>
    /// <returns>The topic group</returns>
    public Task<TopicGroup?> Fetch(Guid appId, string resourceId)
    {
        _byResourceId ??= _query.Select<TopicGroup>(t => t.With(a => a.ResourceId).With(a => a.ApplicationId));
        return _sql.Fetch<TopicGroup>(_byResourceId, new { ResourceId = resourceId, ApplicationId = appId });
    }

    /// <summary>
    /// Fetchs all of the topic groups by the application ID
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="page">The page of topics</param>
    /// <param name="size">The size of the page</param>
    /// <returns>The topic groups</returns>
    public Task<PaginatedResult<TopicGroup>> ByAppId(Guid appId, int page, int size)
    {
        _byAppId ??= _query.Paginate<TopicGroup, DateTime>(t => t.CreatedAt, true, t => t.With(a => a.ApplicationId));
        return _sql.Paginate<TopicGroup>(_byAppId, new { ApplicationId = appId }, page, size);
    }

    /// <summary>
    /// Gets all of the topic groups for the given user
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The topic groups</returns>
    public Task<TopicGroup[]> ByUser(Guid appId, string profileId)
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