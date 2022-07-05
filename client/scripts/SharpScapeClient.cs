using Godot;
using Array = Godot.Collections.Array;
using SharpScape.Game.Dto;
using PlayersById = System.Collections.Generic.Dictionary<int, SharpScape.Game.Dto.PlayerInfo>;

public class SharpScapeClient : Node
{
    [Signal] delegate void WriteLine(string what);

    public WebSocketClient Websocket;
    private WebSocketPeer.WriteMode _writeMode;

    private PlayersById _players = new PlayersById();

    public override void _Ready()
    {
        _writeMode = WebSocketPeer.WriteMode.Binary;
    }

    public SharpScapeClient()
    {
        Websocket = new WebSocketClient();
        Websocket.Connect("connection_established", this, nameof(_ClientConnected));
        Websocket.Connect("connection_error", this, nameof(_ConnectionError));
        Websocket.Connect("connection_closed", this, nameof(_ClientDisconnected));
        Websocket.Connect("server_close_request", this, nameof(_ClientCloseRequest));
        Websocket.Connect("data_received", this, nameof(_DataReceived));
        Websocket.Connect("connection_succeeded", this, nameof(_ClientConnected), new Array(){"multiplayer_protocol"});
        Websocket.Connect("connection_failed", this, nameof(_ClientConnectionFailed));
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

    public void _ClientConnectionFailed()
    {
        EmitSignal(nameof(WriteLine), $"Client connection has failed");
    }

    public void _DataReceived()
    {
        var packet = Websocket.GetPeer(1).GetPacket();
        var isString = Websocket.GetPeer(1).WasStringPacket();

        EmitSignal(nameof(WriteLine), $"Received data. BINARY: {!isString}");

        var packetText = (string) Utils.DecodeData(packet, isString);
        var incoming = Utils.FromJson<MessageDto>(packetText);
        if (incoming is null) return;
        string who = _players.ContainsKey(incoming.ClientId)
            ? _players[incoming.ClientId].UserInfo.Username
            : incoming.ClientId.ToString();

        switch(incoming.Event)
        {
            case MessageEvent.Login:
            {
                var player = Utils.FromJson<PlayerInfo>(incoming.Data);
                _players.Add(incoming.ClientId, player);
                EmitSignal(nameof(WriteLine), $"* {player.UserInfo.Username} logged in ({incoming.ClientId})");
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
                EmitSignal(nameof(WriteLine), $"<{who}> {incoming.Data}");
                break;
            }
            case MessageEvent.Logout:
            {
                EmitSignal(nameof(WriteLine), $"* {who} logged out ({incoming.Data})");
                if (_players.ContainsKey(incoming.ClientId)) _players.Remove(incoming.ClientId);
                break;
            }
            default:
            {
                EmitSignal(nameof(WriteLine), $"Received event: {Utils.ToJson(incoming)}");
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

    public void SetWriteMode(WebSocketPeer.WriteMode mode)
    {
        _writeMode = mode;
    }
}
