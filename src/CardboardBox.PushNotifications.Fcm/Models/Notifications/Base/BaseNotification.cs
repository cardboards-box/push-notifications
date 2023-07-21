namespace CardboardBox.PushNotifications.Fcm;

/// <summary>
/// Base validations for any of the <see cref="INotification"/> implementations
/// </summary>
public abstract class BaseNotification : INotification
{
    /// <summary>
    /// The data to send with the notification
    /// </summary>
    public NotificationData Data { get; set; }

    /// <summary>
    /// The base notification data
    /// </summary>
    /// <param name="data">The data to send with the notification</param>
    public BaseNotification(NotificationData data)
    {
        Data = data;
    }

    /// <summary>
    /// Validates the notification data
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown if any of the following conditions are met:
    /// The <see cref="Data"/> is null
    /// The <see cref="NotificationData.Title"/> is null or empty
    /// The <see cref="NotificationData.Body"/> is null or empty
    /// </exception>
    public virtual void ValidateNotification()
    {
        if (Data == null) throw new ArgumentException("Notification data cannot be null");
        if (string.IsNullOrEmpty(Data.Title)) throw new ArgumentException("Notification title cannot be null or empty");
        if (string.IsNullOrEmpty(Data.Body)) throw new ArgumentException("Notification body cannot be null or empty");
    }

    /// <summary>
    /// Generates the notification data to be sent with the message
    /// </summary>
    /// <returns>The validated notification data</returns>
    public virtual Notification GetNotification()
    {
        ValidateNotification();
        return new Notification
        {
            Title = Data.Title,
            Body = Data.Body,
            ImageUrl = Data.ImageUrl
        };
    }
}