namespace CardboardBox.PushNotifications;

using Fcm;
using static RequestResults;

public interface ISubscriptionService
{
    /// <summary>
    /// Susbcribe the given user to the topic in the context of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the subscription</param>
    /// <param name="topicHash">The topic hash / resource ID for the subscription</param>
    /// <param name="profileId">The user identifier for their profile</param>
    /// <returns>
    /// The status code of the result:
    /// <see cref="HttpStatusCode.NoContent"/> if the user doesn't have any device tokens to subscribe.
    /// <see cref="HttpStatusCode.InternalServerError"/> if the subscription failed.
    /// <see cref="HttpStatusCode.OK"/> if the subscription was successful.
    /// </returns>
    Task<RequestResult> SubscribeTopic(Guid appId, string topicHash, string profileId);

    /// <summary>
    /// Get all of the topics and groups the user is subscribed to in the contenxt of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the topic</param>
    /// <param name="profileId">The user identifier of their profile</param>
    /// <returns>The topic hashs the given user is subscribed to</returns>
    Task<UserSubscriptions> Subscriptions(Guid appId, string profileId);

    /// <summary>
    /// Add a user's device in the context of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns this interaction</param>
    /// <param name="profileId">The user identifier of their profile</param>
    /// <param name="request">The information about the users device</param>
    /// <returns>
    /// The status code of the result:
    /// <see cref="HttpStatusCode.BadRequest"/> if the request is invalid.
    /// <see cref="HttpStatusCode.NoContent"/> if the user has not subscribed to any topics
    /// <see cref="HttpStatusCode.InternalServerError"/> if the device was added but the subscriptions failed.
    /// <see cref="HttpStatusCode.OK"/> if the device was added and the subscriptions were successful.
    /// </returns>
    Task<RequestResult> AddDevice(Guid appId, string profileId, CreateUserDevice request);

    /// <summary>
    /// Send a push notification directly to all of the users devices
    /// </summary>
    /// <param name="appId">The ID of the application that owns this interaction</param>
    /// <param name="profileId">The profile to send the notification to</param>
    /// <param name="notification">The notification to send</param>
    /// <returns>
    /// The status code of the result:
    /// <see cref="HttpStatusCode.NoContent"/> if the user has no devices to send to.
    /// <see cref="HttpStatusCode.OK"/> if the notification was sent successfully.
    /// <see cref="HttpStatusCode.InternalServerError"/> if any of the notifications failed to send (some could have sent successfully).
    /// </returns>
    Task<RequestResult> UserNotification(Guid appId, string profileId, NotificationData notification);

    /// <summary>
    /// Send a push notification regarding a specific topic
    /// </summary>
    /// <param name="appId">The ID of the application that owns the topic</param>
    /// <param name="topicName">The topic hash to send against</param>
    /// <param name="notification">The notification to send</param>
    /// <returns>
    /// The status code for the notification:
    /// <see cref="HttpStatusCode.BadRequest"/> if the topic name is invalid.
    /// <see cref="HttpStatusCode.NoContent"/> if no one is subscribed to the topic
    /// <see cref="HttpStatusCode.OK"/> if the notification was sent successfully.
    /// <see cref="HttpStatusCode.InternalServerError"/> if the notification failed to send.
    /// </returns>
    Task<RequestResult> TopicNotification(Guid appId, string topicName, NotificationData notification);
}

public class SubscriptionService : ISubscriptionService
{
    private readonly IDbService _db;
    private readonly IFcmNotificationService _fcm;
    private readonly ILogger _logger;
    private readonly IJsonService _json;
    
    public SubscriptionService(
        IDbService db, 
        IFcmNotificationService fcm,
        ILogger<SubscriptionService> logger,
        IJsonService json)
    {
        _db = db;
        _fcm = fcm;
        _logger = logger;
        _json = json;
    }

