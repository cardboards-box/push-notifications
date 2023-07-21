using System.ComponentModel.DataAnnotations;

namespace CardboardBox.PushNotifications.Web.Controllers;

/// <summary>
/// Handles all interactions with <see cref="Application"/>s
/// </summary>
public class ApplicationController : ControllerAuthedBase
{
    private readonly IDbService _db;
    private readonly IKeyGenService _keyGen;

    /// <summary>
    /// Handles all interactions with <see cref="Application"/>s
    /// </summary>
    /// <param name="db">The service that handles database interactions</param>
    /// <param name="keyGen">The service that handles crypto key generations</param>
    public ApplicationController(
        IDbService db,
        IKeyGenService keyGen)
    {
        _db = db;
        _keyGen = keyGen;
    }

    /// <summary>
    /// Gets the current applications information
    /// </summary>
    /// <returns>The application information</returns>
    [HttpGet, Route("application")]
    [RequestResults<ApplicationGetResponse>, ExceptionResults(401)]
    public IActionResult Get()
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());

        var res = WasOk(new ApplicationGetResponse(
            appId,
            this.ApplicationName(),
            this.IsAdmin()));
        return Ok(res);
    }

    /// <summary>
    /// Creates a new application
    /// </summary>
    /// <param name="request">The application information</param>
    /// <returns>The results of the request</returns>
    [HttpPost, Route("application"), AdminRole]
    [RequestResults<ApplicationPostResponse>, ExceptionResults(401)]
    public async Task<IActionResult> Create([FromBody] ApplicationPostRequest request)
    {
        var key = _keyGen.GenerateKey();
        var app = new Application
        {
            Name = request.Name,
            Secret = key,
            IsAdmin = false,
            OwnerIds = request.OwnerIds
        };

        var id = await _db.Applications.Insert(app);
        var res = WasOk(new ApplicationPostResponse(id, key));
        return Ok(res);
    }

    /// <summary>
    /// Creates the default admin application if none exist
    /// </summary>
    /// <returns>The applications security key</returns>
    [HttpGet, Route("application/create-admin"), AllowAnonymous]
    [RequestResults<ApplicationPostResponse>, ExceptionResults(401)]
    public async Task<IActionResult> CreateAdmin()
    {
        var count = await _db.Applications.Count();
        if (count != 0) return Unauthorized(WasUnauthorized());

        var key = _keyGen.GenerateKey();
        var app = new Application
        {
            Name = "Admin Application",
            Secret = key,
            IsAdmin = true,
            OwnerIds = Array.Empty<string>()
        };

        var id = await _db.Applications.Insert(app);
        var res = WasOk(new ApplicationPostResponse(id, key));
        return Ok(res);
    }

    /// <summary>
    /// Represents the result of <see cref="Get"/>
    /// </summary>
    /// <param name="AppId">The ID of the application</param>
    /// <param name="Name">The name of the application</param>
    /// <param name="IsAdmin">Whether the application is an admin or not</param>
    public record class ApplicationGetResponse(
        [property: JsonPropertyName("appId")] Guid? AppId,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("isAdmin")] bool IsAdmin);

    /// <summary>
    /// Represents the information required to <see cref="Create(ApplicationPostRequest)"/> an application
    /// </summary>
    /// <param name="Name">The name of the application</param>
    /// <param name="OwnerIds">The owners of the application</param>
    public record class ApplicationPostRequest(
        [property: JsonPropertyName("name"), Required] string Name,
        [property: JsonPropertyName("ownerIds"), Required] string[] OwnerIds);

    /// <summary>
    /// Represents the result of a successful <see cref="Create(ApplicationPostRequest)"/> request
    /// </summary>
    /// <param name="Id">The ID of the application</param>
    /// <param name="Key">The security key of the application</param>
    public record class ApplicationPostResponse(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("key")] string Key);
}
