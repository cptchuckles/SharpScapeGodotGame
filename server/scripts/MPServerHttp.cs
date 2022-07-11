using Godot;
using SharpScape.Game.Dto;
using System;
using System.Text;

public class MPServerHttp : HTTPRequest
{
    [Signal] delegate void ApiLoginSuccess(int clientId, string gameAvatarInfoDto);
    [Signal] delegate void ApiLoginFailure(int clientId);

    public int ClientId;

    public MPServerHttp(int clientId = -1)
    {
        ClientId = clientId;
    }

    public void Authenticate(MPServerMessageDto mpServerMessage)
    {
        Connect("request_completed", this, nameof(_OnLoginHttpRequestCompleted));
        var err = Request($"https://{Utils.GetSharpScapeDomain()}/api/game/login",
            customHeaders: new[] {"Content-Type: application/json"},
            sslValidateDomain: false,
            method: HTTPClient.Method.Post,
            requestData: Utils.ToJson(mpServerMessage));

        if (err != Error.Ok)
        {
            GD.Print($"HTTP Request error: {err}");
        }
    }
    private void _OnLoginHttpRequestCompleted(int result, int responseCode, string[] headers, byte[] body)
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

    public void SavePlayerInfo(MPServerMessageDto mpServerMessage)
    {
        Connect("request_completed", this, nameof(_OnSaveHttpRequestCompleted));
        var err = Request($"https://{Utils.GetSharpScapeDomain()}/api/game/save",
            customHeaders: new[] {"Content-Type: application/json"},
            sslValidateDomain: false,
            method: HTTPClient.Method.Put,
            requestData: Utils.ToJson(mpServerMessage));
        
        if (err != Error.Ok)
        {
            GD.Print($"HTTP Request Error: {err}");
        }
    }
    private void _OnSaveHttpRequestCompleted(int result, int responseCode, string[] headers, byte[] body)
    {
        GD.Print($"Got Api response: {result} Response code: {responseCode}");
        if (200 <= responseCode && responseCode < 300)
        {
            GD.Print($"Saved player information successfully");
        }
        else
        {
            GD.Print($"Failed to save player information");
        }
        QueueFree();
    }
}