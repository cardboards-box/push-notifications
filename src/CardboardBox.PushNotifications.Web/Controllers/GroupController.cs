namespace CardboardBox.PushNotifications.Web.Controllers;

/// <summary>
/// Handles group mappings
/// </summary>
public class GroupController : ControllerAuthedBase
{
    private readonly IPushRollupService _rollup;

    /// <summary>
    /// Handles group mappings
    /// </summary>
    /// <param name="rollup">The service that handles all interactions</param>
    public GroupController(IPushRollupService rollup)
    {
        _rollup = rollup;
    }

    /// <summary>
    /// Maps a topic to a group
    /// </summary>
    /// <param name="resourceId">The resource ID that represents the group</param>
    /// <param name="topic">The topic to map the group to</param>
    /// <returns>The result of the request</returns>
    [HttpGet, Route("group/{resourceId}/map/{topic}")]
    [RequestResults, RequestResults(204), ExceptionResults(401), ExceptionResults(400), ExceptionResults(500)]
    public async Task<IActionResult> MapTopic([FromRoute] string resourceId, [FromRoute] string topic)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());

        var result = await _rollup.Groups.MapTopic(appId.Value, resourceId, topic);
        return StatusCode((int)result.Code, result);
    }

    /// <summary>
    /// Unmaps a topic from a group
    /// </summary>
    /// <param name="resourceId">The resource ID that represents the group</param>
    /// <param name="topic">The topic to unmap from the group</param>
    /// <returns>The result of the request</returns>
    [HttpDelete, Route("group/{resourceId}/map/{topic}")]
    [RequestResults, RequestResults(204), ExceptionResults(401), ExceptionResults(400), ExceptionResults(500)]
    public async Task<IActionResult> UnmapTopic([FromRoute] string resourceId, [FromRoute] string topic)
    {
        var appId = this.ApplicationId();
        if (appId == null) return Unauthorized(WasUnauthorized());

        var result = await _rollup.Groups.UnmapTopic(appId.Value, resourceId, topic);
        return StatusCode((int)result.Code, result);
    }
}
