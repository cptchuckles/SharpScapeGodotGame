using Godot;
using SharpScape.Game.Dto;
using System;
using System.Text;

namespace SharpScape.Game.Services
{
    public class ApiTransientKeyProvider : ServiceNode
    {
        public class ApiTransientKey : JsonSerializable
        {
            public Guid KeyId { get; set; }
            public string X509Pub { get; set; }
        }

        [Signal] delegate void KeyReady();

        public HTTPRequest.Result RequestResult = HTTPRequest.Result.NoResponse;
        public HTTPClient.ResponseCode ResponseCode = HTTPClient.ResponseCode.ImATeapot;
        public ApiTransientKey TransientKey;

        private HTTPRequest _http = new HTTPRequest();

        public bool IsKeyReady()
        {
            return RequestResult == HTTPRequest.Result.Success && ResponseCode == HTTPClient.ResponseCode.Ok;
        }

        public override void _Ready()
        {
            AddChild(_http);
            _http.Connect("request_completed", this, "_OnHttpRequestCompleted");

            var domain = $"{Utils.GetSharpScapeDomain(includeProto: true)}/api/game/transientkey";
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
                var json = Encoding.UTF8.GetString(body);
                TransientKey = Utils.FromJson<ApiTransientKey>(json);
                GD.Print("Got API transient key successfully.");
                EmitSignal(nameof(KeyReady));
            }
            else
            {
                GD.Print($"Error fetching API public key: Server response {responseCode}");
            }

            _http.QueueFree();
        }
    }
}