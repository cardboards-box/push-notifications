namespace CardboardBox.PushNotifications.Subscriptions;

/// <summary>
/// Represents the subscriptions a user has.
/// </summary>
/// <param name="Topics">The topics the user is subscribed to</param>
/// <param name="Groups">The groups the user is subscribed to</param>
public record class UserSubscriptions(Topic[] Topics, TopicGroup[] Groups);
