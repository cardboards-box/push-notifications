namespace CardboardBox.PushNotifications.Subscriptions;

using static RequestResults;

/// <summary>
/// A service for unsubscribing from topics and groups
/// </summary>
public interface IUnsubscriptionService
{
    /// <summary>
    /// Unsubscribe the given user from the topic in the contenxt of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the subscription</param>
    /// <param name="topicName">The topic hash / resource ID for the subscription</param>
    /// <param name="profileId">The ID of the user to unsubscribe from the topic</param>
    /// <returns>The result of the unsubscription request</returns>
    Task<RequestResult> Topic(Guid appId, string topicName, string profileId);

    /// <summary>
    /// Unsubscribe the given user from the group in the context of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the subscription</param>
    /// <param name="resourceId">The resource ID for the group</param>
    /// <param name="profileId">The ID of the user to unsubscribe from the group</param>
    /// <returns>The result of the unsubscription request</returns>
    Task<RequestResult> Group(Guid appId, string resourceId, string profileId);
}

/// <summary>
/// The implementation for the <see cref="IUnsubscriptionService"/>
/// </summary>
public class UnsubscriptionService : IUnsubscriptionService
{
    #region Logger Messages
    private const string EXCEPTION_TOPIC_MESSAGE = "Error occurred unsubscribing user {profileId} to topic {topicName} for {appId}";
    private const string EXCEPTION_GROUP_MESSAGE = "Error occurred unsubscribing user {profileId} to group {resourceId} for {appId}";
    private const string ERROR_TOPIC_MESSAGE = "Error occurred while Unsubscribing user from topic: Topic: {topicHash}, Profile Id: {profileId}, Global Error: {GlobalError}, Errors: {Errors}";
    private const string ERROR_GROUP_MESSAGE = "Error occurred while Unsubscribing user from group: Group: {resourceId}, Profile Id: {profileId}, Global Error: {GlobalError}, Errors: {Errors}";
    #endregion

    private readonly ILogger _logger;
    private readonly IFcmTopicService _fcm;
    private readonly IDbService _db;

    /// <summary>
    /// The implementation for the <see cref="IUnsubscriptionService"/>
    /// </summary>
    /// <param name="logger">The service for logging</param>
    /// <param name="fcm">The FCM subscription service</param>
    /// <param name="db">The database service</param>
    public UnsubscriptionService(
        ILogger<UnsubscriptionService> logger,
        IFcmTopicService fcm,
        IDbService db)
    {
        _logger = logger;
        _fcm = fcm;
        _db = db;
    }

    /// <summary>
    /// Unsubscribe the given user from the topic in the contenxt of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the subscription</param>
    /// <param name="topicName">The topic hash / resource ID for the subscription</param>
    /// <param name="profileId">The ID of the user to unsubscribe from the topic</param>
    /// <returns>The result of the unsubscription request</returns>
    public async Task<RequestResult> Topic(Guid appId, string topicName, string profileId)
    {
        try
        {
            if (!TopicHelper.ValidTopicName(topicName)) return WasBadRequest(TOPIC_NAME_INVALID);

            //Get the user's subscription for the topic
            var sub = await _db.Subscriptions.FetchByUser(appId, topicName, profileId);
            if (sub == null) return WasNoContent(USER_NOT_SUBSCRIBED);
            //Delete the subscription
            await _db.Subscriptions.Delete(sub.Id);
            var devices = await _db.DeviceTokens.Get(appId, profileId);
            if (devices.Length == 0) return WasNoContent(USER_NO_DEVICES);
            //Unsubscribe the devices from the topic
            var result = await _fcm.UnsubscribeDevicesPerTopic(topicName, devices.Select(d => d.Token).ToArray());
            //No errors while unsubscribing? Great!
            if (string.IsNullOrEmpty(result.GlobalError) && !result.Errors.Any()) return WasOk();
            //Log the error and return it
            _logger.LogError(ERROR_TOPIC_MESSAGE, topicName, profileId, result.GlobalError, string.Join(", ", result.Errors));
            return WasException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, EXCEPTION_TOPIC_MESSAGE, profileId, topicName, appId);
            return WasException();
        }
    }

    /// <summary>
    /// Unsubscribe the given user from the group in the context of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the subscription</param>
    /// <param name="resourceId">The resource ID for the group</param>
    /// <param name="profileId">The ID of the user to unsubscribe from the group</param>
    /// <returns>The result of the unsubscription request</returns>
    public async Task<RequestResult> Group(Guid appId, string resourceId, string profileId)
    {
        try
        {
            //Get the user's subscription for the group
            var sub = await _db.GroupSubscriptions.FetchByUser(appId, resourceId, profileId);
            if (sub == null) return WasNoContent(GROUP_NOT_SUBSCRIBED);
            //Delete the subscription
            await _db.GroupSubscriptions.Delete(sub.Id);
            //Get the user's topic subscriptions for the group
            var subs = await _db.Subscriptions.GetByGroup(sub.GroupId, profileId);
            //No topic subscriptions? No problem!
            if (subs.Length == 0) return WasNoContent(GROUP_NOT_SUBSCRIBED);
            //Delete all of the topic subscriptions.
            await _db.Subscriptions.DeleteByGroup(sub.GroupId, profileId);
            //Get the user's devices
            var devices = await _db.DeviceTokens.Get(appId, profileId);
            //No devices? No problem!
            if (devices.Length == 0) return WasNoContent(USER_NO_DEVICES);
            //Unsubscribe the devices from the topics
            var tokens = devices.Select(d => d.Token).Distinct().ToArray();
            var topics = subs.Select(t => t.topic.TopicHash).Distinct().ToArray();
            var result = await _fcm.UnsubscribeTopicsAndDevices(tokens, topics);
            //No errors while subscribing? Great!
            if (result.Failures == 0) return WasOk();
            //Log the errors and return them
            result.Responses
               .Where(t => !string.IsNullOrEmpty(t.GlobalError) || t.Errors.Any())
               .Each(t => _logger.LogError(ERROR_GROUP_MESSAGE, resourceId, profileId, t.GlobalError, string.Join(", ", t.Errors)));
            return WasException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, EXCEPTION_GROUP_MESSAGE, profileId, resourceId, appId);
            return WasException();
        }
    }
}
