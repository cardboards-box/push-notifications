namespace CardboardBox.PushNotifications.Notifications;

using static RequestResults;

/// <summary>
/// A service for sending notifications to topics or users
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send a notification directly to a user's devices
    /// </summary>
    /// <param name="appId">The ID of the application that is sending the notification</param>
    /// <param name="profileId">The ID of the user receiving the notification</param>
    /// <param name="data">The notification data to be sent</param>
    /// <returns>The result of the request</returns>
    Task<RequestResult> Direct(Guid appId, string profileId, NotificationData data);

    /// <summary>
    /// Sends a notification to a topic
    /// </summary>
    /// <param name="appId">The ID of the application that is sending the notification</param>
    /// <param name="topicName">That topic to send the notification to</param>
    /// <param name="data">The notification data to be sent</param>
    /// <returns>The result of the request</returns>
    Task<RequestResult> Topic(Guid appId, string topicName, NotificationData data);
}

/// <summary>
/// The implementation of the <see cref="INotificationService"/>
/// </summary>
public class NotificationService : INotificationService
{
    private const string ERROR_MESSAGE = "Error occurred while sending {type} notification to {profileId} for {appId}";

    private readonly ILogger _logger;
    private readonly IFcmNotificationService _fcm;
    private readonly IDbService _db;
    private readonly IJsonService _json;

    /// <summary>
    /// The implementation of the <see cref="INotificationService"/>
    /// </summary>
    /// <param name="logger">The service for logging</param>
    /// <param name="fcm">The FCM notification service</param>
    /// <param name="db">The database service</param>
    /// <param name="json">The JSON serialization service</param>
    public NotificationService(
        ILogger<NotificationService> logger,
        IFcmNotificationService fcm,
        IDbService db,
        IJsonService json)
    {
        _fcm = fcm;
        _db = db;
        _logger = logger;
        _json = json;
    }

    /// <summary>
    /// Converts the results of a sent notification to a <see cref="History"/> record
    /// </summary>
    /// <param name="data">The notification data to be sent</param>
    /// <param name="appId">The ID of the application that sent the notification</param>
    /// <param name="responses">The responses from FCM</param>
    /// <param name="profileId">The ID of the user the notification was sent to</param>
    /// <param name="topicId">The ID of the topic the notification was sent for</param>
    /// <returns>The historical record</returns>
    public History ToHistorical(
        NotificationData data, Guid appId, NotificationResponse[] responses, 
        string? profileId = null, Guid? topicId = null)
    {
        return new History
        {
            ApplicationId = appId,
            Data = _json.Serialize(data),
            ImageUrl = data.ImageUrl,
            Title = data.Title,
            Body = data.Body,
            Results = responses.SelectMany(t => t.Results).ToArray(),
            TopicId = topicId,
            ProfileId = profileId
        };
    }

    /// <summary>
    /// Send a notification directly to a user's devices
    /// </summary>
    /// <param name="appId">The ID of the application that is sending the notification</param>
    /// <param name="profileId">The ID of the user receiving the notification</param>
    /// <param name="data">The notification data to be sent</param>
    /// <returns>The result of the request</returns>
    public async Task<RequestResult> Direct(Guid appId, string profileId, NotificationData data)
    {
        try
        {
            var devices = await _db.DeviceTokens.Get(appId, profileId);
            if (devices == null || devices.Length == 0) return WasNoContent(USER_NO_DEVICES);

            var results = await _fcm.Direct(data, devices);
            var history = ToHistorical(data, appId, results, profileId);
            var id = await _db.History.Insert(history);
            return WasOk(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ERROR_MESSAGE, "direct", profileId, appId);
            return WasException();
        }
    }

    /// <summary>
    /// Sends a notification to a topic
    /// </summary>
    /// <param name="appId">The ID of the application that is sending the notification</param>
    /// <param name="topicName">That topic to send the notification to</param>
    /// <param name="data">The notification data to be sent</param>
    /// <returns>The result of the request</returns>
    public async Task<RequestResult> Topic(Guid appId, string topicName, NotificationData data)
    {
        try
        {
            if (!TopicHelper.ValidTopicName(topicName)) return WasBadRequest(TOPIC_NAME_INVALID);

            var topic = await _db.Topics.Fetch(appId, topicName);
            if (topic == null) return WasNoContent(TOPIC_NOT_FOUND);

            var results = await _fcm.Topic(data, topic);
            var history = ToHistorical(data, appId, results, topicId: topic.Id);
            var id = await _db.History.Insert(history);
            return WasOk(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ERROR_MESSAGE, "topic", topicName, appId);
            return WasException();
        }
    }
}
