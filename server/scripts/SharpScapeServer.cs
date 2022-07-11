using Godot;
using System;
using System.Linq;
using SharpScape.Game.Dto;
using SharpScape.Game.Services;
using ClientsById = System.Collections.Generic.Dictionary<int, Godot.WebSocketPeer>;
using PlayersById = System.Collections.Generic.Dictionary<int, SharpScape.Game.Dto.PlayerInfo>;
using System.Text;

public class SharpScapeServer : NetworkServiceNode
{
    [Signal] delegate void WriteLog(string msg);
    [Signal] delegate void PlayerLoginEvent(string playerInfo);
    [Signal] delegate void PlayerLogoutEvent(string playerInfo);

    private WebSocketServer _server;
    private ClientsById _clients = new ClientsById();
    private PlayersById _players = new PlayersById();
    private WebSocketPeer.WriteMode _writeMode = WebSocketPeer.WriteMode.Text;
    private MPServerCrypto _crypto = new MPServerCrypto();

    public SharpScapeServer()
    {
        _server = new WebSocketServer();
        if (_crypto.SslCert != null)
        {
            _server.PrivateKey = _crypto.RsaKey;
            _server.SslCertificate = _crypto.SslCert;
        }
        _server.Connect("client_connected", this, nameof(_ClientConnected));
        _server.Connect("client_disconnected", this, nameof(_ClientDisconnected));
        _server.Connect("client_close_request", this, nameof(_ClientCloseRequest));
        _server.Connect("data_received", this, nameof(_ClientReceive));
        Connect(nameof(WriteLog), this, nameof(_OnWriteLog));
    }

    private void _OnWriteLog(string msg)
    {
        GD.Print(msg);
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
        EmitSignal(nameof(WriteLog), $"Client {id} close code: {code}, reason: {reason}");
        Broadcast(Utils.ToJson(new MessageDto(MessageEvent.Logout, reason, id)));
    }

    public void _ClientConnected(int id, string protocol)
    {
        _clients.Add(id,_server.GetPeer(id));
        _clients[id].SetWriteMode(_writeMode);
        EmitSignal(nameof(WriteLog), $"Client {id} connected with protocol {protocol}");
        SendData(id, Utils.ToJson(new MessageDto(MessageEvent.Identify, id.ToString())));
    }

    public void _ClientDisconnected(int id, bool clean = true)
    {
        EmitSignal(nameof(WriteLog), $"Client {id} disconnected. Was clean: {clean}");
        
        if(_clients.ContainsKey(id))
            _clients.Remove(id);

        if(_players.ContainsKey(id))
        {
            var playerInfo = Utils.ToJson(_players[id]);
            EmitSignal(nameof(PlayerLogoutEvent), playerInfo);
            Broadcast(Utils.ToJson(new MessageDto(MessageEvent.Logout, playerInfo, id)));
            TrySavePlayerInfo(_players[id]);
            _players.Remove(id);
        }
    }

    public void _ClientReceive(int id)
    {
        var packet = _server.GetPeer(id).GetPacket();
        var isString = _server.GetPeer(id).WasStringPacket();
        var packetText = (string) Utils.DecodeData(packet, isString);
        EmitSignal(nameof(WriteLog), $"Data from {id} BINARY: {!isString}: {packetText}");
        
        var incoming = Utils.FromJson<MessageDto>(packetText);
        if (incoming is null) return;
        string who = _players.ContainsKey(id)
            ? _players[id].UserInfo.Username
            : id.ToString();

        switch(incoming.Event)
        {
            case MessageEvent.Login:
            {
                TryAuthenticateClient(id, incoming.Data);
                return;
            }
            case MessageEvent.Message:
            {
                EmitSignal(nameof(WriteLog), $"<{who}> {incoming.Data}");
                break;
            }
            case MessageEvent.Movement:
            {
                var dest = (Vector2) GD.Bytes2Var(Convert.FromBase64String(incoming.Data));
                GD.Print($"{who} is moving to {dest.ToString()}");
                var player = _players[id].Avatar;
                if (IsInstanceValid(player))
                {
                    player.MoveTo(dest);
                }
                break;
            }
        }
        Broadcast(Utils.ToJson(new MessageDto(incoming.Event, incoming.Data, id)));
    }

