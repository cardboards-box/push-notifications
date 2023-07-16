namespace CardboardBox.PushNotifications.Web.Controllers;

public class GroupController : ControllerAuthedBase
{
    private readonly IDbService _db;
    
    public GroupController(IDbService db)
    {
        _db = db;
    }


}
