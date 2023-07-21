namespace CardboardBox.PushNotifications.Database;

using Core;
using Models;
using Services;

/// <summary>
/// The dependency injection extensions for the database
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers all of the database services and models
    /// </summary>
    /// <param name="builder">The dependency resolver to register with</param>
    /// <returns>The dependency resolver for chaining</returns>
    public static IDependencyResolver AddDatabase(this IDependencyResolver builder)
    {
        builder
            .Model<Application>()
            .Model<DeviceToken>()
            .Model<Topic>()
            .Model<TopicGroup>()
            .Model<TopicGroupMap>()
            .Model<TopicGroupSubscription>()
            .Model<TopicSubscription>()
            .Model<History>();

        builder
            .Transient<IApplicationDbService, ApplicationDbService>()
            .Transient<IDeviceTokenDbService, DeviceTokenDbService>()
            .Transient<ITopicDbService, TopicDbService>()
            .Transient<ITopicGroupDbService, TopicGroupDbService>()
            .Transient<ITopicGroupMapDbService, TopicGroupMapDbService>()
            .Transient<ITopicGroupSubscriptionDbService, TopicGroupSubscriptionDbService>()
            .Transient<ITopicSubscriptionDbService, TopicSubscriptionDbService>()
            .Transient<IHistoryDbService, HistoryDbService>()
            
            .Transient<IDbService, DbService>();

        return builder;
    }
}