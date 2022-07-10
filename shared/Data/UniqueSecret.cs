using System;
using SharpScape.Game.Dto;

public class UniqueSecret : JsonSerializable
{
    public Guid KeyId { get; set; }
    public string SecureKey { get; set; }
    public string Payload { get; set; }
}