namespace CardboardBox.PushNotifications.Database;

using Services;

public interface IDbService
{
    IApplicationDbService Applications { get; }
    IDeviceTokenDbService DeviceTokens { get; }
    ITopicDbService Topics { get; }
    ITopicGroupDbService Groups { get; }
    ITopicGroupMapDbService GroupMaps { get; }
    ITopicGroupSubscriptionDbService GroupSubscriptions { get; }
    ITopicSubscriptionDbService Subscriptions { get; }
    IHistoryDbService History { get; }
}

public class DbService : IDbService
{
    public IApplicationDbService Applications { get; }
    public IDeviceTokenDbService DeviceTokens { get; }
    public ITopicDbService Topics { get; }
    public ITopicGroupDbService Groups { get; }
    public ITopicGroupMapDbService GroupMaps { get; }
    public ITopicGroupSubscriptionDbService GroupSubscriptions { get; }
    public ITopicSubscriptionDbService Subscriptions { get; }
    public IHistoryDbService History { get; }

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
