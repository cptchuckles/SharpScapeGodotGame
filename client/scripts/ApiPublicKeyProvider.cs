using Godot;
using System.Text;

namespace SharpScape.Game.Services
{
    public class ApiPublicKeyProvider : ServiceNode
    {
        [Signal] delegate void PublicKeyReady();

        public string ApiPublicKey;
        public HTTPRequest.Result RequestResult = HTTPRequest.Result.NoResponse;
        public HTTPClient.ResponseCode ResponseCode = HTTPClient.ResponseCode.ImATeapot;

        private HTTPRequest _http = new HTTPRequest();

        public bool IsKeyReady()
        {
            return RequestResult == HTTPRequest.Result.Success && ResponseCode == HTTPClient.ResponseCode.Ok;
        }

        public override void _Ready()
        {
            AddChild(_http);
            _http.Connect("request_completed", this, "_OnHttpRequestCompleted");

            var domain = $"https://{Utils.GetSharpScapeDomain()}/api/publickey";
            GD.Print($"Fetching API public key from {domain}");
            var err = _http.Request(domain, sslValidateDomain: false);
            if (err != Error.Ok)
            {
                GD.Print($"Couldn't request API public key: {err}");
            }
        }

        private void _OnHttpRequestCompleted(int result, int responseCode, string[] headers, byte[] body)
        {
            RequestResult = (HTTPRequest.Result) result;
            ResponseCode = (HTTPClient.ResponseCode) responseCode;

            if (RequestResult != HTTPRequest.Result.Success)
            {
                GD.Print($"Error fetching API public key: Got result {result}");
            }

            if (IsKeyReady())
            {
                ApiPublicKey = Encoding.UTF8.GetString(body);
                GD.Print("Got API public key successfully.");
                EmitSignal(nameof(PublicKeyReady));
            }
            else
            {
                GD.Print($"Error fetching API public key: Server response {responseCode}");
            }

            _http.QueueFree();
        }
    }
}