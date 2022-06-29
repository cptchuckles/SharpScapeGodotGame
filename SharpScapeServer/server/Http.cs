using Godot;
using System.Text;

public class Http : Node
{
    [Signal] delegate void ApiLoginSuccess(int clientId, string gameAvatarInfoDto);
    [Signal] delegate void ApiLoginFailure(int clientId);

    private HTTPRequest _request = new HTTPRequest();

    public int ClientId;

    public Http(int clientId)
    {
        ClientId = clientId;
    }

    public override void _Ready()
    {
        AddChild(_request);
        _request.Connect("request_completed", this, "_OnHttpRequestCompleted");
    }

    public void Authenticate(string payload)
    {
        var err = _request.Request("https://localhost:7193/api/game/login",
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