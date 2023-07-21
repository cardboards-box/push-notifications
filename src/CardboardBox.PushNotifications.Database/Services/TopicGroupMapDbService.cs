namespace CardboardBox.PushNotifications.Database.Services;

using Models;

/// <summary>
/// The service for interacting with the <see cref="TopicGroupMap"/> db objects.
/// </summary>
public interface ITopicGroupMapDbService : IOrmMap<TopicGroupMap> { }

/// <summary>
/// The implementation of the <see cref="ITopicGroupMapDbService"/>
/// </summary>
public class TopicGroupMapDbService : OrmMap<TopicGroupMap>, ITopicGroupMapDbService
{
    /// <summary>
    /// The implementation of the <see cref="ITopicGroupMapDbService"/>
    /// </summary>
    /// <param name="query">The service to generate queries</param>
    /// <param name="sql">The service that executes SQL queries</param>
    public TopicGroupMapDbService(
        IQueryService query,
        ISqlService sql) : base(query, sql) { }
}