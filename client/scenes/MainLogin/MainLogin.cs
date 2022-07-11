using Godot;
using SharpScape.Game;
using SharpScape.Game.Dto;
using System;

public class MainLogin : Control
{
    [Export]
    private string _wsUrl = $"ws://localhost:8000/test/";

    private SharpScapeClient _client;
    private LoginModal _loginModal;

    public override void _Ready()
    {
        GetParent().Connect("ready", this, "_OnParentReady");
        _loginModal = GetNode<LoginModal>("LoginModal");
        _loginModal.Connect("LoginPayloadReady", this, nameof(_OnLoginPayloadReady));
    }
    private void _OnParentReady()
    {
        _client = this.GetSingleton<SharpScapeClient>();
    }

    private void _OnLoginPayloadReady()
    {
        string[] supportedProtocols = {"my-protocol2", "my-protocol", "binary"};
        _client.ConnectToUrl(_wsUrl, supportedProtocols);
        _client.Websocket.Connect("connection_established", this, nameof(_OnWebsocketConnectionEstablished), flags: (uint)ConnectFlags.Oneshot);
        _client.Websocket.Connect("connection_error",
                                  target: _loginModal,
                                  method: "ErrorRetry",
                                  binds: new Godot.Collections.Array { "Websocket connection failure" },
                                  flags: (uint)ConnectFlags.Oneshot);
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
            _loginModal.QueueFree();
            GetTree().Connect("idle_frame", _client, "_OnWorldLoad", flags: (uint)ConnectFlags.Oneshot);
            GetTree().ChangeScene("res://shared/Scenes/World/World.tscn");
        }
        else
        {
            _loginModal.ErrorRetry("Authentication failed. Try again");
        }
    }
}