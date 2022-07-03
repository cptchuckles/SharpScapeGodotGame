using System;
using System.Linq;
using Newtonsoft.Json;

namespace SharpScape.Game.Dto
{
    public static class MessageEvent
    {
        public const string Message = "Message";
        public const string Movement = "Movement";
        public const string Login = "Login";
        public const string Logout = "Logout";
    }

    public class MessageDto
    {
        private string _event = MessageEvent.Message;
        public string Event
        {
            get => _event;
            set => _event = typeof(MessageEvent).GetFields().Any(f => (string)f.GetRawConstantValue() == value)
                    ? value
                    : throw new NotSupportedException("Event must be defined in SharpScape.Game.Dto.MessageEvent");
        }

        public int ClientId { get; set; }

        public string Data { get; set; }

        public MessageDto() {}
        public MessageDto(string messageEvent, string data, int clientId = 0)
        {
            Event = messageEvent;
            ClientId = clientId;
            Data = data;
        }

        // Json serialization stuff:

        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings() {
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
        };

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, _jsonSettings);
        }

        public static MessageDto FromJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<MessageDto>(json, _jsonSettings);
            }
            catch (JsonSerializationException)
            {
                return null;
            }
        }
    }
}