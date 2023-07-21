namespace CardboardBox.PushNotifications.Fcm;

/// <summary>
/// Represents a notification to be sent to a single topic
/// </summary>
public class TopicNotification : BaseSingleNotification
{
    /// <summary>
    /// The topic to send the notification to
    /// </summary>
    public string Topic { get; set; }

    /// <summary>
    /// Represents a notification to be sent to a single topic
    /// </summary>
    /// <param name="topic">The topic to send the notification to</param>
    /// <param name="data">The data to send with the notification</param>
    public TopicNotification(string topic, NotificationData data) : base(data)
    {
        Topic = topic;
    }

    /// <summary>
    /// Generates all of the FCM messages to be sent for this topic
    /// </summary>
    /// <returns>All of the FCM messages for this notification</returns>
    /// <exception cref="ArgumentException">Thrown if the <see cref="Topic"/> is invalid, null or empty</exception>
    public override IEnumerable<Message> GenerateMessages()
    {
        if (string.IsNullOrEmpty(Topic))
            throw new ArgumentException("Topic cannot be null or empty");

        if (!TopicHelper.ValidTopicName(Topic))
            throw new ArgumentException("Invalid topic. It needs to match " + TopicHelper.TopicRegex);

        yield return new Message
        {
            Topic = Topic,
            Notification = GetNotification(),
            Data = Data.Data ?? new()
        };
    }
}
