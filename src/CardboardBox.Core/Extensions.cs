namespace CardboardBox.Core;

/// <summary>
/// Helpeful methods for working with the dependency resolver
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Starts the <see cref="IDependencyResolver"/> for the given config and services
    /// </summary>
    /// <param name="services">The service collection to register to</param>
    /// <param name="config">The configuration of the application</param>
    /// <param name="configure">The action for resolving services</param>
    /// <returns></returns>
    public static Task AddServices(this IServiceCollection services,
        IConfiguration config,
        Action<IDependencyResolver> configure)
    {
        var bob = new DependencyResolver();
        configure(bob);
        return bob.Build(services, config);
    }

    /// <summary>
    /// Gets a required configuration variable
    /// </summary>
    /// <param name="config">The configuration instance to get the variable from</param>
    /// <param name="key">The key of the configuration variable</param>
    /// <returns>The value of the configuration variable</returns>
    /// <exception cref="ArgumentNullException">Thrown if the configuration variable does not exist</exception>
    public static string Required(this IConfiguration config, string key)
    {
        return config[key]
            ?? throw new ArgumentNullException(key, "Required setting is not present");
    }

    /// <summary>
    /// Gets an optional configuration variable
    /// </summary>
    /// <param name="config">The configuration instance to get the variable from</param>
    /// <param name="key">The key of the configuration variable</param>
    /// <param name="default">The default value if the variable does not exist</param>
    /// <returns>The value of the configuration variable or the <paramref name="default"/> value</returns>
    public static string Optional(this IConfiguration config, string key, string @default)
    {
        return config[key] ?? @default;
    }

    /// <summary>
    /// Gets an optional integer configuration variable
    /// </summary>
    /// <param name="config">The configuration instance to get the variable from</param>
    /// <param name="key">The key of the configuration variable</param>
    /// <param name="default">The default value if the variable does not exist</param>
    /// <returns>The value of the configuration variable or the <paramref name="default"/> value</returns>
    public static int OptionalInt(this IConfiguration config, string key, int @default)
    {
        return int.TryParse(config[key], out int value) ? value : @default;
    }
}
