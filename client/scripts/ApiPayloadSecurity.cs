using Godot;
using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Security;

namespace SharpScape.Game.Services
{
    public class ApiPayloadSecurity : ServiceNode
    {
        public ApiTransientKeyProvider keyProvider;

        public override async void _Ready()
        {
            keyProvider = this.GetTransient<ApiTransientKeyProvider>();
            await ToSignal(keyProvider, "KeyReady");
        }

        public UniqueSecret EncryptPayload(string payload)
        {
            if (keyProvider.TransientKey is null)
                throw new MissingFieldException("API transient key was not initialized");

            var rsaParams = (RsaKeyParameters) PublicKeyFactory.CreateKey(Convert.FromBase64String(keyProvider.TransientKey.X509Pub));
            var engine = new Pkcs1Encoding(new RsaEngine());
            engine.Init(true, rsaParams);
            var data = Encoding.UTF8.GetBytes(payload);
            var encryptedData = engine.ProcessBlock(data, 0, data.Length);

            QueueFree();

            return new UniqueSecret() {
                KeyId = keyProvider.TransientKey.KeyId,
                Payload = Convert.ToBase64String(encryptedData)
            };
        }
    }
}