namespace CardboardBox.PushNotifications.Database;

using Models;
using Services;

/// <summary>
/// The service that rolls up all of the database services
/// </summary>
public interface IDbService
{
    /// <summary>
    /// The service for interacting with the <see cref="Application"/> db objects.
    /// </summary>
    IApplicationDbService Applications { get; }

    /// <summary>
    /// The service for interacting with the <see cref="DeviceToken"/> db objects.
    /// </summary>
    IDeviceTokenDbService DeviceTokens { get; }

    /// <summary>
    /// The service for interacting with the <see cref="Topic"/> db objects.
    /// </summary>
    ITopicDbService Topics { get; }

    /// <summary>
    /// The service for interacting with the <see cref="TopicGroup"/> db objects.
    /// </summary>
    ITopicGroupDbService Groups { get; }

    /// <summary>
    /// The service for interacting with the <see cref="TopicGroupMap"/> db objects.
    /// </summary>
    ITopicGroupMapDbService GroupMaps { get; }

    /// <summary>
    /// The service for interacting with the <see cref="TopicGroupSubscription"/> db objects.
    /// </summary>
    ITopicGroupSubscriptionDbService GroupSubscriptions { get; }

    /// <summary>
    /// The service for interacting with the <see cref="TopicSubscription"/> db objects.
    /// </summary>
    ITopicSubscriptionDbService Subscriptions { get; }

    /// <summary>
    /// The service for interacting with the <see cref="Models.History"/> db objects.
    /// </summary>
    IHistoryDbService History { get; }
}

/// <summary>
/// The implementation of the <see cref="IDbService"/>
/// </summary>
public class DbService : IDbService
{
    /// <summary>
    /// The service for interacting with the <see cref="Application"/> db objects.
    /// </summary>
    public IApplicationDbService Applications { get; }

    /// <summary>
    /// The service for interacting with the <see cref="DeviceToken"/> db objects.
    /// </summary>
    public IDeviceTokenDbService DeviceTokens { get; }

    /// <summary>
    /// The service for interacting with the <see cref="Topic"/> db objects.
    /// </summary>
    public ITopicDbService Topics { get; }

    /// <summary>
    /// The service for interacting with the <see cref="TopicGroup"/> db objects.
    /// </summary>
    public ITopicGroupDbService Groups { get; }

    /// <summary>
    /// The service for interacting with the <see cref="TopicGroupMap"/> db objects.
    /// </summary>
    public ITopicGroupMapDbService GroupMaps { get; }

    /// <summary>
    /// The service for interacting with the <see cref="TopicGroupSubscription"/> db objects.
    /// </summary>
    public ITopicGroupSubscriptionDbService GroupSubscriptions { get; }

    /// <summary>
    /// The service for interacting with the <see cref="TopicSubscription"/> db objects.
    /// </summary>
    public ITopicSubscriptionDbService Subscriptions { get; }

    /// <summary>
    /// The service for interacting with the <see cref="Models.History"/> db objects.
    /// </summary>
    public IHistoryDbService History { get; }

    /// <summary>
    /// The implementation of the <see cref="IDbService"/>
    /// </summary>
    /// <param name="applications">The service for interacting with the <see cref="Application"/> db objects.</param>
    /// <param name="deviceTokens">The service for interacting with the <see cref="DeviceToken"/> db objects.</param>
    /// <param name="topics">The service for interacting with the <see cref="Topic"/> db objects.</param>
    /// <param name="groups">The service for interacting with the <see cref="TopicGroup"/> db objects.</param>
    /// <param name="groupMaps">The service for interacting with the <see cref="TopicGroupMap"/> db objects.</param>
    /// <param name="groupSubscriptions">The service for interacting with the <see cref="TopicGroupSubscription"/> db objects.</param>
    /// <param name="subscriptions">The service for interacting with the <see cref="TopicSubscription"/> db objects.</param>
    /// <param name="history">The service for interacting with the <see cref="Models.History"/> db objects.</param>
    public DbService(
        IApplicationDbService applications,
        IDeviceTokenDbService deviceTokens,
        ITopicDbService topics,
        ITopicGroupDbService groups,
        ITopicGroupMapDbService groupMaps, 
        ITopicGroupSubscriptionDbService groupSubscriptions, 
        ITopicSubscriptionDbService subscriptions, 
        IHistoryDbService history)
    {
        Applications = applications;
        DeviceTokens = deviceTokens;
        Topics = topics;
        Groups = groups;
        GroupMaps = groupMaps;
        GroupSubscriptions = groupSubscriptions;
        Subscriptions = subscriptions;
        History = history;
    }
}
