namespace CardboardBox.PushNotifications.Fcm;

/// <summary>
/// A service for sending push notifications to FCM
/// </summary>
public interface IFcmNotificationService
{
    /// <summary>
    /// Sends all of the given notifications to FCM
    /// </summary>
    /// <param name="builder">The notification builder</param>
    /// <returns>The response codes of the messages sent to FCM</returns>
    IAsyncEnumerable<NotificationResponse> Send(Action<INotificationsBuilder> builder);
}

/// <summary>
/// The implementation of the <see cref="IFcmNotificationService"/>
/// </summary>
public class FcmNotificationService : IFcmNotificationService
{
    /// <summary>
    /// The max number of messages allowed in a single batch: https://firebase.google.com/docs/cloud-messaging/send-message#send-a-batch-of-messages
    /// </summary>
    public const int MAX_BATCH_SIZE = 500;

    private readonly IFcmService _fcm;
    private readonly ILogger _logger;

    /// <summary>
    /// The dependency injection constructor for the <see cref="FcmNotificationService"/>
    /// </summary>
    /// <param name="fcm">The service that handles the <see cref="FirebaseMessaging"/> instance</param>
    /// <param name="logger">The service that handles logging</param>
    public FcmNotificationService(
        IFcmService fcm,
        ILogger<FcmNotificationService> logger)
    {
        _fcm = fcm;
        _logger = logger;
    }

    /// <summary>
    /// Sends all of the given notifications to FCM
    /// </summary>
    /// <param name="builder">The notification builder</param>
    /// <returns>The response codes of the messages sent to FCM</returns>
    public IAsyncEnumerable<NotificationResponse> Send(Action<INotificationsBuilder> builder)
    {
        var bob = new NotificationsBuilder();
        builder(bob);

        var (singles, multicasts) = bob.Generate();
        return Send(singles, multicasts);
    }

    /// <summary>
    /// Sends all of the given messages to FCM
    /// </summary>
    /// <param name="singles">The individual messages to send the FCM</param>
    /// <param name="multicasts">The multicast messages to send to FCM</param>
    /// <returns>The response codes of the messages sent to FCM</returns>
    public async IAsyncEnumerable<NotificationResponse> Send(Message[] singles, MulticastMessage[] multicasts)
    {
        if (singles.Length == 0 && multicasts.Length == 0)
            yield break;

        var msg = _fcm.GetMessaging();
        await foreach (var resp in SendBatch(msg, singles))
            yield return resp;

        await foreach(var resp in SendMulticast(msg, multicasts))
            yield return resp;
    }

    /// <summary>
    /// Send a batch of multicast messages to FCM
    /// </summary>
    /// <param name="fcm">The FCM instance to send the messages to</param>
    /// <param name="multicasts">The messages to send to FCM</param>
    /// <returns>The response messages from FCM / the validator</returns>
    public async IAsyncEnumerable<NotificationResponse> SendMulticast(FirebaseMessaging fcm, MulticastMessage[] multicasts)
    {
        if (multicasts.Length == 0) yield break;

        foreach (var message in multicasts)
            yield return await Send(fcm, message);
    }

    /// <summary>
    /// Send a multicast message to FCM
    /// </summary>
    /// <param name="fcm">The FCM instance to send the messages to</param>
    /// <param name="message">The multicast message to send to FCM</param>
    /// <returns>The response messages from FCM / the validator</returns>
    public async Task<NotificationResponse> Send(FirebaseMessaging fcm, MulticastMessage message)
    {
        try
        {
            var response = await fcm.SendMulticastAsync(message);
            return NotificationResponse.From(message, response.Responses, _logger);
        }
        catch (Exception ex)
        {
            return NotificationResponse.From(message, ex, _logger);
        }
    }

    /// <summary>
    /// Send a batch of messages to FCM
    /// </summary>
    /// <param name="fcm">The FCM instance to send the messages to</param>
    /// <param name="singles">The messages to send to FCM</param>
    /// <returns>The response messages from FCM / the validator</returns>
    public async IAsyncEnumerable<NotificationResponse> SendBatch(FirebaseMessaging fcm, Message[] singles)
    {
        if (singles.Length == 0) yield break;

        if (singles.Length == 1)
        {
            yield return await Send(fcm, singles.First());
            yield break;
        }

        //Split messages into batches of 500
        var chunks = singles.Chunk(MAX_BATCH_SIZE);
        foreach(var chunk in chunks)
            if (chunk.Length > 0)
                yield return await Send(fcm, chunk);
    }

    /// <summary>
    /// Send a single message to FCM
    /// </summary>
    /// <param name="fcm">The FCM instance to send to</param>
    /// <param name="message">The message to send</param>
    /// <returns>The response from FCM / the validator</returns>
    public async Task<NotificationResponse> Send(FirebaseMessaging fcm, Message message)
    {
        try
        {
            var response = await fcm.SendAsync(message);
            return NotificationResponse.From(message, response);
        }
        catch (Exception ex)
        {
            return NotificationResponse.From(message, ex, _logger);
        }
    }

    /// <summary>
    /// Send a batch of messages to FCM
    /// </summary>
    /// <param name="fcm">The FCM instance to send to</param>
    /// <param name="messages">The batch of messages to send</param>
    /// <returns>The responses from FCM / the validator</returns>
    public async Task<NotificationResponse> Send(FirebaseMessaging fcm, Message[] messages)
    {
        //This just ensures that a single message is sent if the chunk only contains one message
        if (messages.Length == 1) return await Send(fcm, messages.First());

        try
        {
            var response = await fcm.SendAllAsync(messages);
            return NotificationResponse.From(messages, response.Responses, _logger);
        }
        catch (Exception ex)
        {
            return NotificationResponse.From(ex, _logger);
        }
    }
}
