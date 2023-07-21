namespace CardboardBox.PushNotifications.Fcm;

/// <summary>
/// Creates a builder for generating notifications
/// </summary>
public interface INotificationsBuilder
{
    /// <summary>
    /// All of the notifications currently in the builder
    /// </summary>
    IReadOnlyCollection<INotification> Notifications { get; }

    /// <summary>
    /// Adds the given notification to the builder
    /// </summary>
    /// <param name="notification">The notification to add</param>
    /// <returns>The current builder for chaining</returns>
    INotificationsBuilder Notification(INotification notification);

    /// <summary>
    /// Adds the given notification to be sent to the given topics to the builder
    /// </summary>
    /// <param name="data">The notification data to be sent</param>
    /// <param name="topics">The topics to send the notification to</param>
    /// <returns>The current builder for chaining</returns>
    INotificationsBuilder Topic(NotificationData data, params string[] topics);

    /// <summary>
    /// Adds the given notification to be sent to the given device tokens to the builder
    /// </summary>
    /// <param name="data">The notification data to be sent</param>
    /// <param name="tokens">The device tokens to send the notification to</param>
    /// <returns>The current builder for chaining</returns>
    INotificationsBuilder Direct(NotificationData data, params string[] tokens);
}

/// <summary>
/// The implementation of the <see cref="INotificationsBuilder"/>
/// </summary>
public class NotificationsBuilder : INotificationsBuilder, INotification
{
    private readonly List<INotification> _notifications = new();

    /// <summary>
    /// All of the notifications currently in the builder
    /// </summary>
    public IReadOnlyCollection<INotification> Notifications => _notifications.AsReadOnly();

    /// <summary>
    /// Adds the given notification to the builder
    /// </summary>
    /// <param name="notification">The notification to add</param>
    /// <returns>The current builder for chaining</returns>
    public INotificationsBuilder Notification(INotification notification)
    {
        _notifications.Add(notification);
        return this;
    }

    /// <summary>
    /// Adds the given notification to be sent to the given topics to the builder
    /// </summary>
    /// <param name="data">The notification data to be sent</param>
    /// <param name="topics">The topics to send the notification to</param>
    /// <returns>The current builder for chaining</returns>
    public INotificationsBuilder Topic(NotificationData data, params string[] topics)
    {
        if (topics.Length == 0) return this;

        if (topics.Length == 1) 
            return Notification(new TopicNotification(topics.First(), data));

        return Notification(new TopicsNotification(topics, data));
    }

    /// <summary>
    /// Adds the given notification to be sent to the given device tokens to the builder
    /// </summary>
    /// <param name="data">The notification data to be sent</param>
    /// <param name="tokens">The device tokens to send the notification to</param>
    /// <returns>The current builder for chaining</returns>
    public INotificationsBuilder Direct(NotificationData data, params string[] tokens)
    {
        if (tokens.Length == 0) return this;

        if (tokens.Length == 1)
            return Notification(new DirectNotification(tokens.First(), data));

        return Notification(new MulticastNotification(tokens, data));
    }

    /// <summary>
    /// Generates all of the notifications to be sent
    /// </summary>
    /// <returns>The notifications to be sent</returns>
    /// <exception cref="NotImplementedException">Thrown if any of the notifications don't implement either <see cref="ISingleNotification"/> or <see cref="IMulticastNotification"/></exception>
    public (Message[] singles, MulticastMessage[] multicasts) Generate()
    {
        var singles = new List<Message>();
        var multicasts = new List<MulticastMessage>();

        foreach(var notification in _notifications)
        {
            if (notification is null) continue;

            switch(notification)
            {
                case ISingleNotification s: singles.AddRange(s.GetMessages()); break;
                case IMulticastNotification m: multicasts.AddRange(m.GetMessages()); break;
                default: throw new NotImplementedException($"The given notification has no message output: {notification.GetType().Name}");
            }
        }

        return (singles.ToArray(), multicasts.ToArray());
    }
}
