namespace CardboardBox.PushNotifications.Fcm;

/// <summary>
/// A collection of helpful methods for interacting with FCM topics
/// </summary>
public static partial class TopicHelper
{
    /// <summary>
    /// The valid regex for a topic name as per: https://firebase.google.com/docs/cloud-messaging/send-message#send-messages-to-topics-legacy
    /// The documentation is for legacy topics, but the regex is still valid for the new topics and the `/topics/` prefix is not required anymore.
    /// Unfortunately, there is no official documentation outlining this but the return results from attempting invalid topic names do reflect this.
    /// </summary>
    public const string TopicRegex = "^[a-zA-Z0-9-_.~%]{1,900}$";

    /// <summary>
    /// Validate that the topic name is valid.
    /// Needs to match the given regex: ^[a-zA-Z0-9-_.~%]{1,900}$
    /// </summary>
    /// <param name="topic">The topic name to validate</param>
    /// <returns>Whether or not the topic name is valid</returns>
    public static bool ValidTopicName(string topic) => ValidTopicRegex().IsMatch(topic);

    /// <summary>
    /// Builds a topic or conditional string for use in a FCM notifications.
    /// </summary>
    /// <param name="topics">The topics to create the conditional for</param>
    /// <returns>The build or conditional</returns>
    public static string BuildTopicOrConditional(params string[] topics)
    {
        var names = topics.Select(t => $"'{t}' in topics");
        return string.Join(" || ", names);
    }

    /// <summary>
    /// The regex to validate a topic name (<see cref="TopicRegex"/>)
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(TopicRegex)]
    private static partial Regex ValidTopicRegex();
}