    private MPServerMessageDto SignAndDate(string payload)
    {
        var timestamp = OS.GetSystemTimeSecs();
        return new MPServerMessageDto() {
            Payload = payload,
            Timestamp = (int)timestamp,
            Signature = _crypto.Sign($"{payload}.{timestamp.ToString()}")
        };
    }
    private void TrySavePlayerInfo(PlayerInfo playerInfo)
    {
        var playerSaveJson = Utils.ToJson(new PlayerSaveDto(playerInfo));
        var playerSaveString = Convert.ToBase64String(Encoding.UTF8.GetBytes(playerSaveJson));
        var message = SignAndDate(playerSaveString);
        var saveRequest = new MPServerHttp();
        AddChild(saveRequest);
        saveRequest.SavePlayerInfo(message);
    }
    private void TryAuthenticateClient(int clientId, string payload)
    {
        var message = SignAndDate(payload);
        var http = new MPServerHttp(clientId);
        http.Connect("ApiLoginSuccess", this, nameof(_OnApiLoginSuccess));
        http.Connect("ApiLoginFailure", this, nameof(_OnApiLoginFailure));
        AddChild(http);
        http.Authenticate(message);
    }
    private void _OnApiLoginSuccess(int clientId, string responseBody)
    {
        var playerInfo = Utils.FromJson<PlayerInfo>(responseBody);
        try
        {
            int loggedInPlayer = _players.Keys.First(k => _players[k].UserInfo.Id == playerInfo.UserInfo.Id);
            _server.DisconnectPeer(loggedInPlayer, 1011, "Killing duplicate login (ghost?)");
        }
        catch (InvalidOperationException)
        {
            // Player wasn't already logged in, continue
        }

        Broadcast(Utils.ToJson(new MessageDto(MessageEvent.Login, responseBody, clientId)));
        foreach (int id in _players.Keys)
        {
            var msg = Utils.ToJson(new MessageDto(MessageEvent.ListPlayer, Utils.ToJson<PlayerInfo>(_players[id]), id));
            _server.GetPeer(clientId).PutPacket(Utils.EncodeData(msg, _writeMode));
        }
        _players.Add(clientId, playerInfo);
        EmitSignal(nameof(PlayerLoginEvent), responseBody);
    }
    private void _OnApiLoginFailure(int clientId)
    {
        _server.DisconnectPeer(clientId, 1002, "Login attempt failed");
    }

    public void Broadcast(string data)
    {
        foreach(int id in _clients.Keys)
        {
            SendData(id, data);
        }
    }
    public void SendData(int clientId, string data)
    {
        _server.GetPeer(clientId).PutPacket(Utils.EncodeData(data, _writeMode));
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

    public override void _OnWorldLoadingComplete()
    {
        var world = GetTree().CurrentScene as World;
        if (world is null)
            throw new Exception("World is not world");

        EmitSignal(nameof(WriteLog), "Server has entered the world");

        Connect(nameof(PlayerLoginEvent), world, "SpawnGameAvatar");
        Connect(nameof(PlayerLogoutEvent), world, "DespawnGameAvatar");
    }
    public override void _OnWorldAvatarSpawned(GameAvatar who)
    {
        foreach (var key in _players.Keys)
        {
            if (_players[key].UserInfo.Id == who.UserId)
            {
                _players[key].Avatar = who;
                who.Connect("UpdateGlobalPosition", this, nameof(_OnPlayerUpdatePosition), new Godot.Collections.Array {key});
                return;
            }
        }
    }
    private void _OnPlayerUpdatePosition(Vector2 globalPosition, int key)
    {
        if (_players.ContainsKey(key))
        {
            _players[key].GlobalPositionX = globalPosition.x;
            _players[key].GlobalPositionY = globalPosition.y;
        }
    }
}
