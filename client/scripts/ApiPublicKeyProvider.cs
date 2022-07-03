using Godot;
using System.Text;

public class ApiPublicKeyProvider : Node
{
    [Signal] delegate void PublicKeyReady();

    public string ApiPublicKey;
    public int RequestResult = (int)HTTPRequest.Result.NoResponse;
    public int ResponseCode = 0;

    private HTTPRequest _http = new HTTPRequest();

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
        if (result != (int)HTTPRequest.Result.Success)
        {
            GD.Print($"Error fetching API public key: {result}");
        }

        if (200 <= responseCode && responseCode < 300)
        {
            ApiPublicKey = Encoding.UTF8.GetString(body);
            GD.Print("Got API public key successfully.");
            EmitSignal(nameof(PublicKeyReady));
        }
        else
        {
            GD.Print($"Error fetching API public key: Server response {responseCode}");
        }

        RequestResult = result;
        ResponseCode = responseCode;
        _http.QueueFree();
    }
}