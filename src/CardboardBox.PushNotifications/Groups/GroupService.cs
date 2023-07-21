namespace CardboardBox.PushNotifications.Groups;

using static RequestResults;

/// <summary>
/// A service for managing group mappings
/// </summary>
public interface IGroupService
{
    /// <summary>
    /// Map the topic to the given group
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="resourceId">The ID of the group</param>
    /// <param name="topic">The topic to map</param>
    /// <returns>The result of the request</returns>
    Task<RequestResult> MapTopic(Guid appId, string resourceId, string topic);

    /// <summary>
    /// Unmaps the topic from the given group
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="resourceId">The ID of the group</param>
    /// <param name="topic">The topic to unmap</param>
    /// <returns>The result of the request</returns>
    Task<RequestResult> UnmapTopic(Guid appId, string resourceId, string topic);
}

/// <summary>
/// The implementation of the <see cref="IGroupService"/>
/// </summary>
public class GroupService : IGroupService
{
    private const string EXCEPTION_MAP_MESSAGE = "Error occurred while mapping topic {topic} to group {resourceId} for {appId}";
    private const string EXCEPTION_UNMAP_MESSAGE = "Error occurred while unmapping topic {topic} from group {resourceId} for {appId}";
    private const string ERROR_MAP_MESSAGE = "Error occurred while mapping topic to group: Topic: {topic}, Group: {resourceId}, App Id: {appId}, Global Error: {GlobalError}, Errors: {Errors}";
    private const string ERROR_UNMAP_MESSAGE = "Error occurred while unmapping topic from group: Topic: {topic}, Group: {resourceId}, App Id: {appId}, Global Error: {GlobalError}, Errors: {Errors}";

    private readonly ILogger _logger;
    private readonly IFcmTopicService _fcm;
    private readonly IDbService _db;

    /// <summary>
    /// The implementation of the <see cref="IGroupService"/>
    /// </summary>
    /// <param name="logger">The service for logging</param>
    /// <param name="fcm">The FCM topic service</param>
    /// <param name="db">The database service</param>
    public GroupService(
        ILogger<GroupService> logger,
        IFcmTopicService fcm,
        IDbService db)
    {
        _logger = logger;
        _fcm = fcm;
        _db = db;
    }

    /// <summary>
    /// Map the topic to the given group
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="resourceId">The ID of the group</param>
    /// <param name="topic">The topic to map</param>
    /// <returns>The result of the request</returns>
    public async Task<RequestResult> MapTopic(Guid appId, string resourceId, string topic)
    {
        try
        {
            //Validate the topic name
            if (!TopicHelper.ValidTopicName(topic)) return WasBadRequest(TOPIC_NAME_INVALID);
            //Create or get all of the resources
            var topicId = await _db.Topics.Upsert(new Topic(appId, topic));
            var groupId = await _db.Groups.Upsert(new TopicGroup(appId, resourceId));
            var mapId = await _db.GroupMaps.Upsert(new TopicGroupMap(groupId, topicId));
            //Get all of the devices currently subscribed to the group
            var devices = await _db.DeviceTokens.ByGroup(appId, groupId);
            //No devices? no problem.
            if (devices.Length == 0) return WasNoContent(GROUP_NO_SUBSCRIBERS);
            //Get all of the tokens and subscribe them to the topic
            var tokens = devices.Select(t => t.Token).Distinct().ToArray();
            var result = await _fcm.SubscribeDevicesPerTopic(topic, tokens);
            //No errors while subscribing? Great!
            if (string.IsNullOrEmpty(result.GlobalError) && !result.Errors.Any()) return WasOk();
            //Log the error, and return the error
            _logger.LogError(ERROR_MAP_MESSAGE, topic, resourceId, appId, result.GlobalError, string.Join(", ", result.Errors));
            return WasException();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, EXCEPTION_MAP_MESSAGE, topic, resourceId, appId);
            return WasException();
        }
    }

    /// <summary>
    /// Unmaps the topic from the given group
    /// </summary>
    /// <param name="appId">The ID of the application</param>
    /// <param name="resourceId">The ID of the group</param>
    /// <param name="topic">The topic to unmap</param>
    /// <returns>The result of the request</returns>
    public async Task<RequestResult> UnmapTopic(Guid appId, string resourceId, string topic)
    {
        try
        {
            //Validate the topic name
            if (!TopicHelper.ValidTopicName(topic)) return WasBadRequest(TOPIC_NAME_INVALID);
            //Get the mapping
            var map = await _db.GroupMaps.Fetch(appId, resourceId, topic);
            if (map == null) return WasNoContent(GROUP_MAP_NOT_FOUND);
            //Get all of the devices subscribed to the topic via the map
            var devices = await _db.DeviceTokens.ByMap(map.Id);
            //No devices? no problem.
            if (devices.Length == 0) return WasNoContent(GROUP_NO_SUBSCRIBERS);
            //Get all of the tokens and unsubscribe them from the topic
            var tokens = devices.Select(t => t.Token).Distinct().ToArray();
            var result = await _fcm.UnsubscribeDevicesPerTopic(topic, tokens);
            //No errors while subscribing? Great!
            if (string.IsNullOrEmpty(result.GlobalError) && !result.Errors.Any()) return WasOk();
            //Log the error, and return the error
            _logger.LogError(ERROR_UNMAP_MESSAGE, topic, resourceId, appId, result.GlobalError, string.Join(", ", result.Errors));
            return WasException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, EXCEPTION_UNMAP_MESSAGE, topic, resourceId, appId);
            return WasException();
        }
    }
}
