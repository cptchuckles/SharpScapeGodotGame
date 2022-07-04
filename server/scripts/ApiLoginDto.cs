using System;
using Newtonsoft.Json;

namespace SharpScape.Game.Dto
{
    public class ApiLoginDto : JsonSerializable
    {
        public string Payload { get; set; }
        public int Timestamp { get; set; }
        public string Signature { get; set; }
    }
}