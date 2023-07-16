using CardboardBox.Database.Mapping;

namespace CardboardBox.Core;

public interface IDependencyResolver
{
    IDependencyResolver AddServices(Func<IServiceCollection, Task> services);

    IDependencyResolver AddServices(Func<IServiceCollection, IConfiguration, Task> services);

    IDependencyResolver AddServices(Action<IServiceCollection, IConfiguration> services);

    IDependencyResolver AddServices(Action<IServiceCollection> services);

    IDependencyResolver Model<T>();

    IDependencyResolver JsonModel<T>(Func<T> @default);

    IDependencyResolver JsonModel<T>();

    IDependencyResolver Transient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;

    IDependencyResolver Singleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;

    IDependencyResolver Singleton<TService>(TService instance)
        where TService : class;

    IDependencyResolver Config(IConfiguration config);

    IDependencyResolver AppSettings(string filename = "appsettings.json", bool optional = false, bool reloadOnChange = true);
}

internal class DependencyResolver : IDependencyResolver
{
    private readonly List<Func<IServiceCollection, IConfiguration, Task>> _services = new();
    private readonly List<Action<IConventionBuilder>> _conventions = new();
    private readonly List<Action<ITypeMapBuilder>> _dbMapping = new();

    public IDependencyResolver AddServices(Func<IServiceCollection, IConfiguration, Task> services)
    {
        _services.Add(services);
        return this;
    }

    public IDependencyResolver AddServices(Func<IServiceCollection, Task> services)
    {
        return AddServices((s, _) => services(s));
    }

    public IDependencyResolver AddServices(Action<IServiceCollection, IConfiguration> services)
    {
        return AddServices((s, c) =>
        {
            services(s, c);
            return Task.CompletedTask;
        });
    }

    public IDependencyResolver AddServices(Action<IServiceCollection> services)
    {
        return AddServices((s, _) => services(s));
    }

    public IDependencyResolver Model<T>()
    {
        _conventions.Add(x => x.Entity<T>());
        return this;
    }

    public IDependencyResolver JsonModel<T>(Func<T> @default)
    {
        _dbMapping.Add(x => x.DefaultJsonHandler(@default));
        return this;
    }

    public IDependencyResolver JsonModel<T>() => JsonModel<T?>(() => default);

    public IDependencyResolver Transient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        return AddServices(x => x.AddTransient<TService, TImplementation>());
    }

    public IDependencyResolver Singleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        return AddServices(x => x.AddSingleton<TService, TImplementation>());
    }

    public IDependencyResolver Singleton<TService>(TService instance)
        where TService : class
    {
        return AddServices(x => x.AddSingleton(instance));
    }

    public IDependencyResolver Config(IConfiguration config)
    {
        return AddServices(x => x.AddSingleton(config));
    }

    public IDependencyResolver AppSettings(string filename = "appsettings.json", bool optional = false, bool reloadOnChange = true)
    {
        return AddServices(x => x.AddAppSettings(c =>
        {
            c.AddFile(filename, optional, reloadOnChange)
                .AddEnvironmentVariables();
        }));
    }

    public async Task RegisterServices(IServiceCollection services, IConfiguration config)
    {
        services
            .AddSerilog()
            .AddJson();

        foreach (var action in _services)
            await action(services, config);
    }

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

    public async Task Build(IServiceCollection services, IConfiguration config)
    {
        RegisterDatabase(services);
        await RegisterServices(services, config);
    }
}