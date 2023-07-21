namespace CardboardBox.PushNotifications.Fcm;

using FcmAction = Func<IReadOnlyList<string>, string, Task<TopicManagementResponse>>;

/// <summary>
/// A service for interacting the topic management requests within FCM
/// </summary>
public interface IFcmTopicService
{
    /// <summary>
    /// Subscribe the given devices to the given topic
    /// </summary>
    /// <param name="topic">The topic to subscribe to</param>
    /// <param name="tokens">The devices to subscribe</param>
    /// <returns>Response representing the result of the subscription request</returns>
    Task<TopicSubResponse> SubscribeDevicesPerTopic(string topic, params string[] tokens);

    /// <summary>
    /// Unsubscribe the given devices from the given topic
    /// </summary>
    /// <param name="topic">The topic to unsubscribe from</param>
    /// <param name="tokens">The devices to unsubscribe</param>
    /// <returns>Response representing the result of the unsubscribe request</returns>
    Task<TopicSubResponse> UnsubscribeDevicesPerTopic(string topic, params string[] tokens);

    /// <summary>
    /// Subscribes one device to multiple topics
    /// </summary>
    /// <param name="token">The device to subscribe to the topics</param>
    /// <param name="topics">The topics to subscribe the device to</param>
    /// <returns>Response representing the result of the subscribe requests</returns>
    Task<TopicSubResponses> SubscribeTopicsPerDevice(string token, params string[] topics);

    /// <summary>
    /// Unsubscribes one device from multiple topics
    /// </summary>
    /// <param name="token">The device token to unsubscribe from the topics</param>
    /// <param name="topics">The topics to unsubscribe the device from</param>
    /// <returns>Response representing the result of the unsubscribe requests</returns>
    Task<TopicSubResponses> UnsubscribeTopicsPerDevice(string token, params string[] topics);

    /// <summary>
    /// Subscribe the given devices to the given topics
    /// </summary>
    /// <param name="tokens">The devices to subscribe to the topics</param>
    /// <param name="topics">The topics to subscribe the devices to</param>
    /// <returns>Response representing the result of the subscribe requests</returns>
    Task<TopicSubResponses> SubscribeTopicsAndDevices(string[] tokens, string[] topics);

    /// <summary>
    /// Unsubscribe the given devices from the given topics
    /// </summary>
    /// <param name="tokens">The devices to unsubscribe from the topics</param>
    /// <param name="topics">The topics to unsubscribe the devices from</param>
    /// <returns>Response representing the result of the unsubscribe requests</returns>
    Task<TopicSubResponses> UnsubscribeTopicsAndDevices(string[] tokens, string[] topics);
}

/// <summary>
/// The implementation of the <see cref="IFcmTopicService"/>
/// </summary>
public partial class FcmTopicService : IFcmTopicService
{
    private readonly IFcmService _fcm;

    /// <summary>
    /// The dependency injection constructor for the <see cref="FcmTopicService"/>
    /// </summary>
    /// <param name="fcm">The service that handles the <see cref="FirebaseMessaging"/> instance</param>
    public FcmTopicService(IFcmService fcm)
    {
        _fcm = fcm;
    }

    #region Manage a single topic
    /// <summary>
    /// Subscribe the given tokens to the given topic
    /// </summary>
    /// <param name="topic">The topic to subscribe to</param>
    /// <param name="tokens">The tokens to subscribe</param>
    /// <returns>Response representing the result of the subscription request</returns>
    public Task<TopicSubResponse> SubscribeDevicesPerTopic(string topic, params string[] tokens) => ManageTopic(topic, tokens, true);

    /// <summary>
    /// Unsubscribe the given tokens from the given topic
    /// </summary>
    /// <param name="topic">The topic to unsubscribe from</param>
    /// <param name="tokens">The tokens to unsubscribe</param>
    /// <returns>Response representing the result of the unsubscribe request</returns>
    public Task<TopicSubResponse> UnsubscribeDevicesPerTopic(string topic, params string[] tokens) => ManageTopic(topic, tokens, false);

    /// <summary>
    /// Handles interfacing with FCM to manage topic subscriptions
    /// </summary>
    /// <param name="topic">The topic to target</param>
    /// <param name="tokens">The tokens to manage</param>
    /// <param name="subscribe">Whether to subscribe or unsubscribe</param>
    /// <returns>Response representing the result of the request</returns>
    public async Task<TopicSubResponse> ManageTopic(string topic, string[] tokens, bool subscribe)
    {
        var actualTokens = tokens
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .ToArray();

        if (actualTokens.Length == 0)
            return new(topic, subscribe, new(), "No valid tokens provided");

        var response = await DetermineAction(subscribe)(actualTokens, topic);
        return Convert(response, topic, actualTokens, subscribe);
    }
    #endregion

