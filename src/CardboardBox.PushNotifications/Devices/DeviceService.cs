namespace CardboardBox.PushNotifications.Devices;

using static RequestResults;

/// <summary>
/// A service for interacting with a user's devices
/// </summary>
public interface IDeviceService
{
    /// <summary>
    /// Add a user's device in the context of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns this interaction</param>
    /// <param name="profileId">The ID of the user's profile</param>
    /// <param name="request">The user's device information</param>
    /// <returns>The results of the request</returns>
    Task<RequestResult> Add(Guid appId, string profileId, CreateUserDevice request);

    /// <summary>
    /// Removes a user's device in the context of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns this interaction</param>
    /// <param name="deviceId">The ID of the user's device</param>
    /// <returns>The results of the request</returns>
    Task<RequestResult> Remove(Guid appId, Guid deviceId);

    /// <summary>
    /// Gets the users devices within the contenxt of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the devices</param>
    /// <param name="profileId">The ID of the user's profile that owns the devices</param>
    /// <returns>The results of the request</returns>
    Task<RequestResult<DeviceToken[]>> Devices(Guid appId, string profileId);
}

/// <summary>
/// The implementation of the <see cref="IDeviceService"/>
/// </summary>
public class DeviceService : IDeviceService
{
    private const string EXCEPTION_ADD_MESSAGE = "Error occurred while creating a device for user {profileId} for {appId}";
    private const string EXCEPTION_REM_MESSAGE = "Error occurred while removing a device {deviceId} for {appId}";
    private const string ERROR_ADD_MESSAGE = "Error occurred while Subscribing user to topic while creating device: Topic: {topicHash}, Profile Id: {profileId}, Global Error: {GlobalError}, Errors: {Errors}";
    private const string ERROR_REM_MESSAGE = "Error occurred while Unsubscribing user from topic while removing device: Topic: {topicHash}, Device Id: {deviceId}, Global Error: {GlobalError}, Errors: {Errors}";

    private readonly ILogger _logger;
    private readonly IFcmTopicService _fcm;
    private readonly IDbService _db;

    /// <summary>
    /// The implementation of the <see cref="IDeviceService"/>
    /// </summary>
    /// <param name="logger">The service that handles logging</param>
    /// <param name="fcm">The service for interacting with FCM subscriptions</param>
    /// <param name="db">The service for interacting with the database</param>
    public DeviceService(
        ILogger<DeviceService> logger,
        IFcmTopicService fcm,
        IDbService db)
    {
        _fcm = fcm;
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Add a user's device in the context of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns this interaction</param>
    /// <param name="profileId">The ID of the user's profile</param>
    /// <param name="request">The user's device information</param>
    /// <returns>The results of the request</returns>
    public async Task<RequestResult> Add(Guid appId, string profileId, CreateUserDevice request)
    {
        try
        {
            //deconstruct the request
            var (token, name, userAgent, deviceType, providerType) = request;
            //validate the request
            var validator = new RequestValidator()
                .NotNull(profileId, nameof(profileId))
                .NotNull(name, nameof(name))
                .NotNull(token, nameof(token));
            //Request invalid, return bad request
            if (!validator.Valid) return WasBadRequest(validator);
            //Create the device
            var device = new DeviceToken
            {
                ApplicationId = appId,
                ProfileId = profileId,
                Token = token,
                Name = name,
                UserAgent = userAgent,
                DeviceType = deviceType ?? PushDeviceType.Web,
                ProviderType = providerType ?? ProviderType.FCM
            };

            device.Id = await _db.DeviceTokens.Upsert(device);
            //Get the topics the user is subscribed to
            var topics = await _db.Topics.GetByUser(appId, profileId);
            //No topics? no problem!
            if (topics.Length == 0) return WasNoContent(USER_NO_TOPICS);
            //Subscribe the device to the topics
            var result = await _fcm.SubscribeTopicsPerDevice(token, topics.Select(t => t.TopicHash).ToArray());
            if (result.Failures == 0) return WasOk();
            //Log the errors and return the exception
            result.Responses
                .Where(t => !string.IsNullOrEmpty(t.GlobalError) || t.Errors.Any())
                .Each(t => _logger.LogError(ERROR_ADD_MESSAGE, t.Topic, profileId, t.GlobalError, string.Join(", ", t.Errors)));
            return WasException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, EXCEPTION_ADD_MESSAGE, profileId, appId);
            return WasException();
        }
    }

    /// <summary>
    /// Removes a user's device in the context of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns this interaction</param>
    /// <param name="deviceId">The ID of the user's device</param>
    /// <returns>The results of the request</returns>
    public async Task<RequestResult> Remove(Guid appId, Guid deviceId)
    {
        try
        {
            //Get the device from the database
            var device = await _db.DeviceTokens.Fetch(deviceId);
            //Device not found? return not found
            if (device == null) return WasNotFound("Device");
            //Device doesn't belong to this application? return unauthorized
            if (device.ApplicationId != appId) return WasUnauthorized();
            //Delete the device from the database
            await _db.DeviceTokens.Delete(deviceId);
            //Get the topics the user is subscribed to
            var topics = await _db.Topics.GetByUser(appId, device.ProfileId);
            //No topics? no problem!
            if (topics.Length == 0) return WasNoContent(USER_NO_TOPICS);
            //Unsubscribe all of the topics from this device
            var result = await _fcm.UnsubscribeTopicsPerDevice(device.Token, topics.Select(t => t.TopicHash).ToArray());
            if (result.Failures == 0) return WasOk();
            //Log the errors and return the exception
            result.Responses
                    .Where(t => !string.IsNullOrEmpty(t.GlobalError) || t.Errors.Any())
                    .Each(t => _logger.LogError(ERROR_REM_MESSAGE, t.Topic, deviceId, t.GlobalError, string.Join(", ", t.Errors)));
            return WasException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, EXCEPTION_REM_MESSAGE, deviceId, appId);
            return WasException();
        }
    }

    /// <summary>
    /// Gets the users devices within the contenxt of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the devices</param>
    /// <param name="profileId">The ID of the user's profile that owns the devices</param>
    /// <returns>The results of the request</returns>
    public async Task<RequestResult<DeviceToken[]>> Devices(Guid appId, string profileId)
    {
        var devices = await _db.DeviceTokens.Get(appId, profileId);
        return WasOk(devices);
    }
}
