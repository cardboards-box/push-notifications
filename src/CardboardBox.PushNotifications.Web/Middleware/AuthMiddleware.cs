using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace CardboardBox.PushNotifications.Web.Middleware;

using Database;

public class AuthMiddlewareOptions : AuthenticationSchemeOptions { }

public class AuthMiddleware : AuthenticationHandler<AuthMiddlewareOptions>
{
    public const string SCHEMA = "cba-push-notis";
    public readonly string[] Headers = new[] { "authorization", "x-api-key", "access-token" };
    public readonly string[] Prefixes = new[] { "bearer", "api-key" };

    private readonly IDbService _db;
    private readonly ILogger _logger;

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

    public string CleanPrefixes(string token)
    {
        foreach(var prefix in Prefixes)
            if (token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return token[prefix.Length..].Trim();

        return token;
    }

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
