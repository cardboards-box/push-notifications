namespace CardboardBox.Core;

public static class Extensions
{
    public static Task AddServices(this IServiceCollection services,
        IConfiguration config,
        Action<IDependencyResolver> configure)
    {
        var bob = new DependencyResolver();
        configure(bob);
        return bob.Build(services, config);
    }

    public static string Required(this IConfiguration config, string key)
    {
        return config[key]
            ?? throw new ArgumentNullException(key, "Required setting is not present");
    }

    public static string Optional(this IConfiguration config, string key, string @default)
    {
        return config[key] ?? @default;
    }

    public static int OptionalInt(this IConfiguration config, string key, int @default)
    {
        return int.TryParse(config[key], out int value) ? value : @default;
    }
}
