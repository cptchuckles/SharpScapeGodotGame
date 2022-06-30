using Godot;
using System;
using System.Text;

public class MPServerCrypto
{
    private Crypto _crypto = new Crypto();
    private CryptoKey _rsaPrivateKey = new CryptoKey();

    public MPServerCrypto()
    {
        _rsaPrivateKey.Load("res://server/.rsa/private.pem", false);
    }

    public string Sign(string payload)
    {
        var bytes = Encoding.UTF8.GetBytes(payload);
        byte[] hashBytes;
        using (var ctx = new HashingContext())
        {
            ctx.Start(HashingContext.HashType.Sha256);
            ctx.Update(bytes);
            hashBytes = ctx.Finish();
        }

        var signature = _crypto.Sign(HashingContext.HashType.Sha256, hashBytes, _rsaPrivateKey);
        return Convert.ToBase64String(signature);
    }
}
