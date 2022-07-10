using System;
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using RandomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator;

namespace SharpScape.Game.Services
{
    public class MPClientCrypto : ServiceNode
    {
        public ApiTransientKeyProvider KeyProvider;

        public override async void _Ready()
        {
            KeyProvider = this.GetTransient<ApiTransientKeyProvider>();
            await ToSignal(KeyProvider, "KeyReady");
        }

        public string EncryptPayload(string payload)
        {
            if (KeyProvider.TransientKey is null)
                throw new MissingFieldException("API transient key was not initialized");

            var data = Encoding.UTF8.GetBytes(payload);
            AesEncrypt(data, out byte[] securePayload, out byte[] aesKey);
            byte[] secureKey = RsaEncrypt(aesKey, KeyProvider.TransientKey.X509Pub);

            var uniqueSecret = new UniqueSecret() {
                KeyId = KeyProvider.TransientKey.KeyId,
                SecureKey = Convert.ToBase64String(secureKey),
                Payload = Convert.ToBase64String(securePayload)
            };

            QueueFree();

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Utils.ToJson(uniqueSecret)));
        }

        private void AesEncrypt(byte[] input, out byte[] output, out byte[] key)
        {
            key = new byte[256 / 8];
            byte[] iv = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key, 0, key.Length);
                rng.GetBytes(iv, 0, iv.Length);
            }

            var aesEngine = new AesEngine();

            var cbc = new CbcBlockCipher(aesEngine);
            var cipher = new PaddedBufferedBlockCipher(cbc);
            var cipherParameters = new ParametersWithIV(new KeyParameter(key), iv);
            cipher.Init(true, cipherParameters);

            output = new byte[cipher.GetOutputSize(input.Length) + iv.Length];
            System.Buffer.BlockCopy(iv, 0, output, 0, iv.Length);

            int len = cipher.ProcessBytes(input, 0, input.Length, output, iv.Length);
            cipher.DoFinal(output, len + iv.Length);
        }

        private byte[] RsaEncrypt(byte[] input, string x509pub)
        {
            var rsaParams = (RsaKeyParameters) PublicKeyFactory.CreateKey(Convert.FromBase64String(x509pub));
            var engine = new Pkcs1Encoding(new RsaEngine());
            engine.Init(true, rsaParams);
            return engine.ProcessBlock(input, 0, input.Length);
        }
    }
}