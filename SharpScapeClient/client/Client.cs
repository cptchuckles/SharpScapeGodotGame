using System;
using Godot;
using Dictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;

public class Client : Node
{
	public RichTextLabel _logDest;
	private WebSocketClient _client;
	private Utils _utils;
	private WebSocketPeer.WriteMode _writeMode;
	public int lastConnectedClient;
	public override void _Ready()
	{
		_logDest=GetParent().GetNode<RichTextLabel>("Panel/VBoxContainer/RichTextLabel");
		_utils=GetNode<Utils>("/root/Utils");
		_writeMode = WebSocketPeer.WriteMode.Binary;
		lastConnectedClient = 0;
	}
	public Client()
	{  
		_client = new WebSocketClient();
		_client.Connect("connection_established", this, nameof(_ClientConnected));
		_client.Connect("connection_error", this, nameof(_ConnectionError));
		_client.Connect("connection_closed", this, nameof(_ClientDisconnected));
		_client.Connect("server_close_request", this, nameof(_ClientCloseRequest));
		_client.Connect("data_received", this, nameof(_DataReceived));
		_client.Connect("connection_succeeded", this, nameof(_ClientConnected), new Array(){"multiplayer_protocol"});
		_client.Connect("connection_failed", this, nameof(_ClientDisconnected));
	}
	public void _ConnectionError()
	{
		_utils._Log(_logDest, $"Failed to connect.");
	}
	public void _ClientCloseRequest(string code, string reason)
	{  
		_utils._Log(_logDest, $"Close code: {code}, reason: {reason}");
	}
	public override void _ExitTree()
	{  
		_client.DisconnectFromHost(1001, "Bye bye!");
	}
	
	public override void _Process(float _delta)
	{  
		if(_client.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Disconnected)
		{
			return;
		}
		_client.Poll();
	}
	public void _ClientConnected(string protocol)
	{  
		_utils._Log(_logDest, $"Client just connected with protocol: {protocol}");
		_client.GetPeer(1).SetWriteMode(_writeMode);
	}
	public void _ClientDisconnected(bool clean=true)
	{  
		_utils._Log(_logDest, $"Client just disconnected. Was clean: {clean.ToString()}");
	}
	public void _DataReceived()
	{
		var packet = _client.GetPeer(1).GetPacket();
		var isString = _client.GetPeer(1).WasStringPacket();
		_utils._Log(_logDest, $"Received data. BINARY: {!isString}: {_utils.DecodeData(packet, isString)}");
	}
	public Error ConnectToUrl(string host, string[] protocols)
	{
		return _client.ConnectToUrl(host, protocols);
	}
	public void DisconnectFromHost()
	{  
		_client.DisconnectFromHost(1000, "Bye bye!");
	}
	public void SendData(string data)
	{  
		_client.GetPeer(1).SetWriteMode(_writeMode);
		_client.GetPeer(1).PutPacket(_utils.EncodeData(data, _writeMode));
	}
	public void SetWriteMode(WebSocketPeer.WriteMode mode)
	{  
		_writeMode = mode;
	}
}
