using Godot;

public class Utils
{
	public static string GetSharpScapeDomain()
	{
		if (OS.HasFeature("JavaScript"))
		{	// this is an HTML5 export
			var parentLocation = (string) JavaScript.Eval("window.parent.location.href");
			var location = (string) JavaScript.Eval("window.location.href");
			if (parentLocation != location)
			{	// we are inside an <iframe>
				return (string) JavaScript.Eval("window.parent.location.host");
			}
			else {
				return "localhost:7193";
			}
		}
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
