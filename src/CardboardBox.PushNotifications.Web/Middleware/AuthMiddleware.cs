using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace CardboardBox.PushNotifications.Web.Middleware;

using Database;

/// <summary>
/// The middleware options for the custom authentication scheme
/// </summary>
public class AuthMiddlewareOptions : AuthenticationSchemeOptions { }

/// <summary>
/// The custom authentication middleware scheme
/// </summary>
public class AuthMiddleware : AuthenticationHandler<AuthMiddlewareOptions>
{
    /// <summary>
    /// The name of the custom scheme
    /// </summary>
    public const string SCHEMA = "cba-push-notis";
    /// <summary>
    /// The headers that will be checked for the authentication key
    /// </summary>
    public readonly string[] Headers = new[] { "authorization", "x-api-key", "access-token" };
    /// <summary>
    /// The prefixes that will be stripped from the authentication key
    /// </summary>
    public readonly string[] Prefixes = new[] { "bearer", "api-key" };

    private readonly IDbService _db;
    private readonly ILogger _logger;

    /// <summary>
    /// The custom authentication middleware scheme
    /// </summary>
    /// <param name="options">The options for the middleware</param>
    /// <param name="factory">The logger factory</param>
    /// <param name="encoder">The encoding to use for authentication middleware</param>
    /// <param name="clock">The system clock (not sure what it's used for)</param>
    /// <param name="db">The service that handles interaction with the database</param>
    /// <param name="logger">The service that handles logging</param>
    public AuthMiddleware(
        IOptionsMonitor<AuthMiddlewareOptions> options, 
        ILoggerFactory factory, 
        UrlEncoder encoder, 
        ISystemClock clock,
        IDbService db,
        ILogger<AuthMiddleware> logger) : base(options, factory, encoder, clock)
    {
        _db = db;
        _logger = logger;
    }
    
    /// <summary>
    /// Handle the authentication request
    /// </summary>
    /// <returns>The results of the authentication request</returns>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var key = GetKey();
            if (string.IsNullOrEmpty(key)) 
                return AuthenticateResult.NoResult();

            return await Authenticate(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while attempting authentication");
            return AuthenticateResult.Fail(ex);
        }
    }

    /// <summary>
    /// Handles authentication for the given key
    /// </summary>
    /// <param name="key">The security key of the application or user</param>
    /// <returns>The results of the authentication request</returns>
    public async Task<AuthenticateResult> Authenticate(string key)
    {
        var application = await _db.Applications.GetByKey(key);
        if (application == null) return AuthenticateResult.Fail("Invalid key");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, application.Id.ToString()),
            new(ClaimTypes.Name, application.Name),
            new(ClaimTypes.Role, Roles.APPLICATION)
        };

        if (application.IsAdmin)
            claims.Add(new(ClaimTypes.Role, Roles.ADMIN));

        var id = new ClaimsIdentity(claims, SCHEMA);
        var prin = new ClaimsPrincipal(id);
        return AuthenticateResult.Success(new AuthenticationTicket(prin, SCHEMA));
    }

    /// <summary>
    /// Cleans all of the <see cref="Prefixes"/> from the token
    /// </summary>
    /// <param name="token">The token to clean the prefixes from</param>
    /// <returns>The cleaned token</returns>
    public string CleanPrefixes(string token)
    {
        foreach(var prefix in Prefixes)
            if (token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return token[prefix.Length..].Trim();

        return token;
    }

    /// <summary>
    /// Scans all of the <see cref="Headers"/> for the token
    /// </summary>
    /// <returns>The token or null if none was found</returns>
    public string? GetKey()
    {
        foreach (var header in Request.Headers)
            if (Headers.Contains(header.Key.ToLower()))
                return CleanPrefixes(header.Value.ToString());

        foreach(var query in Request.Query)
            if (Headers.Contains(query.Key.ToLower()))
                return CleanPrefixes(query.Value.ToString());

        return null;
    }
}
