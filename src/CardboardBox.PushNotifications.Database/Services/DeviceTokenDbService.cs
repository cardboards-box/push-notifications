namespace CardboardBox.PushNotifications.Database.Services;

using Models;

/// <summary>
/// The service for interacting with <see cref="DeviceToken"/> db objects
/// </summary>
public interface IDeviceTokenDbService : IOrmMap<DeviceToken> 
{
    /// <summary>
    /// Gets all of the device tokens for a given application and profile
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The device tokens</returns>
    Task<DeviceToken[]> Get(Guid appId, string profileId);

    /// <summary>
    /// Gets all of the devices that are subscribed to the given group
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="groupId">The ID of the group</param>
    /// <returns>The device tokens</returns>
    Task<DeviceToken[]> ByGroup(Guid appId, Guid groupId);

    /// <summary>
    /// Gets all of the devices subscribed to a given map
    /// </summary>
    /// <param name="mapId">The ID of the group-topic mapping</param>
    /// <returns>The device tokens</returns>
    Task<DeviceToken[]> ByMap(Guid mapId);
}

/// <summary>
/// The implementation of the <see cref="IDeviceTokenDbService"/>
/// </summary>
public class DeviceTokenDbService : OrmMap<DeviceToken>, IDeviceTokenDbService
{
    private static string? _byAppProfile;

    /// <summary>
    /// The implementation of the <see cref="IDeviceTokenDbService"/>
    /// </summary>
    /// <param name="query">The service to generate queries</param>
    /// <param name="sql">The service that executes SQL queries</param>
    public DeviceTokenDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }

    /// <summary>
    /// Gets all of the device tokens for a given application and profile
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The device tokens</returns>
    public Task<DeviceToken[]> Get(Guid appId, string profileId)
    {
        _byAppProfile ??= _query.Select<DeviceToken>(t =>
            t.With(a => a.ApplicationId)
             .With(a => a.ProfileId));

        return _sql.Get<DeviceToken>(_byAppProfile, new { ApplicationId = appId, ProfileId = profileId });
    }

    /// <summary>
    /// Gets all of the devices that are subscribed to the given group
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="groupId">The ID of the group</param>
    /// <returns>The device tokens</returns>
    public Task<DeviceToken[]> ByGroup(Guid appId, Guid groupId)
    {
        const string QUERY = @"SELECT DISTINCT d.*
FROM noti_device_tokens d
JOIN noti_topic_group_subscription s ON s.profile_id = d.profile_id
JOIN noti_topic_groups g ON g.id = s.group_id
WHERE
    g.application_id = :appId AND
    g.id = :groupId;";
        return _sql.Get<DeviceToken>(QUERY, new { appId, groupId });
    }

    /// <summary>
    /// Gets all of the devices subscribed to a given map
    /// </summary>
    /// <param name="mapId">The ID of the group-topic mapping</param>
    /// <returns>The device tokens</returns>
    public Task<DeviceToken[]> ByMap(Guid mapId)
    {
        const string QUERY = @"SELECT DISTINCT d.*
FROM noti_device_tokens d 
JOIN noti_topic_subscriptions s ON s.profile_id = d.profile_id
JOIN noti_topic_group_map m ON s.topic_id = m.topic_id AND s.group_id = m.group_id
WHERE
    m.id = :mapId;";
        return _sql.Get<DeviceToken>(QUERY, new { mapId });
    }
}
