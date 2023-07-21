using CardboardBox.Database.Mapping;

namespace CardboardBox.Core;

/// <summary>
/// An builder for interfacing with the dependency injection system.
/// </summary>
public interface IDependencyResolver
{
    /// <summary>
    /// Add the given services to the dependency injection system.
    /// </summary>
    /// <param name="services">The service actions</param>
    /// <returns>The current resolver for chaining</returns>
    IDependencyResolver AddServices(Func<IServiceCollection, Task> services);

    /// <summary>
    /// Add the given services to the dependency injection system.
    /// </summary>
    /// <param name="services">The service actions</param>
    /// <returns>The current resolver for chaining</returns>
    IDependencyResolver AddServices(Func<IServiceCollection, IConfiguration, Task> services);

    /// <summary>
    /// Add the given services to the dependency injection system.
    /// </summary>
    /// <param name="services">The service actions</param>
    /// <returns>The current resolver for chaining</returns>
    IDependencyResolver AddServices(Action<IServiceCollection, IConfiguration> services);

    /// <summary>
    /// Add the given services to the dependency injection system.
    /// </summary>
    /// <param name="services">The service actions</param>
    /// <returns>The current resolver for chaining</returns>
    IDependencyResolver AddServices(Action<IServiceCollection> services);

    /// <summary>
    /// Adds the given database object class to the dependency injection system.
    /// </summary>
    /// <typeparam name="T">The type of class to add</typeparam>
    /// <returns>The current resolver for chaining</returns>
    IDependencyResolver Model<T>();

    /// <summary>
    /// Adds the given object, that represents a database typed object, to the dependency injection system.
    /// </summary>
    /// <typeparam name="T">The type of class to add</typeparam>
    /// <param name="default">A resolver for default values for this type</param>
    /// <returns>The current resolver for chaining</returns>
    IDependencyResolver JsonModel<T>(Func<T> @default);

    /// <summary>
    /// Adds the given object, that represents a database typed object, to the dependency injection system.
    /// </summary>
    /// <typeparam name="T">The type of class to add</typeparam>
    /// <returns>The current resolver for chaining</returns>
    IDependencyResolver JsonModel<T>();

    /// <summary>
    /// Adds a transient service to the dependency injection system.
    /// </summary>
    /// <typeparam name="TService">The interface that represents the service</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation of the service</typeparam>
    /// <returns>The current resolver for chaining</returns>
    IDependencyResolver Transient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;

    /// <summary>
    /// Adds a singleton service to the dependency injection system.
    /// </summary>
    /// <typeparam name="TService">The interface that represents the service</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation of the service</typeparam>
    /// <returns>The current resolver for chaining</returns>
    IDependencyResolver Singleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;

    /// <summary>
    /// Adds a singleton service to the dependency injection system.
    /// </summary>
    /// <typeparam name="TService">The interface that represents the service</typeparam>
    /// <param name="instance">The instance of the service</param>
    /// <returns>The current resolver for chaining</returns>
    IDependencyResolver Singleton<TService>(TService instance)
        where TService : class;
}

internal class DependencyResolver : IDependencyResolver
{
    private readonly List<Func<IServiceCollection, IConfiguration, Task>> _services = new();
    private readonly List<Action<IConventionBuilder>> _conventions = new();
    private readonly List<Action<ITypeMapBuilder>> _dbMapping = new();

    /// <summary>
    /// Add the given services to the dependency injection system.
    /// </summary>
    /// <param name="services">The service actions</param>
    /// <returns>The current resolver for chaining</returns>
    public IDependencyResolver AddServices(Func<IServiceCollection, IConfiguration, Task> services)
    {
        _services.Add(services);
        return this;
    }

    /// <summary>
    /// Add the given services to the dependency injection system.
    /// </summary>
    /// <param name="services">The service actions</param>
    /// <returns>The current resolver for chaining</returns>
    public IDependencyResolver AddServices(Func<IServiceCollection, Task> services)
    {
        return AddServices((s, _) => services(s));
    }

