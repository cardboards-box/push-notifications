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
}
