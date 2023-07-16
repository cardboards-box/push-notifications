using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace CardboardBox.PushNotifications.Fcm;

using Core;

/// <summary>
/// A service for interfacing with Firebase Cloud Messaging (FCM)
/// </summary>
public interface IFcmNotificationService
{
    /// <summary>
    /// Validate that the topic name is valid.
    /// Needs to match the given regex: ^[a-zA-Z0-9-_.~%]{1,900}$
    /// </summary>
    /// <param name="topic">The topic name to validate</param>
    /// <returns>Whether or not the topic name is valid</returns>
    bool ValidTopicName(string topic);

    /// <summary>
    /// Subscribe the given tokens to the given topic
    /// </summary>
    /// <param name="topic">The topic to subscribe to</param>
    /// <param name="tokens">The tokens to subscribe</param>
    /// <returns>Response representing the result of the subscription request</returns>
    Task<TopicSubResponse> Subscribe(string topic, params string[] tokens);

    /// <summary>
    /// Unsubscribe the given tokens from the given topic
    /// </summary>
    /// <param name="topic">The topic to unsubscribe from</param>
    /// <param name="tokens">The tokens to unsubscribe</param>
    /// <returns>Response representing the result of the unsubscribe request</returns>
    Task<TopicSubResponse> Unsubscribe(string topic, params string[] tokens);

    /// <summary>
    /// Send a push notification directly to the given token
    /// </summary>
    /// <param name="token">The token to send the notification to</param>
    /// <param name="notification">The notification to send</param>
    /// <returns>The returned string from FCM</returns>
    Task<string> SendDirect(string token, NotificationData notification);

    /// <summary>
    /// Send a push notification to the given topic
    /// </summary>
    /// <param name="topic">The topic to send the notification to</param>
    /// <param name="notification">The notification to send</param>
    /// <returns>The returned string from FCM</returns>
    Task<string> SendTopic(string topic, NotificationData notification);
}

/// <summary>
/// The implementation of the <see cref="IFcmNotificationService"/>
/// </summary>
public partial class FcmNotificationService : IFcmNotificationService
{
    private readonly IConfiguration _config;
    private readonly FirebaseMessaging _messaging;

    /// <summary>
    /// Where to find the credentials JSON file for the FCM service
    /// </summary>
    public string CredentialsPath => _config.Required("FCM:CredentialsPath");

    /// <summary>
    /// The scope to use for the FCM credentials
    /// </summary>
    public string Scope => _config.Optional("FCM:Scope", "https://www.googleapis.com/auth/firebase.messaging");

    /// <summary>
    /// The implementation of the <see cref="IFcmNotificationService"/>
    /// </summary>
    /// <param name="config">The configuration options</param>
    public FcmNotificationService(IConfiguration config)
    {
        _config = config;
        _messaging = CreateInstance();
    }

    /// <summary>
    /// Validate that the topic name is valid.
    /// Needs to match the given regex: ^[a-zA-Z0-9-_.~%]{1,900}$
    /// </summary>
    /// <param name="topic">The topic name to validate</param>
    /// <returns>Whether or not the topic name is valid</returns>
    public bool ValidTopicName(string topic)
    {
        var regex = new Regex("^[a-zA-Z0-9-_.~%]{1,900}$");
        return regex.IsMatch(topic);
    }

    /// <summary>
    /// Subscribe the given tokens to the given topic
    /// </summary>
    /// <param name="topic">The topic to subscribe to</param>
    /// <param name="tokens">The tokens to subscribe</param>
    /// <returns>Response representing the result of the subscription request</returns>
    public Task<TopicSubResponse> Subscribe(string topic, params string[] tokens) => ManageTopic(topic, tokens, true);

    /// <summary>
    /// Unsubscribe the given tokens from the given topic
    /// </summary>
    /// <param name="topic">The topic to unsubscribe from</param>
    /// <param name="tokens">The tokens to unsubscribe</param>
    /// <returns>Response representing the result of the unsubscribe request</returns>
    public Task<TopicSubResponse> Unsubscribe(string topic, params string[] tokens) => ManageTopic(topic, tokens, false);

    /// <summary>
    /// Handles interfacing with FCM to manage topic subscriptions
    /// </summary>
    /// <param name="topic">The topic to target</param>
    /// <param name="tokens">The tokens to manage</param>
    /// <param name="subscribe">Whether to subscribe or unsubscribe</param>
    /// <returns>Response representing the result of the request</returns>
    public async Task<TopicSubResponse> ManageTopic(string topic, string[] tokens, bool subscribe)
    {
        Func<IReadOnlyList<string>, string, Task<TopicManagementResponse>> action = subscribe
            ? _messaging.SubscribeToTopicAsync
            : _messaging.UnsubscribeFromTopicAsync;

        var actualTokens = tokens
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .ToArray();

        if (actualTokens.Length == 0)
            return new(topic, subscribe, new(), "No valid tokens provided");

        var response = await action(actualTokens, topic);
        if (response.FailureCount == 0)
            return new(topic, subscribe, new());

        var dic = new Dictionary<string, string>();
        foreach (var item in response.Errors)
            dic.Add(actualTokens[item.Index], item.Reason);

        return new(topic, true, dic);
    }

    /// <summary>
    /// Send a push notification directly to the given token
    /// </summary>
    /// <param name="token">The token to send the notification to</param>
    /// <param name="notification">The notification to send</param>
    /// <returns>The returned string from FCM</returns>
    public Task<string> SendDirect(string token, NotificationData notification) => Send(notification, token);

    /// <summary>
    /// Send a push notification to the given topic
    /// </summary>
    /// <param name="topic">The topic to send the notification to</param>
    /// <param name="notification">The notification to send</param>
    /// <returns>The returned string from FCM</returns>
    public Task<string> SendTopic(string topic, NotificationData notification) => Send(notification, topic: topic);

    /// <summary>
    /// Send a push notification to the given token or topic
    /// </summary>
    /// <param name="notification">The notification to send</param>
    /// <param name="token">The token to send the notification to (use either this or <paramref name="topic"/>, not both)</param>
    /// <param name="topic">The topic to send the notification to (use either this or <paramref name="token"/>, not both)</param>
    /// <returns>The returned string from FCM</returns>
    public async Task<string> Send(NotificationData notification, string? token = null, string? topic = null)
    {
        var (title, content, imageUrl, data) = notification;

        var message = new Message
        {
            Token = token,
            Topic = topic,
            Notification = new Notification
            {
                Title = title,
                Body = content,
                ImageUrl = imageUrl
            },
            Data = data ?? new()
        };

        return await _messaging.SendAsync(message);
    }

    /// <summary>
    /// Create an instance of the firebase messaging handler
    /// </summary>
    /// <returns></returns>
    public FirebaseMessaging CreateInstance()
    {
        var creds = GoogleCredential
            .FromFile(CredentialsPath)
            .CreateScoped(Scope);
        var opts = new AppOptions
        {
            Credential = creds
        };
        var app = FirebaseApp.Create(opts);
        return FirebaseMessaging.GetMessaging(app);
    }

}