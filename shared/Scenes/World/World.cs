using Godot;
using SharpScape.Game.Dto;
using System;
using System.Linq;

public class World : Node2D
{
    [Signal] delegate void AvatarSpawned(GameAvatar who);

    private PackedScene _avatarScene;

    public override void _Ready()
    {
        _avatarScene = GD.Load<PackedScene>("res://shared/Scenes/GameAvatar/GameAvatar.tscn");
    }

    public GameAvatar SpawnGameAvatar(string playerInfo)
    {
        var player = Utils.FromJson<PlayerInfo>(playerInfo);
        var existingPlayer = GetTree().GetNodesInGroup("Players").OfType<GameAvatar>().FirstOrDefault(p => p.UserId == player.UserInfo.Id);
        if (IsInstanceValid(existingPlayer))
        {
            existingPlayer.QueueFree();
        }

        var avatar = _avatarScene.Instance() as GameAvatar;
        AddChild(avatar);
        avatar.UserId = player.UserInfo.Id;
        avatar.Username = player.UserInfo.Username;
        avatar.GlobalPosition = new Vector2(player.GlobalPositionX, player.GlobalPositionY);

        EmitSignal(nameof(AvatarSpawned), avatar);
        return avatar;
    }
    public void DespawnGameAvatar(string playerInfo)
    {
        var player = Utils.FromJson<PlayerInfo>(playerInfo);
        var existingPlayer = GetTree().GetNodesInGroup("Players").OfType<GameAvatar>().FirstOrDefault(p => p.UserId == player.UserInfo.Id);
        if (IsInstanceValid(existingPlayer))
        {
            existingPlayer.QueueFree();
        }
    }
}