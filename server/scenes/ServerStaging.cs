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
        GetTree().Connect("idle_frame", this, nameof(_DelayedReady), flags: (uint)ConnectFlags.Oneshot);
    }
    private void _DelayedReady()
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

        GetTree().ChangeScene("res://shared/Scenes/World/World.tscn");
    }
}