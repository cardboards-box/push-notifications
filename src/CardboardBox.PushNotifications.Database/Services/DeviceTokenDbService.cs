namespace CardboardBox.PushNotifications.Database.Services;

using Models;

public interface IDeviceTokenDbService : IOrmMap<DeviceToken> 
{
    Task<DeviceToken[]> Get(Guid appId, string profileId);
}

public class DeviceTokenDbService : OrmMap<DeviceToken>, IDeviceTokenDbService
{
    private static string? _byAppProfile;

    public DeviceTokenDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }

    public Task<DeviceToken[]> Get(Guid appId, string profileId)
    {
        _byAppProfile ??= _query.Select<DeviceToken>(t =>
            t.With(a => a.ApplicationId)
             .With(a => a.ProfileId));

        return _sql.Get<DeviceToken>(_byAppProfile, new { ApplicationId = appId, ProfileId = profileId });
    }
}
