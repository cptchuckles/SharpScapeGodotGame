using Godot;
using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Encodings;

namespace SharpScape.Game.Services
{
    public class ApiPayloadSecurity : ServiceNode
    {
        private RsaKeyParameters _apiPublicKey;

        public override async void _Ready()
        {
            var keyProvider = this.GetSingleton<ApiPublicKeyProvider>();
            if (! keyProvider.IsKeyReady())
            {
                await ToSignal(keyProvider, "PublicKeyReady");
            }

            using (var stringReader = new StringReader(keyProvider.ApiPublicKey))
            {
                var pemReader = new PemReader(stringReader);
                _apiPublicKey = (RsaKeyParameters) pemReader.ReadObject();
            }
        }

        public string EncryptPayload(string payload)
        {
            if (_apiPublicKey is null)
                throw new MissingFieldException("API Public Key was not initialized");

            var engine = new Pkcs1Encoding(new RsaEngine());
            engine.Init(forEncryption: true, _apiPublicKey);

            var data = Encoding.UTF8.GetBytes(payload);
            var encryptedData = engine.ProcessBlock(data, 0, data.Length);

            return Convert.ToBase64String(encryptedData);
        }
    }
}