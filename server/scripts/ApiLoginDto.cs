using System;

namespace SharpScape.Game.Dto
{
    public class ApiLoginDto : JsonSerializable
    {
        public Guid KeyId { get; set; }
        public string Payload { get; set; }
        public int Timestamp { get; set; }
        public string Signature { get; set; }
    }
}