    /// <summary>
    /// Susbcribe the given user to the topic in the context of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the subscription</param>
    /// <param name="topicHash">The topic hash / resource ID for the subscription</param>
    /// <param name="profileId">The user identifier for their profile</param>
    /// <returns>
    /// The status code of the result:
    /// <see cref="HttpStatusCode.BadRequest"/> if the topic hash is invalid.
    /// <see cref="HttpStatusCode.NoContent"/> if the user doesn't have any device tokens to subscribe.
    /// <see cref="HttpStatusCode.InternalServerError"/> if the subscription failed.
    /// <see cref="HttpStatusCode.OK"/> if the subscription was successful.
    /// </returns>
    public async Task<RequestResult> SubscribeTopic(Guid appId, string topicHash, string profileId)
    {
        if (!_fcm.ValidTopicName(topicHash)) return WasBadRequest("Invalid topic. Must match `^[a-zA-Z0-9-_.~%]{1,900}$`");

        //Create the topic if it doesn't exist, and get the topic id
        var topicId = await _db.Topics.Upsert(new Topic(appId, topicHash));
        //Create the subscription if it doesn't exist and get the subscription id
        var subscriptionId = await _db.Subscriptions.Upsert(new TopicSubscription(profileId, topicId));
        //Get all of the users device tokens for the current application
        var devices = await _db.DeviceTokens.Get(appId, profileId);
        var tokens = devices.Select(t => t.Token).ToArray();

        //Not tokens? No problem
        if (tokens.Length == 0) return WasNoContent("Request user has no devices");

        //Subscribe the user to the topic
        var result = await _fcm.Subscribe(topicHash, tokens);
        //Ensure there were no errors
        if (!string.IsNullOrEmpty(result.GlobalError) || result.Errors.Any())
        {
            LogSubscriptionError("Subscribing user to topic", result, profileId, topicHash);
            //Delete the subscription as it errored out
            await _db.Subscriptions.Delete(subscriptionId);
            return WasException();
        }

        return WasOk();
    }

    /// <summary>
    /// Get all of the topics and groups the user is subscribed to in the contenxt of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the topic</param>
    /// <param name="profileId">The user identifier of their profile</param>
    /// <returns>The topic hashs the given user is subscribed to</returns>
    public async Task<UserSubscriptions> Subscriptions(Guid appId, string profileId)
    {
        var topics = await _db.Topics.GetNonGroupsByUser(appId, profileId);
        var topicHashes = topics.Select(t => t.TopicHash).ToArray();

        var groups = await _db.Groups.GetByUser(appId, profileId);
        var resourceIds = groups.Select(g => g.ResourceId).ToArray();

        return new(topicHashes, resourceIds);
    }

    /// <summary>
    /// Add a user's device in the context of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns this interaction</param>
    /// <param name="profileId">The user identifier of their profile</param>
    /// <param name="request">The information about the users device</param>
    /// <returns>
    /// The status code of the result:
    /// <see cref="HttpStatusCode.BadRequest"/> if the request is invalid.
    /// <see cref="HttpStatusCode.NoContent"/> if the user has not subscribed to any topics
    /// <see cref="HttpStatusCode.InternalServerError"/> if the device was added but the subscriptions failed.
    /// <see cref="HttpStatusCode.OK"/> if the device was added and the subscriptions were successful.
    /// </returns>
    public async Task<RequestResult> AddDevice(Guid appId, string profileId, CreateUserDevice request)
    {
        var (token, name, userAgent, deviceType, providerType) = request;

        var validator = new RequestValidator()
            .NotNull(profileId, nameof(profileId))
            .NotNull(name, nameof(name))
            .NotNull(token, nameof(token));

        if (!validator.Valid) return WasBadRequest(validator);

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

        var deviceId = await _db.DeviceTokens.Upsert(device);
        var topics = await _db.Topics.GetByUser(appId, profileId);
        var topicHashes = topics.Select(t => t.TopicHash).ToArray();

        if (topicHashes.Length == 0) return WasNoContent("User is not subscribed to any topics");

        var requests = await Task.WhenAll(topicHashes.Select(async t =>
        {
            var result = await _fcm.Subscribe(t, request.Token);
            return (t, result);
        }));

        var errors = requests.Where(r => !string.IsNullOrEmpty(r.result.GlobalError) || r.result.Errors.Any()).ToArray();
        if (errors.Length == 0) return WasOk();

        errors.Each(t => LogSubscriptionError("Subscribing user to topic", t.result, profileId, t.t));
        return WasException();
    }

