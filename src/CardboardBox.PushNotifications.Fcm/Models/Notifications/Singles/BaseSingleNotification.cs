namespace CardboardBox.PushNotifications.Fcm;

/// <summary>
/// The interface that represents a <see cref="Message"/> notification that is sent to FCM
/// </summary>
public interface ISingleNotification : INotification
{
    /// <summary>
    /// Generates all of the FCM messages to be sent for this notification
    /// </summary>
    /// <returns>All of the FCM messages for this notification</returns>
    IEnumerable<Message> GetMessages();
}

/// <summary>
/// Base class for <see cref="ISingleNotification"/> implementations
/// </summary>
public abstract class BaseSingleNotification : BaseNotification, ISingleNotification
{
    /// <summary>
    /// The base notification data
    /// </summary>
    /// <param name="data">The data to send with the notification</param>
    public BaseSingleNotification(NotificationData data) : base(data) { }

    /// <summary>
    /// Validates the notification data and generates all of the FCM messages to be sent for this notification
    /// </summary>
    /// <returns>All of the FCM messages for this notification</returns>
    public virtual IEnumerable<Message> GetMessages()
    {
        ValidateNotification();
        return GenerateMessages();
    }

    /// <summary>
    /// Generates all of the FCM messages to be sent for this notification
    /// </summary>
    /// <returns>All of the FCM messages for this notification</returns>
    public abstract IEnumerable<Message> GenerateMessages();
}
