using System.Security.Cryptography;

namespace CardboardBox.PushNotifications;

/// <summary>
/// Service for generating keys
/// </summary>
public interface IKeyGenService
{
    /// <summary>
    /// Generates a cryptographically secure key
    /// </summary>
    /// <returns>The cryptographically secure generated key</returns>
    string GenerateKey();
}

/// <summary>
/// The implementation of the <see cref="IKeyGenService"/>
/// </summary>
public class KeyGenService : IKeyGenService
{
    /// <summary>
    /// Generates a cryptographically secure key
    /// </summary>
    /// <returns>The cryptographically secure generated key</returns>
    public string GenerateKey()
    {
        using var aes = Aes.Create();
        aes.GenerateIV();
        aes.GenerateKey();

        var key = aes.Key.Concat(aes.IV);
        return Convert.ToBase64String(key.ToArray());
    }
}
