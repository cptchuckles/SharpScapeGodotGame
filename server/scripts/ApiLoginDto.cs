using System;
using Newtonsoft.Json;

namespace SharpScape.Game.Dto
{
    public class ApiLoginDto
    {
        public string Payload { get; set; }
        public int Timestamp { get; set; }
        public string Signature { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}