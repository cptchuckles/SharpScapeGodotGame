using System;
using System.Linq;
using Newtonsoft.Json;

namespace SharpScape.Game.Dto
{
    public static class MessageEvent
    {
        public const string Login = "Login";
        public const string Logout = "Logout";
        public const string ListPlayer = "ListPlayer";
        public const string Message = "Message";
        public const string Movement = "Movement";
    }

    public class MessageDto : JsonSerializable
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
    }
}