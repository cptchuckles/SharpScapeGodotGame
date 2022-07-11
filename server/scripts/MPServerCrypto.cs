using Godot;
using System;
using System.Text;

public class MPServerCrypto
{
    private Crypto _crypto = new Crypto();
    public CryptoKey RsaKey = new CryptoKey();
    public X509Certificate SslCert;

    public MPServerCrypto()
    {
        using (var fs = new File())
        {
            var rsapath = fs.FileExists("./x509/sharpscape.key") ? "./x509/sharpscape.key" : "res://server/x509/sharpscape.key";
            RsaKey.Load(rsapath);
            if (fs.FileExists("./x509/sharpscape.crt"))
            {
                SslCert = new X509Certificate();
                SslCert.Load("./x509/sharpscape.crt");
            }
        }
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

        var signature = _crypto.Sign(HashingContext.HashType.Sha256, hashBytes, RsaKey);
        return Convert.ToBase64String(signature);
    }
}
