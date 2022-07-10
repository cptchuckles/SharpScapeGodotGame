using Godot;
using Newtonsoft.Json;
using SharpScape.Game.Dto;

public static class Utils
{
	public static string GetSharpScapeDomain(bool includeProto = false)
	{
		var proto = includeProto ? "https://" : string.Empty;

		if (OS.HasFeature("JavaScript"))
		{	// this is an HTML5 export
			var window = JavaScript.GetInterface("window");
			var location = (JavaScriptObject) window.Get("location");
			var parentWindow = (JavaScriptObject) window.Get("parent");
			var parentLocation = (JavaScriptObject) parentWindow.Get("location");

			var href = (string) location.Get("href");
			var parentHref = (string) parentLocation.Get("href");

			proto = (string) location.Get("protocol");

			if (href != parentHref)
			{	// we are inside an <iframe>
				if (includeProto) return $"{proto}//" + (string) parentLocation.Get("host");
				return (string) parentLocation.Get("host");
			}
			else if ((string) location.Get("hostname") == "localhost")
			{	// this is a toplevel test of an HTML5 export, probably being served on a different port than SharpScape
				if (includeProto) return "https://localhost:7193";
				return "localhost:7193";
			}
			else
			{	// this is a production toplevel HTML5 export
				if (includeProto) return $"{proto}//" + (string) location.Get("host");
				return (string) location.Get("host");
			}
		}

		var envDomain = OS.GetEnvironment("SHARPSCAPE_DOMAIN");
		var domain = envDomain.Length > 0
			? envDomain
			: "localhost:7193";
		return proto + domain;
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

	private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings() {
		DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
	};

	public static string ToJson<T>(T obj) where T : JsonSerializable
	{
		return JsonConvert.SerializeObject(obj, _jsonSettings);
	}

	public static T FromJson<T>(string json) where T : JsonSerializable
	{
		try
		{
			return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
		}
		catch (JsonSerializationException)
		{
			return null;
		}
	}
}
