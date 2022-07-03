using Godot;

public class Utils
{
	public static string GetSharpScapeDomain()
	{
		var domain = OS.GetEnvironment("SHARPSCAPE_DOMAIN");
		return domain.Length > 0
			? domain
			: "localhost:7193";
	}

	public static byte[] EncodeData(string data, WebSocketPeer.WriteMode mode)
	{
		if(mode == WebSocketPeer.WriteMode.Text)
		{
			return System.Text.Encoding.UTF8.GetBytes(data);
		}
		return GD.Var2Bytes(data);
	}

	public static object DecodeData(byte[] data, bool isString)
	{
		if(isString)
		{
			return System.Text.Encoding.UTF8.GetString(data);
		}
		return GD.Bytes2Var(data);
	}

	public static void Log(RichTextLabel printTarget, string msg)
	{
		GD.Print(msg);
		printTarget.AddText(GD.Str(msg) + "\n");
	}
}
