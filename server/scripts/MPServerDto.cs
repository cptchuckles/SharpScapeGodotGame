using System;
using System.Text;

namespace SharpScape.Game.Dto
{
    public class MPServerMessageDto : JsonSerializable
    {
        public string Payload { get; set; }
        public int Timestamp { get; set; }
        public string Signature { get; set; }
    }

    public class PlayerSaveDto : JsonSerializable
    {
        public int UserId { get; set; }
        public float GlobalPositionX { get; set; }
        public float GlobalPositionY { get; set; }
        public PlayerSaveDto() {}
        public PlayerSaveDto(PlayerInfo playerInfo)
        {
            UserId = playerInfo.UserInfo.Id;
            GlobalPositionX = playerInfo.GlobalPositionX;
            GlobalPositionY = playerInfo.GlobalPositionY;
        }
    }
}