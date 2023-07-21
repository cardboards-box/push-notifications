namespace CardboardBox.PushNotifications.Fcm;

/// <summary>
/// Represents a notification to be sent to multiple topics
/// </summary>
public class TopicsNotification : BaseSingleNotification
{
    /// <summary>
    /// The max number of topics that can be sent in a single request
    /// </summary>
    public const int MAX_TOPICS = 5;

    /// <summary>
    /// All of the topics to send for this notification (max <see cref="MAX_TOPICS"/>)
    /// </summary>
    public string[] Topics { get; set; }

    /// <summary>
    /// Represents a notification to be sent to multiple topics
    /// </summary>
    /// <param name="topics">All of the topics to send for this notification (max 5)</param>
    /// <param name="data">The data to send with the notification</param>
    public TopicsNotification(string[] topics, NotificationData data) : base(data)
    {
        Topics = topics;
    }

    /// <summary>
    /// Represents a notification to be sent to multiple topics
    /// </summary>
    /// <param name="data">The data to send with the notification</param>
    /// <param name="topics">All of the topics to send for this notification (max 5)</param>
    public TopicsNotification(NotificationData data, params string[] topics) : this(topics, data) { }

    /// <summary>
    /// Generates all of the notifications to be sent for any of these topic
    /// </summary>
    /// <returns>All of the FCM messages for this notification</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if one of the following conditions are met:
    /// Any of the topics are invalid.
    /// There are no topics.
    /// </exception>
    public override IEnumerable<Message> GenerateMessages()
    {
        if (Topics == null || Topics.Length == 0)
            throw new ArgumentException("At least 1 topic is required.");

        if (Topics.Any(x => string.IsNullOrEmpty(x) || !TopicHelper.ValidTopicName(x)))
            throw new ArgumentException("Invalid topic. It needs to match " + TopicHelper.TopicRegex);

        var chunks = Topics.Chunk(MAX_TOPICS);
        foreach(var chunk in chunks)
        {
            if (chunk.Length == 1)
            {
                var noti = new TopicNotification(chunk.First(), Data);
                foreach (var message in noti.GetMessages())
                    yield return message;

                break;
            }

            var conditional = TopicHelper.BuildTopicOrConditional(chunk);
            yield return new Message
            {
                Condition = conditional,
                Notification = GetNotification(),
                Data = Data.Data ?? new()
            };
        }
    }
}

