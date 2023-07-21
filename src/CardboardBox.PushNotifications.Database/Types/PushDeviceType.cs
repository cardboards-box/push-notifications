namespace CardboardBox.PushNotifications.Database.Types;

/// <summary>
/// The type of device
/// </summary>
public enum PushDeviceType
{
    /// <summary>
    /// Device type is known, and not supported
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Android mobile app (Not supported yet)
    /// </summary>
    Android = 1,
    /// <summary>
    /// iOS mobile app (Not supported yet)
    /// </summary>
    iOS = 2,
    /// <summary>
    /// Browser / PWA app
    /// </summary>
    Web = 3
}
