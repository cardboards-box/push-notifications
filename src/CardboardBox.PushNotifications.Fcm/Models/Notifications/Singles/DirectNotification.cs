namespace CardboardBox.PushNotifications.Fcm;

/// <summary>
/// Represents a notification to be sent directly to an individual device
/// </summary>
public class DirectNotification : BaseSingleNotification
{
    /// <summary>
    /// The token of the device to send the notification to
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Represents a notification to be sent directly to an individual device
    /// </summary>
    /// <param name="token">The token of the device to send the notification to</param>
    /// <param name="data">The data to send with the notification</param>
    public DirectNotification(string token, NotificationData data) : base(data)
    {
        Token = token;
    }

    /// <summary>
    /// Generates all of the notifications to be sent for this device
    /// </summary>
    /// <returns>All of the FCM messages for this notification</returns>
    /// <exception cref="ArgumentException">Thrown if the <see cref="Token"/> isn't provided</exception>
    public override IEnumerable<Message> GenerateMessages()
    {
        if (string.IsNullOrEmpty(Token))
            throw new ArgumentException("Token cannot be null or empty");

        yield return new Message
        {
            Token = Token,
            Notification = GetNotification(),
            Data = Data.Data ?? new()
        };
    }
}