    /// <summary>
    /// Send a push notification directly to all of the users devices
    /// </summary>
    /// <param name="appId">The ID of the application that owns this interaction</param>
    /// <param name="profileId">The profile to send the notification to</param>
    /// <param name="notification">The notification to send</param>
    /// <returns>
    /// The status code of the notification:
    /// <see cref="HttpStatusCode.NoContent"/> if the user has no devices to send to.
    /// <see cref="HttpStatusCode.OK"/> if the notification was sent successfully.
    /// <see cref="HttpStatusCode.InternalServerError"/> if any of the notifications failed to send (some could have sent successfully).
    /// </returns>
    public async Task<RequestResult> UserNotification(Guid appId, string profileId, NotificationData notification)
    {
        try
        {
            var devices = await _db.DeviceTokens.Get(appId, profileId);
            var tokens = devices.Select(t => t.Token).ToArray();

            if (tokens.Length == 0) return WasNoContent("Request user has no devices");

            var results = await Task.WhenAll(devices.Select(async token =>
            {
                try
                {
                    var code = await _fcm.SendDirect(token.Token, notification);
                    return (true, code);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending push notification:\r\n" +
                        "Profile ID: {profileId}\r\n" +
                        "Application ID: {appId}", profileId, appId);
                    return (false, $"Error occurred for: {token.Id}: {ex.Message}");
                }
            }));

            var errors = results.Where(t => !t.Item1).Select(t => t.Item2).ToArray();

            var history = new History
            {
                ProfileId = profileId,
                Data = _json.Serialize(notification.Data ?? new()),
                Title = notification.Title,
                Body = notification.Body,
                ImageUrl = notification.ImageUrl,
                Results = results.Select(t => t.Item2).ToArray(),
            };

            await _db.History.Insert(history);
            return errors.Length > 0 
                ? WasException(errors) 
                : WasOk();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending notification to user {profileId} for {appId}", profileId, appId);
            return WasException();
        }
    }

    /// <summary>
    /// Send a push notification regarding a specific topic
    /// </summary>
    /// <param name="appId">The ID of the application that owns the topic</param>
    /// <param name="topicName">The topic hash to send against</param>
    /// <param name="notification">The notification to send</param>
    /// <returns>
    /// The status code for the notification:
    /// <see cref="HttpStatusCode.BadRequest"/> if the topic name is invalid.
    /// <see cref="HttpStatusCode.NoContent"/> if no one is subscribed to the topic
    /// <see cref="HttpStatusCode.OK"/> if the notification was sent successfully.
    /// <see cref="HttpStatusCode.InternalServerError"/> if the notification failed to send.
    /// </returns>
    public async Task<RequestResult> TopicNotification(Guid appId, string topicName, NotificationData notification)
    {
        try
        {
            if (!_fcm.ValidTopicName(topicName)) return WasBadRequest("Invalid topic. Must match `^[a-zA-Z0-9-_.~%]{1,900}$`");

            var topic = await _db.Topics.Fetch(appId, topicName);
            if (topic == null) return WasNotFound("topic");

            var count = await _db.Subscriptions.CountByTopic(appId, topicName);
            if (count <= 0) return WasNoContent("Nobody has subscribed to that topic");

            var results = await _fcm.SendTopic(topicName, notification);
            var history = new History
            {
                TopicId = topic.Id,
                Data = _json.Serialize(notification.Data ?? new()),
                Title = notification.Title,
                Body = notification.Body,
                ImageUrl = notification.ImageUrl,
                Results = new[] { results }
            };
            await _db.History.Insert(history);
            return WasOk();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending notification to topic {topicName} for {appId}", topicName, appId);
            return WasException();
        }
    }

    public void LogSubscriptionError(string content, TopicSubResponse result, string profileId, string topicHash)
    {
        _logger.LogError("Error occurred while executing request: {content}\r\n" +
            "Topic Hash: {topicHash}\r\n" +
            "Profile Id: {profileId}\r\n" +
            "Global Error: {GlobalError}\r\n" +
            "Errors: {Errors}", content, topicHash, profileId, result.GlobalError, string.Join("\r\n\t", result.Errors));
    }

}

/// <summary>
/// Represents the return result from <see cref="Subscriptions"/>
/// </summary>
/// <param name="Topics">The topic hashes the user is subscribed to</param>
/// <param name="Groups">The group resource IDs the user is subscribed to</param>
public record class UserSubscriptions(
    [property: JsonPropertyName("topics")] string[] Topics, 
    [property: JsonPropertyName("groups")] string[] Groups);

/// <summary>
/// Represents the request to create a user device
/// </summary>
/// <param name="Token">The users device token</param>
/// <param name="Name">The name of the users device</param>
/// <param name="UserAgent">The optional user-agent that created the device</param>
/// <param name="DeviceType">The optional device type, will default to <see cref="PushDeviceType.Web"/></param>
/// <param name="ProviderType">The optional provider type, will default to <see cref="ProviderType.FCM"/></param>
public record class CreateUserDevice(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("userAgent")] string? UserAgent,
    [property: JsonPropertyName("deviceType")] PushDeviceType? DeviceType,
    [property: JsonPropertyName("providerType")] ProviderType? ProviderType);
