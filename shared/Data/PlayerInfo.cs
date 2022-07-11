using Godot;
using Newtonsoft.Json;

namespace SharpScape.Game.Dto
{
    public class PlayerInfo : JsonSerializable
    {
        public class UserInfoDto
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Role { get; set; }
        }
        public UserInfoDto UserInfo { get; set; }
        public string SpriteName { get; set; }
        public float GlobalPositionX { get; set; }
        public float GlobalPositionY { get; set; }

        [JsonIgnore]
        public GameAvatar Avatar;
    }
}