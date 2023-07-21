namespace CardboardBox.PushNotifications.Web.Controllers;

using Devices;

/// <summary>
/// Handles all interactions with <see cref="DeviceToken"/>s
/// </summary>
public class DeviceController : ControllerAuthedBase
{
    private readonly IPushRollupService _rollup;

    /// <summary>
    /// Handles all interactions with <see cref="DeviceToken"/>s
    /// </summary>
    /// <param name="rollup">The service that handles all interactions</param>
    public DeviceController(IPushRollupService rollup)
    {
        _rollup = rollup;
    }

    /// <summary>
    /// Adds a new device to the users profile.
    /// This also subscribes them to any topics or groups they are subscribed to on other devices.
    /// </summary>
    /// <param name="profileId">The user's unique identifier</param>
    /// <param name="request">The device information for the user</param>
    /// <returns>The status code of the device creation and subsequent subscription requests</returns>
    [HttpPost, Route("device/{profileId}")]
    [RequestResults, RequestResults(204), ExceptionResults(401), ExceptionResults(400), ExceptionResults(500)]
    public async Task<IActionResult> Create([FromRoute] string profileId, [FromBody] CreateUserDevice request)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());
        if (string.IsNullOrEmpty(profileId)) return BadRequest(WasBadRequest("Profile ID cannot be null"));

        var result = await _rollup.Devices.Add(appId.Value, profileId, request);
        return StatusCode((int)result.Code, result);
    }

    /// <summary>
    /// Gets all of the devices that a user has registered
    /// </summary>
    /// <param name="profileId">The user's unique identifier</param>
    /// <returns>A collection of all of the users devices scoped to the current application</returns>
    [HttpGet, Route("device/{profileId}")]
    [RequestResults<DeviceToken[]>, ExceptionResults(401), ExceptionResults(400)]
    public async Task<IActionResult> Get([FromRoute] string profileId)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());
        if (string.IsNullOrEmpty(profileId)) return BadRequest(WasBadRequest("Profile ID cannot be null"));

        var devices = await _rollup.Devices.Devices(appId.Value, profileId);
        return Ok(devices);
    }

    /// <summary>
    /// Deletes the given device
    /// </summary>
    /// <param name="deviceId">The ID of the device </param>
    /// <returns></returns>
    [HttpDelete, Route("device/{deviceId}")]
    [RequestResults, RequestResults(204), ExceptionResults(401), ExceptionResults(400), ExceptionResults(500)]
    public async Task<IActionResult> Get([FromRoute] Guid deviceId)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());

        var result = await _rollup.Devices.Remove(appId.Value, deviceId);
        return StatusCode((int)result.Code, result);
    }
}
