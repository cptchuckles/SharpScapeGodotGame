using System;
using Godot;

public class Utils : Node
{
	public static string SharpScapeDomain = "localhost:7193";

	public Utils()
	{
		var domain = OS.GetEnvironment("SHARPSCAPE_DOMAIN");
		if (domain.Length > 0)
			SharpScapeDomain = domain;

        GD.Print($"SharpScape Domain is set to {SharpScapeDomain}");
	}

	public byte[] EncodeData(string data, WebSocketPeer.WriteMode mode)
	{
		if(mode == WebSocketPeer.WriteMode.Text)
		{
			return System.Text.Encoding.UTF8.GetBytes(data);
		}
		return GD.Var2Bytes(data);
	}
	public object DecodeData(byte[] data, bool isString)
	{
		if(isString)
		{
			return System.Text.Encoding.UTF8.GetString(data);
		}
		return GD.Bytes2Var(data);
	}
	public void Log(RichTextLabel node, string msg)
	{
		GD.Print(msg);
		node.AddText(GD.Str(msg) + "\n");
	}
}
