namespace CardboardBox.PushNotifications.Subscriptions;

using static RequestResults;

/// <summary>
/// A service for subscribing to topics and groups
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Subscribe the given user to the topic in the contenxt of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the subscription</param>
    /// <param name="topicName">The topic hash / resource ID for the subscription</param>
    /// <param name="profileId">The ID of the user to subscribe to the topic</param>
    /// <returns>The result of the subscription request</returns>
    Task<RequestResult> Topic(Guid appId, string topicName, string profileId);

    /// <summary>
    /// Subscribe the given user to the group in the context of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the subscription</param>
    /// <param name="resourceId">The resource ID for the group</param>
    /// <param name="profileId">The ID of the user to subscribe to the group</param>
    /// <returns>The result of the subscription request</returns>
    Task<RequestResult> Group(Guid appId, string resourceId, string profileId);

    /// <summary>
    /// Gets all of the subscriptions for the given user in the context of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the subscriptions</param>
    /// <param name="profileId">The ID of the user who owns the subscriptions</param>
    /// <returns>The users subscriptions</returns>
    Task<RequestResult<UserSubscriptions>> Get(Guid appId, string profileId);
}

/// <summary>
/// The implementation for the <see cref="ISubscriptionService"/>
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    #region Logger Messages
    private const string EXCEPTION_TOPIC_MESSAGE = "Error occurred subscribing user {profileId} to topic {topicName} for {appId}";
    private const string EXCEPTION_GROUP_MESSAGE = "Error occurred subscribing user {profileId} to group {groupName} for {appId}";
    private const string ERROR_TOPIC_MESSAGE = "Error occurred while Subscribing user to topic: Topic: {topicHash}, Profile Id: {profileId}, Global Error: {GlobalError}, Errors: {Errors}";
    private const string ERROR_GROUP_MESSAGE = "Error occurred while subscribing user to group: Group: {resourceId}, Profile Id: {profileId}, Global Error: {GlobalError}, Errors: {Errors}";
    #endregion

    private readonly ILogger _logger;
    private readonly IFcmTopicService _fcm;
    private readonly IDbService _db;

    /// <summary>
    /// The implementation for the <see cref="ISubscriptionService"/>
    /// </summary>
    /// <param name="logger">The service for logging</param>
    /// <param name="fcm">The FCM subscription service</param>
    /// <param name="db">The database service</param>
    public SubscriptionService(
        ILogger<SubscriptionService> logger, 
        IFcmTopicService fcm, 
        IDbService db)
    {
        _logger = logger;
        _fcm = fcm;
        _db = db;
    }

    /// <summary>
    /// Gets all of the subscriptions for the given user in the context of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the subscriptions</param>
    /// <param name="profileId">The ID of the user who owns the subscriptions</param>
    /// <returns>The users subscriptions</returns>
    public async Task<RequestResult<UserSubscriptions>> Get(Guid appId, string profileId)
    {
        var topics = await _db.Topics.GetNonGroupsByUser(appId, profileId);
        var groups = await _db.Groups.ByUser(appId, profileId);

        return WasOk(new UserSubscriptions(topics, groups));
    }

    /// <summary>
    /// Subscribe the given user to the topic in the contenxt of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the subscription</param>
    /// <param name="topicName">The topic hash / resource ID for the subscription</param>
    /// <param name="profileId">The ID of the user to subscribe to the topic</param>
    /// <returns>The result of the subscription request</returns>
    public async Task<RequestResult> Topic(Guid appId, string topicName, string profileId)
    {
        try
        {
            if (!TopicHelper.ValidTopicName(topicName)) return WasBadRequest(TOPIC_NAME_INVALID);

            //Get or create the topic
            var topicId = await _db.Topics.Upsert(new Topic(appId, topicName));
            //Create the subscription if it doesn't exist and get the subscription ID
            var subId = await _db.Subscriptions.Upsert(new TopicSubscription(profileId, topicId));
            //Get all of the users device tokens for the current application
            var devices = await _db.DeviceTokens.Get(appId, profileId);
            //Not tokens? no problem.
            if (devices == null || devices.Length == 0) return WasNoContent(USER_NO_DEVICES);
            //Subscribe the devices to the topic
            var result = await _fcm.SubscribeDevicesPerTopic(topicName, devices.Select(d => d.Token).ToArray());
            //No errors while subscribing? Great!
            if (string.IsNullOrEmpty(result.GlobalError) && !result.Errors.Any()) return WasOk();
            //Log the error, delete the subscription, and return the error
            _logger.LogError(ERROR_TOPIC_MESSAGE, topicName, profileId, result.GlobalError, string.Join(", ", result.Errors));
            await _db.Subscriptions.Delete(subId);
            return WasException();
        }
        catch (Exception ex)
        {   
            _logger.LogError(ex, EXCEPTION_TOPIC_MESSAGE, profileId, topicName, appId);
            return WasException();
        }
    }

    /// <summary>
    /// Subscribe the given user to the group in the context of the application
    /// </summary>
    /// <param name="appId">The ID of the application that owns the subscription</param>
    /// <param name="resourceId">The resource ID for the group</param>
    /// <param name="profileId">The ID of the user to subscribe to the group</param>
    /// <returns>The result of the subscription request</returns>
    public async Task<RequestResult> Group(Guid appId, string resourceId, string profileId)
    {
        try
        {
            //Get or create the group
            var groupId = await _db.Groups.Upsert(new TopicGroup(appId, resourceId));
            //Get or create the subscription to the group
            var subId = await _db.GroupSubscriptions.Upsert(new TopicGroupSubscription(profileId, groupId));
            //Get all of the topics in the group
            var topics = await _db.Topics.GetByGroup(appId, groupId);
            if (topics.Length == 0) return WasNoContent(GROUP_NO_TOPICS);
            //Create the subscriptions to the topics for the group
            await topics.Select(t => _db.Subscriptions.Upsert(new TopicSubscription(profileId, t.Id, groupId))).WhenAll();
            //Get all of the users device tokens for the current application
            var devices = await _db.DeviceTokens.Get(appId, profileId);
            if (devices.Length == 0) return WasNoContent(USER_NO_DEVICES);
            //Subscribe the devices to the topics
            var tokens = devices.Select(t => t.Token).ToArray();
            var topicHashes = topics.Select(t => t.TopicHash).ToArray();
            var result = await _fcm.SubscribeTopicsAndDevices(tokens, topicHashes);
            //No errors while subscribing? Great!
            if (result.Failures == 0) return WasOk();
            //Log the error, delete the subscription, and return the error
            await _db.GroupSubscriptions.Delete(subId);
            await _db.Subscriptions.DeleteByGroup(groupId, profileId);
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
