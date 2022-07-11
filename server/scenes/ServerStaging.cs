using Godot;
using SharpScape.Game;
using System;

public class ServerStaging : Node
{
    [Export] private int _websocketPort = 8000;
    private SharpScapeServer _server;

    public override void _Ready()
    {
        try
        {
            int envPort = Convert.ToInt32(OS.GetEnvironment("WS_PORT"));
            _websocketPort = envPort;
        }
        catch (FormatException)
        {
            GD.Print($"WS_PORT not specified. Defaulting to {_websocketPort}");
        }
        GetParent().Connect("ready", this, nameof(_OnParentReady));
    }
    private void _OnParentReady()
    {
        string[] supportedProtocols = {"sharpScapeJson", "binary"};
        _server = this.GetSingleton<SharpScapeServer>();
        Error e = _server.Listen(_websocketPort, supportedProtocols);
        if (e == Error.Ok)
        {
            GD.Print($"Listening on port {_websocketPort}");
        }
        else
        {
            throw new OperationCanceledException($"Error listening on port {_websocketPort}: {e}");
        }

        GetTree().Connect("idle_frame", _server, "_OnWorldLoad", flags: (uint)ConnectFlags.Oneshot);
        GetTree().ChangeScene("res://shared/Scenes/World/World.tscn");
    }
}