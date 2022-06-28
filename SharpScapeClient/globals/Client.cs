using System;
using Godot;
using Dictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;

public class Client : Node
{
    [Signal] delegate void WriteLine(string what);
    public WebSocketClient Websocket;
    private Utils _utils;
    private WebSocketPeer.WriteMode _writeMode;
    public int lastConnectedClient;
    public override void _Ready()
    {
        _utils=GetNode<Utils>("/root/Utils");
        _writeMode = WebSocketPeer.WriteMode.Binary;
        lastConnectedClient = 0;
    }
    public Client()
    {  
        Websocket = new WebSocketClient();
        Websocket.Connect("connection_established", this, nameof(_ClientConnected));
        Websocket.Connect("connection_error", this, nameof(_ConnectionError));
        Websocket.Connect("connection_closed", this, nameof(_ClientDisconnected));
        Websocket.Connect("server_close_request", this, nameof(_ClientCloseRequest));
        Websocket.Connect("data_received", this, nameof(_DataReceived));
        Websocket.Connect("connection_succeeded", this, nameof(_ClientConnected), new Array(){"multiplayer_protocol"});
        Websocket.Connect("connection_failed", this, nameof(_ClientDisconnected));
    }
    public void _ConnectionError()
    {
        EmitSignal(nameof(WriteLine), $"Failed to connect.");
    }
    public void _ClientCloseRequest(string code, string reason)
    {  
        EmitSignal(nameof(WriteLine), $"Close code: {code}, reason: {reason}");
    }
    public override void _ExitTree()
    {  
        Websocket.DisconnectFromHost(1001, "Bye bye!");
    }
    
    public override void _Process(float _delta)
    {  
        if(Websocket.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Disconnected)
        {
            return;
        }
        Websocket.Poll();
    }
    public void _ClientConnected(string protocol)
    {  
        EmitSignal(nameof(WriteLine), $"Client just connected with protocol: {protocol}");
        Websocket.GetPeer(1).SetWriteMode(_writeMode);
    }
    public void _ClientDisconnected(bool clean=true)
    {  
        EmitSignal(nameof(WriteLine), $"Client just disconnected. Was clean: {clean.ToString()}");
    }
    public void _DataReceived()
    {
        var packet = Websocket.GetPeer(1).GetPacket();
        var isString = Websocket.GetPeer(1).WasStringPacket();
        EmitSignal(nameof(WriteLine), $"Received data. BINARY: {!isString}: {_utils.DecodeData(packet, isString)}");
    }
    public Error ConnectToUrl(string host, string[] protocols)
    {
        return Websocket.ConnectToUrl(host, protocols);
    }
    public void DisconnectFromHost()
    {  
        Websocket.DisconnectFromHost(1000, "Bye bye!");
    }
    public void SendData(string data)
    {  
        Websocket.GetPeer(1).SetWriteMode(_writeMode);
        Websocket.GetPeer(1).PutPacket(_utils.EncodeData(data, _writeMode));
    }
    public void SetWriteMode(WebSocketPeer.WriteMode mode)
    {  
        _writeMode = mode;
    }
}
