using Godot;
using Array = Godot.Collections.Array;
using SharpScape.Game.Dto;
using PlayersById = System.Collections.Generic.Dictionary<int, SharpScape.Game.Dto.PlayerInfo>;
using System;
using SharpScape.Game.Services;

public class SharpScapeClient : ServiceNode
{
    [Signal] delegate void WriteLog(string msg);
    [Signal] delegate void AuthenticationResult(bool success);
    [Signal] delegate void PlayerLoginEvent(string playerInfo);
    [Signal] delegate void PlayerLogoutEvent(string playerInfo);

    public int ClientId = -1;
    public WebSocketClient Websocket;
    private WebSocketPeer.WriteMode _writeMode;
    private bool _tryingAuthenticate = false;

    private PlayersById _players = new PlayersById();

    public override void _Ready()
    {
        _writeMode = WebSocketPeer.WriteMode.Binary;
    }

    public SharpScapeClient()
    {
        Connect(nameof(WriteLog), this, nameof(_OnWriteLog));
        Websocket = new WebSocketClient();
        Websocket.Connect("connection_established", this, nameof(_ClientConnected));
        Websocket.Connect("connection_error", this, nameof(_ConnectionError));
        Websocket.Connect("connection_closed", this, nameof(_ClientDisconnected));
        Websocket.Connect("server_close_request", this, nameof(_ServerCloseRequest));
        Websocket.Connect("data_received", this, nameof(_DataReceived));
        Websocket.Connect("connection_succeeded", this, nameof(_ClientConnected), new Array(){"multiplayer_protocol"});
        Websocket.Connect("connection_failed", this, nameof(_ClientConnectionFailed));
    }

    private void _OnWriteLog(string msg)
    {
        GD.Print(msg);
    }

    public void _ConnectionError()
    {
        EmitSignal(nameof(WriteLog), $"Failed to connect.");
    }

    public void _ServerCloseRequest(string code, string reason)
    {
        ClientId = -1;
        EmitSignal(nameof(WriteLog), $"Close code: {code}, reason: {reason}");
        if (_tryingAuthenticate)
        {
            EmitSignal(nameof(AuthenticationResult), false);
            _tryingAuthenticate = false;
        }
        else
        {
            GetTree().ChangeScene("res://client/scenes/MainLogin/MainLogin.tscn");
        }
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
        EmitSignal(nameof(WriteLog), $"Client just connected with protocol: {protocol}");
        Websocket.GetPeer(1).SetWriteMode(_writeMode);
    }

    public void _ClientDisconnected(bool clean=true)
    {
        EmitSignal(nameof(WriteLog), $"Client just disconnected. Was clean: {clean.ToString()}");
    }

    public void _ClientConnectionFailed()
    {
        EmitSignal(nameof(WriteLog), $"Client connection has failed");
    }

    public void _DataReceived()
    {
        var packet = Websocket.GetPeer(1).GetPacket();
        var isString = Websocket.GetPeer(1).WasStringPacket();

        EmitSignal(nameof(WriteLog), $"Received data. BINARY: {!isString}");

        var packetText = (string) Utils.DecodeData(packet, isString);
        var incoming = Utils.FromJson<MessageDto>(packetText);
        if (incoming is null) return;
        string who = _players.ContainsKey(incoming.ClientId)
            ? _players[incoming.ClientId].UserInfo.Username
            : incoming.ClientId.ToString();

        switch(incoming.Event)
        {
            case MessageEvent.Identify:
            {
                ClientId = Convert.ToInt32(incoming.Data);
                break;
            }
            case MessageEvent.Login:
            {
                var player = Utils.FromJson<PlayerInfo>(incoming.Data);
                _players.Add(incoming.ClientId, player);
                EmitSignal(nameof(WriteLog), $"* {player.UserInfo.Username} logged in ({incoming.ClientId})");
                if (_tryingAuthenticate && incoming.ClientId == ClientId)
                {
                    _tryingAuthenticate = false;
                    EmitSignal(nameof(AuthenticationResult), true);
                }
                EmitSignal(nameof(PlayerLoginEvent), incoming.Data);
                break;
            }
            case MessageEvent.ListPlayer:
            {
                if (_players.ContainsKey(incoming.ClientId))
                    break;
                var player = Utils.FromJson<PlayerInfo>(incoming.Data);
                _players.Add(incoming.ClientId, player);
                break;
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
                var player = _players[incoming.ClientId].Avatar;
                if (IsInstanceValid(player))
                {
                    player.MoveTo(dest);
                }
                break;
            }
            case MessageEvent.Logout:
            {
                EmitSignal(nameof(WriteLog), $"* {who} logged out");
                EmitSignal(nameof(PlayerLogoutEvent), incoming.Data);
                if (_players.ContainsKey(incoming.ClientId)) _players.Remove(incoming.ClientId);
                break;
            }
            default:
            {
                EmitSignal(nameof(WriteLog), $"Received event: {Utils.ToJson(incoming)}");
                break;
            }
        }
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
        Websocket.GetPeer(1).PutPacket(Utils.EncodeData(data, _writeMode));
    }

    public void TryAuthenticate(string data)
    {
        _tryingAuthenticate = true;
        _writeMode = WebSocketPeer.WriteMode.Text;
        SendData(data);
    }

    public void SetWriteMode(WebSocketPeer.WriteMode mode)
    {
        _writeMode = mode;
    }

    private void _OnWorldLoad()
    {
        var world = GetTree().CurrentScene as World;
        if (world is null)
            throw new Exception("World is not world");
        world.AddChild(GD.Load<PackedScene>("res://client/scenes/ClickInputHandler/ClickInputHandler.tscn").Instance() as ClickInputHandler);
        foreach (var id in _players.Keys)
        {
            _players[id].Avatar = world.SpawnGameAvatar(Utils.ToJson(_players[id]));
            if (id == ClientId)
                _players[id].Avatar.FocusMe();
        }
        Connect(nameof(PlayerLoginEvent), world, "SpawnGameAvatar");
        Connect(nameof(PlayerLogoutEvent), world, "DespawnGameAvatar");
        world.Connect("AvatarSpawned", this, nameof(_OnWorldAvatarSpawned));
    }
    private void _OnWorldAvatarSpawned(GameAvatar who)
    {
        foreach (var key in _players.Keys)
        {
            if (_players[key].UserInfo.Id == who.UserId)
            {
                _players[key].Avatar = who;
                return;
            }
        }
    }
}