    /// <summary>
    /// Add the given services to the dependency injection system.
    /// </summary>
    /// <param name="services">The service actions</param>
    /// <returns>The current resolver for chaining</returns>
    public IDependencyResolver AddServices(Action<IServiceCollection, IConfiguration> services)
    {
        return AddServices((s, c) =>
        {
            services(s, c);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Add the given services to the dependency injection system.
    /// </summary>
    /// <param name="services">The service actions</param>
    /// <returns>The current resolver for chaining</returns>
    public IDependencyResolver AddServices(Action<IServiceCollection> services)
    {
        return AddServices((s, _) => services(s));
    }

    /// <summary>
    /// Adds the given database object class to the dependency injection system.
    /// </summary>
    /// <typeparam name="T">The type of class to add</typeparam>
    /// <returns>The current resolver for chaining</returns>
    public IDependencyResolver Model<T>()
    {
        _conventions.Add(x => x.Entity<T>());
        return this;
    }

    /// <summary>
    /// Adds the given object, that represents a database typed object, to the dependency injection system.
    /// </summary>
    /// <typeparam name="T">The type of class to add</typeparam>
    /// <param name="default">A resolver for default values for this type</param>
    /// <returns>The current resolver for chaining</returns>
    public IDependencyResolver JsonModel<T>(Func<T> @default)
    {
        _dbMapping.Add(x => x.DefaultJsonHandler(@default));
        return this;
    }

    /// <summary>
    /// Adds the given object, that represents a database typed object, to the dependency injection system.
    /// </summary>
    /// <typeparam name="T">The type of class to add</typeparam>
    /// <returns>The current resolver for chaining</returns>
    public IDependencyResolver JsonModel<T>() => JsonModel<T?>(() => default);

    /// <summary>
    /// Adds a transient service to the dependency injection system.
    /// </summary>
    /// <typeparam name="TService">The interface that represents the service</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation of the service</typeparam>
    /// <returns>The current resolver for chaining</returns>
    public IDependencyResolver Transient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        return AddServices(x => x.AddTransient<TService, TImplementation>());
    }

    /// <summary>
    /// Adds a singleton service to the dependency injection system.
    /// </summary>
    /// <typeparam name="TService">The interface that represents the service</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation of the service</typeparam>
    /// <returns>The current resolver for chaining</returns>
    public IDependencyResolver Singleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        return AddServices(x => x.AddSingleton<TService, TImplementation>());
    }

    /// <summary>
    /// Adds a singleton service to the dependency injection system.
    /// </summary>
    /// <typeparam name="TService">The interface that represents the service</typeparam>
    /// <param name="instance">The instance of the service</param>
    /// <returns>The current resolver for chaining</returns>
    public IDependencyResolver Singleton<TService>(TService instance)
        where TService : class
    {
        return AddServices(x => x.AddSingleton(instance));
    }

    /// <summary>
    /// Registers some default services and runs all of the configuration actions
    /// </summary>
    /// <param name="services">The service collection to add to</param>
    /// <param name="config">The configuration instance</param>
    /// <returns></returns>
    public async Task RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services
            .AddSerilog()
            .AddJson();

        foreach (var action in _services)
            await action(services, config);
    }

    /// <summary>
    /// Register all of the database models and services
    /// </summary>
    /// <param name="services"></param>
    public void RegisterDatabase(IServiceCollection services)
    {
        static async Task ExecuteFiles(IDbConnection con, string extension)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
            if (!Directory.Exists(path)) return;

            var files = Directory.GetFiles(path, extension, SearchOption.AllDirectories)
                .Where(t => !t.ToLower().EndsWith(".onetime.sql"))
                .OrderBy(t => Path.GetFileName(t))
                .ToArray();

            if (files.Length <= 0) return;

            foreach (var file in files)
            {
                var context = await File.ReadAllTextAsync(file);
                await con.ExecuteAsync(context);
            }
        }

        services
            .AddSqlService(c =>
            {
                c.ConfigureGeneration(a => a.WithCamelCaseChange())
                 .ConfigureTypes(a =>
                 {
                     var conv = a.CamelCase();
                     foreach (var convention in _conventions)
                         convention(conv);

                     foreach (var mapping in _dbMapping)
                         mapping(a);
                 });

                c.AddPostgres<SqlConfig>(a => a.OnInit(con => ExecuteFiles(con, "*.sql")));
            });
    }

    /// <summary>
    /// Registers all of the services and configurations with the service provider
    /// </summary>
    /// <param name="services">The service collections to add to</param>
    /// <param name="config">The configuration to add</param>
    /// <returns></returns>
    public async Task Build(IServiceCollection services, IConfiguration config)
    {
        RegisterDatabase(services);
        await RegisterServices(services, config);
    }
}