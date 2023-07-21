namespace CardboardBox.PushNotifications.Fcm;

/// <summary>
/// Represents a notification to be sent directly to multiple devices
/// </summary>
public class MulticastNotification : BaseMulticastNotification
{
    /// <summary>
    /// The max number of device tokens allowed by multicast messages. 
    /// This is defined by: https://firebase.google.com/docs/cloud-messaging/send-message#send-messages-to-multiple-devices
    /// </summary>
    public const int MAX_DEVICES = 500;

    /// <summary>
    /// The tokens to the send the notification to
    /// </summary>
    public string[] Tokens { get; set; }

    /// <summary>
    /// Represents a notification to be sent directly to multiple devices
    /// </summary>
    /// <param name="tokens">The tokens to the send the notification to</param>
    /// <param name="data">The data to send with the notification</param>
    public MulticastNotification(string[] tokens, NotificationData data) : base(data)
    {
        Tokens = tokens;
    }

    /// <summary>
    /// Represents a notification to be sent directly to multiple devices
    /// </summary>
    /// <param name="data">The data to send with the notification</param>
    /// <param name="tokens">The tokens to the send the notification to</param>
    public MulticastNotification(NotificationData data, params string[] tokens): base(data)
    {
        Tokens = tokens;
    }

    /// <summary>
    /// Generates all of the multicast notifications to be sent for these devices, split into chunks of <see cref="MAX_DEVICES"/>
    /// </summary>
    /// <returns>All of the FCM messages for this notification</returns>
    public override IEnumerable<MulticastMessage> GenerateMessages()
    {
        if (Tokens == null)
            throw new ArgumentException("Tokens cannot be null");

        if (Tokens.Length == 0)
            throw new ArgumentException("At least 2 tokens are required");

        if (Tokens.Length == 1)
            throw new ArgumentException($"At least 2 tokens are required. Please use a {nameof(DirectNotification)} instead");

        //Split the tokens into chunks of 500 devices.
        var chunks = Tokens.Chunk(MAX_DEVICES);
        foreach (var chunk in chunks)
            yield return new MulticastMessage
            {
                Tokens = chunk,
                Notification = GetNotification(),
                Data = Data.Data ?? new()
            };
    }
}
