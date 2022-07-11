using Godot;
using SharpScape.Game;
using SharpScape.Game.Dto;
using System;

public class MainLogin : Control
{
    [Export]
    private string _websocketUrl = $"ws://localhost:8000/test/";

    private SharpScapeClient _client;
    private LoginModal _loginModal;

    public override void _Ready()
    {
        GetTree().Connect("idle_frame", this, nameof(_DelayedReady), flags: (uint)ConnectFlags.Oneshot);
        _loginModal = GetNode<LoginModal>("LoginModal");
        _loginModal.Connect("LoginPayloadReady", this, nameof(_OnLoginPayloadReady));
    }
    private void _DelayedReady()
    {
        _client = this.GetSingleton<SharpScapeClient>();
        _client.Websocket.Connect("connection_established", this, nameof(_OnWebsocketConnectionEstablished), flags: (uint)ConnectFlags.Oneshot);
        _client.Websocket.Connect("connection_error",
                                  target: _loginModal,
                                  method: "ErrorRetry",
                                  binds: new Godot.Collections.Array { "Websocket connection failure" },
                                  flags: (uint)ConnectFlags.Oneshot);
    }

    private void _OnLoginPayloadReady()
    {
        string[] supportedProtocols = {"sharpScapeJson", "binary"};
        _client.ConnectToUrl(_websocketUrl, supportedProtocols);
    }
    private void _OnWebsocketConnectionEstablished(string protocol)
    {
        _client.SetWriteMode(WebSocketPeer.WriteMode.Text);
        _client.TryAuthenticate(Utils.ToJson(new MessageDto(MessageEvent.Login, _loginModal.SecurePayload)));
        _client.Connect("AuthenticationResult", this, nameof(_OnClientAuthResult), flags: (uint)ConnectFlags.Oneshot);
    }
    private void _OnClientAuthResult(bool success)
    {
        if (success)
        {
            GetTree().ChangeScene("res://shared/Scenes/World/World.tscn");
        }
        else
        {
            _loginModal.ErrorRetry("Authentication failed. Try again");
        }
    }
}