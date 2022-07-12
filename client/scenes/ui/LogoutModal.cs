using Godot;
using SharpScape.Game;
using System;

public class LogoutModal : PanelContainer
{
    private SharpScapeClient _client;

    public override void _Ready()
    {
        _client = this.GetSingleton<SharpScapeClient>();
    }

    public void _OnConfirmPressed()
    {
        _client.Websocket.DisconnectFromHost(1000, "Logging out");
        GetTree().ChangeScene("res://client/scenes/MainLogin/MainLogin.tscn");
    }

    public void _OnCancelPressed()
    {
        Owner.QueueFree();
    }
}
