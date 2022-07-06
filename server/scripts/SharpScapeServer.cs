using System;
using System.Linq;
using Godot;
using SharpScape.Game.Dto;
using ClientsById = System.Collections.Generic.Dictionary<int, Godot.WebSocketPeer>;
using PlayersById = System.Collections.Generic.Dictionary<int, SharpScape.Game.Dto.PlayerInfo>;

public class SharpScapeServer : Node
{
    public RichTextLabel _logDest;
    public int lastConnectedClient;

    private WebSocketServer _server;
    private ClientsById _clients = new ClientsById();
    private PlayersById _players = new PlayersById();
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
        _writeMode = WebSocketPeer.WriteMode.Binary;
        lastConnectedClient = 0;
    }

    public override void _ExitTree()
    {
        _clients.Clear();
        _players.Clear();
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
        Utils.Log(_logDest, $"Client {id} close code: {code}, reason: {reason}");
        SendData(Utils.ToJson(new MessageDto(MessageEvent.Logout, reason, id)));
    }

    public void _ClientConnected(int id, string protocol)
    {
        _clients.Add(id,_server.GetPeer(id));
        _clients[id].SetWriteMode(_writeMode);
        lastConnectedClient = id;
        Utils.Log(_logDest, $"Client {id} connected with protocol {protocol}");
    }

    public void _ClientDisconnected(int id, bool clean = true)
    {
        Utils.Log(_logDest, $"Client {id} disconnected. Was clean: {clean}");
        if(_clients.ContainsKey(id))
            _clients.Remove(id);
        if(_players.ContainsKey(id))
            _players.Remove(id);
    }

    public void _ClientReceive(int id)
    {
        var packet = _server.GetPeer(id).GetPacket();
        var isString = _server.GetPeer(id).WasStringPacket();
        Utils.Log(_logDest, $"Data from {id} BINARY: {!isString}: {Utils.DecodeData(packet, isString)}");
        if (isString)
        {
            var payloadJson = System.Text.Encoding.UTF8.GetString(packet);
            var msgObject = Utils.FromJson<MessageDto>(payloadJson);
            if (msgObject is null) return;

            switch(msgObject.Event)
            {
                case MessageEvent.Login:
                {
                    var uniqueSecret = Utils.FromJson<UniqueSecret>(msgObject.Data);
                    var timestamp = OS.GetSystemTimeSecs();
                    var loginDto = new ApiLoginDto() {
                        KeyId = uniqueSecret.KeyId,
                        Payload = uniqueSecret.Payload,
                        Timestamp = (int)timestamp,
                        Signature = _crypto.Sign($"{uniqueSecret.Payload}.{timestamp.ToString()}")
                    };
                    TryAuthenticateClient(id, Utils.ToJson(loginDto));
                    break;
                }
                default:
                {
                    SendData(Utils.ToJson(new MessageDto(msgObject.Event, msgObject.Data, id)));
                    break;
                }
            }
        }
    }

    private void TryAuthenticateClient(int clientId, string loginDto)
    {
        var http = new HttpAuthentication(clientId);
        http.Connect("ApiLoginSuccess", this, "_OnApiLoginSuccess");
        http.Connect("ApiLoginFailure", this, "_OnApiLoginFailure");
        AddChild(http);
        http.Authenticate(loginDto);
    }

    private void _OnApiLoginSuccess(int clientId, string responseBody)
    {
        var playerInfo = Utils.FromJson<PlayerInfo>(responseBody);
        try
        {
            int loggedInPlayer = _players.Keys.First(k => _players[k].UserInfo.Id == playerInfo.UserInfo.Id);
            _server.DisconnectPeer(loggedInPlayer, 1011, "Killing duplicate login (ghost?)");
            _clients.Remove(loggedInPlayer);
            _players.Remove(loggedInPlayer);
        }
        catch (InvalidOperationException) {} // Player wasn't already logged in, continue

        SendData(Utils.ToJson(new MessageDto(MessageEvent.Login, responseBody, clientId)));
        foreach (int id in _players.Keys)
        {
            var msg = Utils.ToJson(new MessageDto(MessageEvent.ListPlayer, Utils.ToJson<PlayerInfo>(_players[id]), id));
            _server.GetPeer(clientId).PutPacket(Utils.EncodeData(msg, _writeMode));
        }
        _players.Add(clientId, playerInfo);
    }

    private void _OnApiLoginFailure(int clientId)
    {
        _server.DisconnectPeer(clientId, 1002, "Login attempt failed");
    }

    public void SendData(string data)
    {
        foreach(int id in _clients.Keys)
        {
            _server.GetPeer(id).PutPacket(Utils.EncodeData(data, _writeMode));
        }
    }

    public Error Listen(int port, string[] supportedProtocols)
    {
        return _server.Listen(port, supportedProtocols);
    }

    public void Stop()
    {
        _clients.Clear();
        _players.Clear();
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
