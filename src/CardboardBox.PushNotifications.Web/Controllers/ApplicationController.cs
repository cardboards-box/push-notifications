namespace CardboardBox.PushNotifications.Web.Controllers;

public class ApplicationController : ControllerAuthedBase
{
    private readonly IDbService _db;
    private readonly IKeyGenService _keyGen;

    public ApplicationController(
        IDbService db,
        IKeyGenService keyGen)
    {
        _db = db;
        _keyGen = keyGen;
    }

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

    public record class ApplicationGetResponse(
        [property: JsonPropertyName("appId")] Guid? AppId,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("isAdmin")] bool IsAdmin);

    public record class ApplicationPostRequest(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("ownerIds")] string[] OwnerIds);

    public record class ApplicationPostResponse(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("key")] string Key);
}
