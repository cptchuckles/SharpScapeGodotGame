using System;
using Godot;
using ClientsById = System.Collections.Generic.Dictionary<int,Godot.WebSocketPeer>;
using Array = Godot.Collections.Array;
using SharpScape.Game.Dto;
using Newtonsoft.Json;

public class SharpScapeServer : Node
{
    public RichTextLabel _logDest;
    public int lastConnectedClient;

    private Utils _utils;
    private WebSocketServer _server;
    private ClientsById _clients;
    private WebSocketPeer.WriteMode _writeMode;
    private MPServerCrypto _crypto = new MPServerCrypto();

    public SharpScapeServer()
    {
        _server = new WebSocketServer();
        _server.Connect("client_connected", this, nameof(_ClientConnected));
        _server.Connect("client_disconnected", this, nameof(_ClientDisconnected));
        _server.Connect("client_close_request", this, nameof(_ClientCloseRequest));
        _server.Connect("data_received", this, nameof(_ClientReceive));
    }

    public override void _Ready()
    {
        _logDest = GetParent().GetNode<RichTextLabel>("Panel/VBoxContainer/RichTextLabel");
        _utils=GetNode<Utils>("/root/Utils");
        _clients = new ClientsById(){};
        _writeMode = WebSocketPeer.WriteMode.Binary;
        lastConnectedClient = 0;
    }

    public override void _ExitTree()
    {
        _clients.Clear();
        _server.Stop();
    }

    public override void _Process(float _delta)
    {
        if(_server.IsListening())
        {
            _server.Poll();
        }
    }

    public void _ClientCloseRequest(int id, string code, string reason)
    {
        GD.Print(reason == "Bye bye!");
        _utils._Log(_logDest, $"Client {id} close code: {code}, reason: {reason}");
    }

    public void _ClientConnected(int id, string protocol)
    {
        _clients.Add(id,_server.GetPeer(id));
        _clients[id].SetWriteMode(_writeMode);
        lastConnectedClient = id;
        _utils._Log(_logDest, $"Client {id} connected with protocol {protocol}");
    }

    public void _ClientDisconnected(int id, bool clean = true)
    {
        _utils._Log(_logDest, $"Client {id} disconnected. Was clean: {clean}");
        if(_clients.ContainsKey(id))
        {
            _clients.Remove(id);
        }
    }

    public void _ClientReceive(int id)
    {
        var packet = _server.GetPeer(id).GetPacket();
        var isString = _server.GetPeer(id).WasStringPacket();
        _utils._Log(_logDest, $"Data from {id} BINARY: {!isString}: {_utils.DecodeData(packet, isString)}");
        if (isString)
        {
            var payloadJson = System.Text.Encoding.UTF8.GetString(packet);
            var msgObject = MessageDto.FromJson(payloadJson);
            switch(msgObject.Event)
            {
            case MessageEvent.Login:
                var timestamp = OS.GetSystemTimeSecs();
                var loginDto = new ApiLoginDto() {
                    Payload = msgObject.Data,
                    Timestamp = (int)timestamp,
                    Signature = _crypto.Sign($"{msgObject.Data}.{timestamp.ToString()}")
                };
                TryAuthenticateClient(id, loginDto.ToString());
                break;
            case MessageEvent.Message:
            default:
                SendData(new MessageDto(msgObject.Event, msgObject.Data, id).ToString());
                break;
            }
        }
    }

    private void TryAuthenticateClient(int clientId, string loginDto)
    {
        var http = new Http(clientId);
        http.Connect("ApiLoginSuccess", this, "_OnApiLoginSuccess");
        http.Connect("ApiLoginFailure", this, "_OnApiLoginFailure");
        AddChild(http);
        http.Authenticate(loginDto);
    }

    private void _OnApiLoginSuccess(int clientId, string responseBody)
    {
        SendData(new MessageDto(MessageEvent.Login, responseBody, clientId).ToString());
    }

    private void _OnApiLoginFailure(int clientId)
    {
        _server.DisconnectPeer(clientId, 1002, "Login attempt failed");
    }

    public void SendData(string data)
    {
        foreach(int id in _clients.Keys)
        {
            _server.GetPeer(id).PutPacket(_utils.EncodeData(data, _writeMode));
        }
    }

    public Error Listen(int port, string[] supportedProtocols)
    {
        return _server.Listen(port, supportedProtocols);
    }

    public void Stop()
    {
        _clients.Clear();
        _server.Stop();
    }

    public void SetWriteMode(WebSocketPeer.WriteMode mode)
    {
        _writeMode = mode;
        foreach(int id in _clients.Keys)
        {
            _clients[id].SetWriteMode(_writeMode);
        }
    }
}