    #region Manage multiple topics for one device
    /// <summary>
    /// Subscribes one device to multiple topics
    /// </summary>
    /// <param name="token">The device to subscribe to the topics</param>
    /// <param name="topics">The topics to subscribe the device to</param>
    /// <returns>Response representing the result of the subscribe requests</returns>
    public Task<TopicSubResponses> SubscribeTopicsPerDevice(string token, params string[] topics) => ManageTopics(new[] { token }, topics, true);

    /// <summary>
    /// Unsubscribes one device from multiple topics
    /// </summary>
    /// <param name="token">The device token to unsubscribe from the topics</param>
    /// <param name="topics">The topics to unsubscribe the device from</param>
    /// <returns>Response representing the result of the unsubscribe requests</returns>
    public Task<TopicSubResponses> UnsubscribeTopicsPerDevice(string token, params string[] topics) => ManageTopics(new[] { token }, topics, false);
    #endregion

    #region Manage multiple topics for multiple devices
    /// <summary>
    /// Subscribe the given devices to the given topics
    /// </summary>
    /// <param name="tokens">The devices to subscribe to the topics</param>
    /// <param name="topics">The topics to subscribe the devices to</param>
    /// <returns>Response representing the result of the subscribe requests</returns>
    public Task<TopicSubResponses> SubscribeTopicsAndDevices(string[] tokens, string[] topics) => ManageTopics(tokens, topics, true);

    /// <summary>
    /// Unsubscribe the given devices from the given topics
    /// </summary>
    /// <param name="tokens">The devices to unsubscribe from the topics</param>
    /// <param name="topics">The topics to unsubscribe the devices from</param>
    /// <returns>Response representing the result of the unsubscribe requests</returns>
    public Task<TopicSubResponses> UnsubscribeTopicsAndDevices(string[] tokens, string[] topics) => ManageTopics(tokens, topics, false);
    #endregion

    /// <summary>
    /// Handles interfacing with FCM to manage topic subscriptions
    /// </summary>
    /// <param name="tokens">The tokens to manage</param>
    /// <param name="topics">The topics to target</param>
    /// <param name="subscribe">Whether to subscribe or unsubscribe</param>
    /// <returns>Response representing the result of the request</returns>
    public async Task<TopicSubResponses> ManageTopics(string[] tokens, string[] topics, bool subscribe)
    {
        var action = DetermineAction(subscribe);
        var actualTokens = tokens
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .ToArray();

        var responses = await topics
            .Select(async t =>
            {
                if (!TopicHelper.ValidTopicName(t))
                    return new TopicSubResponse(t, subscribe, new(), "Invalid topic name");
                var response = await action(actualTokens, t);
                return Convert(response, t, actualTokens, subscribe);
            })
            .WhenAll();

        var failures = responses.Sum(t => t.Errors.Count);
        return new TopicSubResponses(actualTokens, subscribe, responses, failures);
    }

    #region Utility methods
    
    /// <summary>
    /// Gets the FCM management action corresponding to the given subscribe value
    /// </summary>
    /// <param name="subscribe">Whether its a subscribe or unsubscribe method</param>
    /// <returns>The FCM management action</returns>
    public FcmAction DetermineAction(bool subscribe)
    {
        var msg = _fcm.GetMessaging();
        return subscribe
            ? msg.SubscribeToTopicAsync
            : msg.UnsubscribeFromTopicAsync;
    }

    /// <summary>
    /// Converts the <see cref="TopicManagementResponse"/> to a <see cref="TopicSubResponse"/>
    /// </summary>
    /// <param name="response">The response from the FCM management request</param>
    /// <param name="topic">The topic that was managed</param>
    /// <param name="actualTokens">The tokens that were managed</param>
    /// <param name="subscribe">Whether or not it was a subscription or an unsubscription</param>
    /// <returns>The converted response</returns>
    public static TopicSubResponse Convert(TopicManagementResponse response, string topic, string[] actualTokens, bool subscribe)
    {
        if (response.FailureCount == 0)
            return new(topic, subscribe, new());

        var dic = new Dictionary<string, string>();
        foreach (var item in response.Errors)
            dic.Add(actualTokens[item.Index], item.Reason);

        return new(topic, true, dic);
    }

    #endregion
}