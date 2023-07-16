using System.Security.Cryptography;

namespace CardboardBox.PushNotifications;

public interface IKeyGenService
{
    string GenerateKey();
}

public class KeyGenService : IKeyGenService
{
    public string GenerateKey()
    {
        using var aes = Aes.Create();
        aes.GenerateIV();
        aes.GenerateKey();

        var key = aes.Key.Concat(aes.IV);
        return Convert.ToBase64String(key.ToArray());
    }
}
