using Godot;
using System.Linq;
using SharpScape.Game;
using SharpScape.Game.Dto;
using SharpScape.Game.Services;

public class World : Node2D
{
    [Signal] delegate void AvatarSpawned(GameAvatar who);
    [Signal] delegate void WorldLoadingComplete();

    private PackedScene _avatarScene;

    public override void _Ready()
    {
        _avatarScene = GD.Load<PackedScene>("res://shared/Scenes/GameAvatar/GameAvatar.tscn");

        var _networkService = this.GetSingleton<NetworkServiceNode>();
        Connect(nameof(WorldLoadingComplete), _networkService, "_OnWorldLoadingComplete", flags: (uint)ConnectFlags.Oneshot);
        Connect(nameof(AvatarSpawned), _networkService, "_OnWorldAvatarSpawned");

        GetTree().Connect("idle_frame", this,
            method: "emit_signal",
            binds: new Godot.Collections.Array { nameof(WorldLoadingComplete) },
            flags: (uint)ConnectFlags.Oneshot);
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
        avatar.SetAnimatedSpriteFrames(player.SpriteName);

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