using Godot;
using System.Text;

public class HttpAuthentication : HTTPRequest
{
    [Signal] delegate void ApiLoginSuccess(int clientId, string gameAvatarInfoDto);
    [Signal] delegate void ApiLoginFailure(int clientId);

    public int ClientId;

    public HttpAuthentication(int clientId)
    {
        ClientId = clientId;
    }

    public override void _Ready()
    {
        Connect("request_completed", this, "_OnHttpRequestCompleted");
    }

    public void Authenticate(string payload)
    {
        var err = Request($"https://{Utils.GetSharpScapeDomain()}/api/game/login",
            customHeaders: new[] {"Content-Type: application/json"},
            sslValidateDomain: false,
            method: HTTPClient.Method.Post,
            requestData: payload);

        if (err != Error.Ok)
        {
            GD.Print($"Request error: {err}");
        }
    }

    private void _OnHttpRequestCompleted(int result, int responseCode, string[] headers, byte[] body)
    {
        GD.Print($"Got Api response: {result} Response code: {responseCode}");
        if (200 <= responseCode && responseCode < 300)
        {
            EmitSignal(nameof(ApiLoginSuccess), ClientId, Encoding.UTF8.GetString(body));
        }
        else
        {
            EmitSignal(nameof(ApiLoginFailure), ClientId);
        }
        QueueFree();
    }
}